# N4-2 · Implementation Log

- **Công cụ code:** Antigravity (MCP `use_antigravity`), model yêu cầu `gemini-3.8-flash-medium` (server từng cảnh báo không khớp tên hiển thị; chưa rõ model thực chạy). **Số vòng:** 1 (chưa có vòng sửa).
- **Commit gốc:** `a94d80e` (sau N4-1 `b70457a`). Build nền xanh trước khi giao.
- Spec được cập nhật trước khi giao (dòng "Cập nhật 2026-11-16" ở đầu): form dùng `Frm_Base.RunSavingAsync`/`OnSavingStateChanged`, riêng `UC_ConfigEnvCustom` dùng `FrmMain.RunWithLoadingAsync(() => Task.Run(...))` + cờ `_isBusy`.

## File đã đổi (đúng 6 file code theo spec, +538/−255 gồm spec)
| File | Thay đổi |
|---|---|
| `UC_ConfigEnvCustom.Designer.cs` | `dgv_Envs` → `spc_Containers` (SplitContainer ngang) chứa `dgv_System` + `dgv_User`, `lbl_SystemTitle`/`lbl_UserTitle`, cột `col_Sys*`/`col_User*`, nối sự kiện trong Designer |
| `UC_ConfigEnvCustom.cs` | Nạp/lọc hai lưới qua `GetVariables`, `_activeGrid`, Add/Edit/Delete/Restore theo scope, `_isBusy` |
| `FrmCustomEnvEditor.cs` | Nhận `EnvScope`, gọi `CreateVariable`/`UpdateVariable` |
| `FrmEnvListEditor.cs` | Nhận `EnvScope` (mặc định Machine), `UpdateVariable(_scope, ...)` |
| `FrmEnvRestorePicker.cs` + `.Designer.cs` | Picker theo scope, mọi key, thêm cột `col_Key` |

## Kiểm tra của Claude
- `git diff`: chỉ 6 file code + spec; không còn tham chiếu `dgv_Envs`; không `Console./Log./Debug.`; `new Control` chỉ nằm trong Designer.
- `dotnet build BaseRepo.slnx -m:1`: 0 Error, không `WFO1000`.

## Cần Tester/Reviewer chú ý
- AC2–AC9 là thủ công (UI/Administrator), Tester không thao tác UI; chỉ build + cấu trúc + harness nếu có.
- Reviewer: tính đúng của `_activeGrid` khi thao tác, thứ tự xác nhận → gọi dịch vụ, xử lý `KeyNotFoundException`/`UnauthorizedAccessException`, mask giá trị chung hai lưới, hồi quy tab Mặc định, `NeedsExtraWarning` khi sửa biến thường.
