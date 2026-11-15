# N4-2 — ConfigEnv: giao diện hai lưới System / User · Bàn giao

- **Ngày:** 2026-11-16 · **Trạng thái:** ✅ xong (người dùng chạy tay checklist đạt 2026-11-16) · **Sprint:** —

## 1. Tóm tắt
Tab **Tùy chỉnh** của ConfigEnv giờ có **hai lưới trên/dưới**: System (Machine, HKLM) và User (HKCU, tiêu đề ghi tên tài khoản đang chạy). Cả hai hiện **mọi** biến, kể cả 6 key `BaseRepo__*` mặc định. Tìm kiếm, Hiện/Ẩn giá trị, Làm mới áp cho cả hai lưới; Thêm/Sửa/Xóa/Khôi phục tác động lưới đang được chọn (có dấu ▶ ở tiêu đề). Mọi ghi đi qua hàm theo scope của N4-1 nên tự backup; Khôi phục chọn bản backup của scope đang chọn (mọi key, kể cả biến đã xóa).

## 2. Giải thích luồng xử lý
Thuật ngữ: **scope** = Machine hoặc User; **lưới đang chọn** = `_activeGrid`, cập nhật khi lưới nhận focus (`Enter`), mặc định System.

```
UC_ConfigEnvCustom (tab Tùy chỉnh)
  Load → RunWithLoadingAsync(Task.Run(GetVariables(Machine) + GetVariables(User)))
       → ApplyFilter đổ dgv_System / dgv_User (cùng bộ lọc, cùng cờ mask)
  Enter lưới → _activeGrid → scope
  Thêm  → FrmCustomEnvEditor(scope) → CreateVariable(scope)        (Frm_Base.RunSavingAsync)
  Sửa   → [cốt lõi: gõ lại key] → key danh sách? FrmEnvListEditor(key, raw, scope)
                                   : FrmCustomEnvEditor(...scope) → UpdateVariable(scope)   (backup Edit)
  Xóa   → hỏi Yes/No → DeleteVariable(scope)                       (backup Delete; cốt lõi bị chặn)
  Khôi phục → FrmEnvRestorePicker(scope) → gõ lại key → RestoreVariable(entry)   (backup BeforeRestore nếu biến còn)
```
- UC không kế thừa `Frm_Base` nên dùng thẳng `FrmMain.RunWithLoadingAsync(() => Task.Run(...))` và cờ `_isBusy` chống bấm lặp; các form dùng `RunSavingAsync`/`IsSaving`/`OnSavingStateChanged` của `Frm_Base`.
- Lỗi: `KeyNotFoundException` → báo + tải lại; `UnauthorizedAccessException`/`SecurityException` → báo cần Administrator (Machine) hoặc lỗi quyền chung (User); thông báo chỉ nêu key, không nêu value.

## 3. File đã thay đổi
| Đường dẫn | Vai trò |
|---|---|
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.Designer.cs` | `spc_Containers` (SplitContainer ngang) chứa `dgv_System`, `dgv_User`, `lbl_SystemTitle`, `lbl_UserTitle`, cột `col_Sys*`/`col_User*` |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` | Nạp/lọc hai lưới, `_activeGrid`, Add/Edit/Delete/Restore theo scope, `_isBusy` |
| `SystemManagement/Views/ConfigEnv/FrmCustomEnvEditor.cs` | Nhận `EnvScope`, gọi `CreateVariable`/`UpdateVariable` |
| `SystemManagement/Views/ConfigEnv/FrmEnvListEditor.cs` | Nhận `EnvScope`, lưu qua `UpdateVariable(scope, ...)` |
| `SystemManagement/Views/ConfigEnv/FrmEnvRestorePicker.cs` / `.Designer.cs` | Picker theo scope, mọi key, thêm cột `col_Key` |
| `.claude/docs/tasks/N4-2-.../02-coding-spec.md` | Cập nhật spec theo `Frm_Base` trước khi giao |

## 4. Thay đổi DB
Không có.

## 5. Cách chạy và kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1` → 0 Error, không `WFO1000`.
- Test tự động: harness WinForms ngoài repo (STA, reflection, backup giả, không đụng registry) 29/29 đạt.
- **Kiểm tra thủ công (người dùng chạy, Administrator):** theo checklist mục 5 của `04-test-report.md`. Trước khi thử nhớ `reg export` hai hive:
  `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env_machine.reg` và `reg export "HKCU\Environment" env_user.reg`. Chỉ thử bằng biến `N4_TEST_*`; với 6 key `BaseRepo__*` phải khôi phục lại ngay bằng nút Khôi phục.
  Cần xác nhận: lưới System/User đúng nội dung (AC2), tìm kiếm/ẩn giá trị/làm mới cả hai (AC3), thêm/sửa/xóa/khôi phục `N4_TEST_UI` ở từng lưới (AC4–AC5), biến cốt lõi và Machine không elevated (AC6), `BaseRepo__*` (AC7), thao tác đúng lưới đang chọn (AC8), không hồi quy (AC9), loader hiện và UI không đứng khi lưu (mẫu N6).

## 6. Kết quả kiểm chứng
- Test: PASS phần tự động (xem `04-test-report.md`). AC4–AC9 và phần thao tác thật của AC2/AC3/AC8 **chưa chạy**.
- Review: APPROVE vòng 1, 0 blocker/major, 5 minor (xem `05-review.md`).
- Công cụ code: Antigravity, 1 lượt giao, không cần vòng sửa; model yêu cầu `gemini-3.8-flash-medium` (chưa rõ model thực chạy).

## 7. Rủi ro và lưu ý
- Phụ thuộc N4-1 chưa verify nhánh Machine khi elevated; checklist thủ công ở trên sẽ kiểm luôn phần này.
- Sửa/xóa các biến Machine thật (đặc biệt `BaseRepo__*`, `Path`) có thể ảnh hưởng hệ thống; đã có backup nhưng vẫn nên `reg export` trước.
- File backup plaintext trong `Backups/Env/` (đã gitignore).

## 8. Việc tồn đọng / đề xuất tiếp theo
Minor từ review (không chặn):
1. `UpdateGridTitles` tạo `new Font` mỗi lần đổi lưới mà không Dispose.
2. Double-click không dùng `sender`, dựa vào `_activeGrid` (đúng vì `Enter` chạy trước).
3. `ReadRaw` và `ListBackups` còn chạy đồng bộ trên luồng UI.
4. Sửa biến thường ở Machine không kiểm elevated trước hộp cảnh báo (hỏi rồi mới báo lỗi quyền; kết quả vẫn đúng).
5. `_activeGrid ?? dgv_System` thừa.
Cùng minor của N4-1 (`ListAll` race, validate key khi khôi phục). Chưa có mã N mới; nếu muốn dọn các minor này thì đề xuất N7.

## 9. Liên kết
- Yêu cầu: `../N4-configenv-hai-luoi-user-system/01-requirement.md` · Spec: `02-coding-spec.md`
