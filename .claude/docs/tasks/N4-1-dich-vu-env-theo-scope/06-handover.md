# N4-1 — ConfigEnv: dịch vụ biến môi trường theo phạm vi User/Machine + backup mọi thao tác · Bàn giao

- **Ngày:** 2026-11-16 · **Trạng thái:** ✅ xong (người dùng chạy tay checklist đạt 2026-11-16, gồm nhánh Machine) · **Sprint:** —

## 1. Tóm tắt
Tầng dịch vụ của ConfigEnv giờ làm việc với **hai phạm vi** (`Machine` = HKLM, `User` = HKCU) và với **mọi biến**, không chỉ 12 biến cốt lõi. Có hàm liệt kê/tạo/sửa/xóa/khôi phục theo scope; mọi sửa, xóa, khôi phục đều tự backup trước, và khôi phục dùng được cả cho biến đã bị xóa. File backup có thêm dòng `Scope=` (file cũ không có dòng này được coi là `Machine`). `Backups/` đã vào `.gitignore` vì backup chứa giá trị plaintext. Chưa đổi giao diện (thuộc N4-2); các hàm N1/N2 giữ nguyên hành vi Machine.

## 2. Giải thích luồng xử lý
Thuật ngữ: **scope** = nơi lưu biến (Machine = `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`, User = `HKCU\Environment`); **kind** = `REG_SZ`/`REG_EXPAND_SZ`, phải giữ nguyên khi ghi lại để `%...%` không bị mở rộng.

```
(N4-2 sẽ gọi) CustomEnvService.Update/Delete/RestoreVariable
  → kiểm quyền (RequiresElevation + IsElevated, chỉ Machine)
  → validate (key, ValidateValue, cấm \r\n, kind String/ExpandString)
  → EnvRegistryService.ReadRaw(scope, key)        (đọc thô + kind)
  → EnvBackupService.CreateBackup(scope, ...)     (Backups/Env/<Scope>_<key>_<ts>.txt; lỗi thì dừng)
  → EnvRegistryService.WriteRaw / DeleteRaw       (giữ kind)
  → EnvRegistryService.BroadcastSettingChange()   (WM_SETTINGCHANGE)
```
- `EnvRegistryService.cs`: `enum EnvScope`, overload `ReadRaw/WriteRaw/DeleteRaw/ListAll(scope, ...)`; hàm cũ ủy quyền `Machine`.
- `EnvBackupService.cs`: `CreateBackup(scope, ...)` ghi `Scope=`; `TryRead` thiếu `Scope` = Machine, giá trị lạ thì bỏ file; `ListBackups(EnvScope)` liệt kê mọi key của scope; `ListBackups(string key)` (hàm cũ) **chỉ trả backup Machine**.
- `CustomEnvService.cs`: `GetVariables`, `CreateVariable` (không backup, chặn trùng trong scope, chặn trùng default key chỉ ở Machine, `%` → `ExpandString`), `UpdateVariable` (backup `Edit`), `DeleteVariable` (chặn biến cốt lõi trước khi chạm registry, backup `Delete`), `RestoreVariable` (backup `BeforeRestore` nếu biến còn, bỏ qua nếu đã xóa), `RequiresElevation`.
- `RestoreCoreValue` (hàm cũ) từ chối entry `Scope != Machine`.

## 3. File đã thay đổi
| Đường dẫn | Vai trò |
|---|---|
| `SystemManagement/Views/ConfigEnv/EnvRegistryService.cs` | Đọc/ghi/xóa/liệt kê registry thô theo scope |
| `SystemManagement/Views/ConfigEnv/EnvBackupService.cs` | Backup có `Scope`, liệt kê theo scope |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` | Hàm dịch vụ theo scope, tự backup |
| `.gitignore` | Thêm `Backups/` |

## 4. Thay đổi DB
Không có.

## 5. Cách chạy và kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1` → 0 Error (đôi khi lỗi môi trường CS2012 "Insufficient system resources", chạy lại là hết).
- Test tự động: harness console ngoài repo, 46/46 đạt (xem mục 6).
- Kiểm tra thủ công: chưa có giao diện để thử, sẽ kiểm cùng N4-2. Nên thử riêng nhánh Machine khi chạy elevated (xem mục 7).

## 6. Kết quả kiểm chứng
- Test: PASS 46/46 vòng 2, gồm 7 ca hồi quy cho lỗi major (xem `04-test-report.md`). Registry chỉ dùng key `N4_TEST_*` ở HKCU, đã dọn sạch.
- Review: APPROVE vòng 2 (xem `05-review.md`). Vòng 1 REQUEST_CHANGES: `ListBackups(string key)` trả nhầm backup User, `RestoreCoreValue` không kiểm scope; đã sửa ở vòng sửa 1/2.
- Công cụ code: Antigravity (model yêu cầu `gemini-3.8-flash-medium`, server báo không khớp tên hiển thị nên model thực chạy chưa rõ), 2 lượt giao (code + 1 vòng sửa).

## 7. Rủi ro và lưu ý
- **Chưa kiểm chứng:** `Create/Update/Delete/RestoreVariable(Machine)` khi chạy elevated; phiên test không có quyền Administrator, chỉ xác nhận ném `UnauthorizedAccessException` trước khi backup/ghi.
- File backup chứa giá trị biến dạng plaintext (kể cả secret); đã gitignore nhưng vẫn nằm trên đĩa trong `Backups/Env/`.
- Backup cũ (N2-1) không có `Scope` được coi là Machine; `Scope=` giá trị lạ thì file bị bỏ qua.

## 8. Việc tồn đọng / đề xuất tiếp theo
- Minor 1: `EnvRegistryService.ListAll` có race nhỏ giữa `GetValueNames()` và `GetValueKind()` nếu biến bị xóa đúng lúc đó (có thể ném lỗi).
- Minor 2: `RestoreVariable` validate key chưa đầy đủ như `CreateVariable` (thiếu `=`, ký tự điều khiển, độ dài).
- Minor 3: logic validate key lặp giữa `CreateVariable` và `ValidateKey`.
- Verify Machine elevated: làm kèm N4-2 khi có UI, hoặc chạy harness trong PowerShell Administrator với key `N4_TEST_*`.
- Tiếp theo: N4-2 (giao diện hai lưới), cần điều chỉnh spec dùng `Frm_Base.RunSavingAsync`/`OnSavingStateChanged`.

## 9. Liên kết
- Yêu cầu: `../N4-configenv-hai-luoi-user-system/01-requirement.md` · Spec: `02-coding-spec.md`
