# N2-1 — ConfigEnv: mở khóa sửa biến cốt lõi + backup + khôi phục · Bàn giao

- **Ngày:** 2026-11-15 · **Trạng thái:** ✅ xong (người dùng chạy tay checklist đạt 2026-11-16) · **Sprint:** chưa gán

## 1. Tóm tắt
Trong tab **Tùy chỉnh** của ConfigEnv, 9 biến hệ thống cốt lõi không phải dạng danh sách (`ComSpec`, `SystemRoot`, `windir`, `SystemDrive`, `OS`, `TEMP`, `TMP`, `PROCESSOR_ARCHITECTURE`, `NUMBER_OF_PROCESSORS`) giờ sửa được value: tiến trình phải đang elevated và người dùng phải **gõ lại đúng tên key**. Trước mỗi lần ghi, giá trị cũ được **backup bắt buộc** vào `Backups/Env/` cạnh file exe; backup lỗi thì không ghi. Ghi qua registry Machine, giữ nguyên kiểu giá trị (`REG_EXPAND_SZ`/`REG_SZ`) và đọc giá trị thô (không mở rộng `%...%`), rồi broadcast `WM_SETTINGCHANGE`. Nút **Khôi phục** (mới) đưa một biến cốt lõi về một bản backup đã chọn. `Path`, `PATHEXT`, `PSModulePath` vẫn **khóa sửa** (cảnh báo "chưa hỗ trợ", làm ở N2-2) nhưng khôi phục được. Xóa biến cốt lõi vẫn bị chặn như N1. Không đổi DB, `system.config`, `CommonLibs`, không thêm package.

## 2. Giải thích luồng xử lý
**Thuật ngữ:** *biến cốt lõi* = 12 biến hệ thống trong `CustomEnvService.CoreProtectedKeys`. *Elevated* = tiến trình chạy quyền Administrator (`app.manifest` đã `requireAdministrator`). *Giá trị thô* = chuỗi registry chưa mở rộng `%...%` (đọc bằng `RegistryValueOptions.DoNotExpandEnvironmentNames`). *Backup* = file văn bản chứa key, kiểu và giá trị cũ.

```
Tab Tùy chỉnh (UC_ConfigEnvCustom)
 ├─ Sửa value ─ btn_Edit_Click ─ IsListKey? ─ có → cảnh báo, dừng
 │                               └ IsProtected (không list) → IsElevated → FrmEnvTypeKeyConfirm (gõ lại key)
 │                                 → GetCoreRawValue → FrmCustomEnvEditor(isCoreEdit) → BtnSave_Click
 │                                 → CustomEnvService.UpdateCoreValue
 │                                    ReadRaw → kiểm Kind → CreateBackup(Edit) → WriteRaw(giữ Kind) → BroadcastSettingChange
 └─ Khôi phục ─ btn_Restore_Click ─ IsProtected → IsElevated → FrmEnvRestorePicker (ListBackups)
                                  → FrmEnvTypeKeyConfirm → CustomEnvService.RestoreCoreValue
                                     validate bản backup → ReadRaw → kiểm Kind → CreateBackup(BeforeRestore) → WriteRaw → Broadcast
```

1. **Sửa.** `btn_Edit_Click` (`UC_ConfigEnvCustom.cs:161`): biến danh sách → cảnh báo rồi dừng (`:173`); biến cốt lõi khác → kiểm `IsElevated` → `FrmEnvTypeKeyConfirm` (`:189`, OK chỉ bật khi gõ đúng, `Ordinal`) → `GetCoreRawValue` (`:196`, `CustomEnvService.cs:72`) → mở `FrmCustomEnvEditor(isCoreEdit: true)` (`:210`, hiện giá trị thô, ẩn nhóm "Sinh giá trị") → `BtnSave_Click` (`:619`) gọi `UpdateCoreValue` (`:643`). Biến thường giữ nguyên luồng N1.
2. **Ghi.** `CustomEnvService.UpdateCoreValue` (`CustomEnvService.cs:88`): `IsProtected` → `IsElevated` → `ValidateValue` + cấm `\r`/`\n` → `EnvRegistryService.ReadRaw` (`EnvRegistryService.cs:40`; `null` = biến không còn → `KeyNotFoundException`) → kiểm `Kind` chỉ `String`/`ExpandString` → `EnvBackupService.CreateBackup` (`EnvBackupService.cs:32`) → `WriteRaw` (`EnvRegistryService.cs:61`) → `BroadcastSettingChange` (`:77`, `SendMessageTimeout` 5000 ms, lỗi broadcast không làm hỏng lần ghi). Hàm chấp nhận mọi biến cốt lõi kể cả list (N2-2 dùng lại); việc chặn list nằm ở UI.
3. **Khôi phục.** `btn_Restore_Click` (`UC_ConfigEnvCustom.cs:94`) → `FrmEnvRestorePicker` (`:122`, `ListBackups` mới nhất trước, `EnvBackupService.cs:158`) → `FrmEnvTypeKeyConfirm` (`:134`) → `RestoreCoreValue` (`:144`, `CustomEnvService.cs:129`). Hàm validate dữ liệu **lấy từ file** (key không trắng đầu/cuối, `ValidateValue`, cấm xuống dòng), `ReadRaw` hiện tại (chỉ khi `null` mới bỏ backup `BeforeRestore`; lỗi đọc thì dừng), kiểm `Kind`, backup, ghi đúng giá trị và kiểu từ file, broadcast.
4. **File backup.** `Backups/Env/<key>_<yyyyMMdd_HHmmss>.txt`, UTF-8, trùng giây thêm `_1`, `_2`. `TryRead` (`EnvBackupService.cs:71`) đọc `Value=` từ sau marker đến **hết file**, bỏ qua file hỏng/thiếu trường/`Kind` ngoài `String`/`ExpandString`/`CreatedAt` sai định dạng. Mẫu (giá trị giả):
   ```
   Key=TEMP
   Kind=ExpandString
   CreatedAt=2026-11-15T17:30:02
   Reason=Edit
   Value=%SystemRoot%\TEMP
   ```
5. **Lỗi.** `SecurityException`/`UnauthorizedAccessException` → thông báo cần Administrator; `KeyNotFoundException` → biến không còn, tải lại danh sách; lỗi khác (kể cả lỗi đọc registry/backup) → `FrmMain.ThrowError`. Thông báo chỉ nêu **key**, không nêu value; code mới không có lệnh log/console.
6. **DB:** không có.

## 3. File đã thay đổi
| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Vai trò |
|---|---|
| `EnvRegistryService.cs` (tạo) | Đọc/ghi giá trị thô + kiểu ở registry Machine, broadcast `WM_SETTINGCHANGE` |
| `EnvBackupService.cs` (tạo) | Tạo/liệt kê/đọc file backup trong `Backups/Env/` |
| `FrmEnvTypeKeyConfirm.cs` (tạo) | Hộp xác nhận mạnh (gõ lại tên key) |
| `FrmEnvRestorePicker.cs` (tạo) | Chọn bản backup để khôi phục |
| `CustomEnvService.cs` (sửa) | `IsListKey`, `IsElevated`, `GetCoreRawValue`, `UpdateCoreValue`, `RestoreCoreValue` |
| `UC_ConfigEnvCustom.cs` (sửa) | Luồng Sửa biến cốt lõi, `btn_Restore_Click`, `FrmCustomEnvEditor(isCoreEdit)` |
| `UC_ConfigEnvCustom.Designer.cs` (sửa) | Nút `btn_Restore`, đánh lại `TabIndex` toolbar |

## 4. Thay đổi DB
Không có.

## 5. Cách chạy và kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1` → **0 Error**, 913 Warning, không `WFO1000` (Claude chạy lại độc lập sau mỗi vòng code).
- Test tự động: repo **chưa có test project** (M1 xUnit chưa làm, không được thêm package). Tester dựng harness console tạm ngoài repo, vòng 1 **32/32**, vòng 2 **54/54** assertion cho `EnvBackupService`, `ReadRaw` (key thiếu), `IsListKey`/`IsProtected`, các nhánh từ chối của `UpdateCoreValue`/`RestoreCoreValue` (không phải biến cốt lõi, key có khoảng trắng), `TryRead` (nhiều dòng, CRLF/LF, rỗng, `CreatedAt` sai, culture `ar-SA`).
- **Kiểm tra thủ công (CHƯA CHẠY):** chạy `SystemManagement` bằng Administrator theo checklist ở **`04-test-report.md` (phần vòng 1, mục checklist chạy tay)**. Quan trọng: trước khi thử, chạy `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env.reg`; chỉ thử biến an toàn như `NUMBER_OF_PROCESSORS`, dùng nút Khôi phục để trả lại ngay, rồi đối chiếu với `env.reg`. Các AC cần chạy tay: AC2 và phần `ExpandString` của AC1, AC7 đến AC13 (hộp xác nhận, không elevated, sửa thật, khôi phục thật, list key chỉ cảnh báo, hồi quy N1).

## 6. Kết quả kiểm chứng
- Test: **PASS** cho phần chạy/đọc code được (vòng 2, 54/54, không bug); AC cần UI/Administrator **chưa chạy, không tính là PASS** (xem `04-test-report.md`).
- Review: **APPROVE** vòng 2, 0 blocker/major (xem `05-review.md`; vòng 1 có 1 major R5 đã sửa).
- Đã kiểm độc lập: diff đúng 7 file của spec; grep `Console./Log./Debug./Trace.` rỗng; không có `Backups/`/`Logs/` sinh ra trong repo.

## 7. Rủi ro và lưu ý
- **Chưa từng ghi registry thật bằng code này:** mọi nhánh sau bước kiểm elevated (ghi, khôi phục, broadcast) mới được xác nhận bằng đọc code. Chưa kiểm chứng `AppContext.BaseDirectory` khi `PublishSingleFile` (giả định trỏ thư mục chứa exe), cũng chưa kiểm bằng mắt Designer/hộp thoại.
- **File backup là văn bản thường, không mã hóa, cạnh exe:** `Path`/`TEMP` có thể lộ tên thư mục người dùng; app không tự xóa file backup.
- Giá trị backup có **xuống dòng** (hiếm) không khôi phục được vì `RestoreCoreValue` cấm; file sửa tay có newline cuối cũng bị từ chối an toàn với thông báo rõ.
- Sửa biến Machine ảnh hưởng toàn máy: tiến trình đang chạy chỉ nhận giá trị mới sau khi mở lại. `RunWithLoadingAsync` chạy đồng bộ trên luồng UI nên ghi registry và broadcast có thể làm UI đứng vài giây (cùng pattern N1, chấp nhận).
- Chỉ **hai vòng code**: vòng 1 (Antigravity, `gemini-3.8-flash-medium`) và vòng sửa 1 theo Review; model tham số được nhận (`warning: null`) nhưng không có cách xác nhận độc lập model thực chạy.

## 8. Việc tồn đọng / đề xuất tiếp theo
- **Chạy tay checklist** (mục 5) trên máy Windows có quyền Administrator, rồi mới đánh ✅ cho N2-1.
- **N2-2** (trình sửa từng mục cho `Path`/`PATHEXT`/`PSModulePath`) làm tiếp theo; nó dùng lại `UpdateCoreValue` (đã chấp nhận key danh sách), `FrmEnvTypeKeyConfirm`, backup/khôi phục của task này.
- Minor/nit chưa sửa (ngoài phạm vi, không chặn): `RestoreCoreValue` chưa tự kiểm `entry.Kind` (chỉ `TryRead` kiểm); backup có `Value=` rỗng vẫn được liệt kê; sắp xếp `_10` đứng trước `_2` khi quá 9 backup cùng giây; `btn_Restore_Click` dùng `object? sender` khác style các handler còn lại; chưa bọc `Task.Run` cho ghi registry.
- Khi có M1 (xUnit): chuyển các AC "Unit/harness (chờ M1)" thành test thật cho `EnvBackupService`/`CustomEnvService`.
- Thay đổi chưa commit (7 file code + docs); nhánh `feature/N1-configenv-env-tuy-chinh`, commit gốc `18bdd8e`. Cần bạn quyết định commit (có thể dùng skill `commit-helper`, gợi ý `feat(N2-1): add core env edit, backup and restore`).

## 9. Liên kết
- Yêu cầu: `../N2-configenv-sua-bien-loi/01-requirement.md` · Spec: `02-coding-spec.md` · Log: `03-implementation-log.md` · Test: `04-test-report.md` · Review: `05-review.md`
