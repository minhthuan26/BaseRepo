# N4-2 — ConfigEnv: giao diện hai lưới System / User · Code Review

- **Reviewer:** Claude (chế độ B, tự review) · **Ngày:** 2026-11-16 · **Vòng:** 1

## 1. Phát hiện
| Mức độ | File:dòng | Vấn đề | Gợi ý sửa |
|---|---|---|---|
| minor | UC_ConfigEnvCustom.cs `UpdateGridTitles` | Mỗi lần lưới nhận focus tạo 2 `new Font(...)` mà không Dispose (rò rỉ GDI nhỏ, tích lũy theo số lần đổi lưới). | Giữ 2 Font tĩnh (Bold/Regular) tạo một lần, hoặc Dispose font cũ trước khi gán. |
| minor | UC_ConfigEnvCustom.cs `dgv_System/User_CellDoubleClick` → `btn_Edit_Click` | Double-click không dùng `sender`, dựa vào `_activeGrid` do `Enter` đặt. Hiện đúng vì `Enter` chạy trước CellDoubleClick, nhưng ngầm định. | Đặt `_activeGrid` tường minh từ `sender` trong 2 handler double-click trước khi gọi sửa. |
| minor | UC_ConfigEnvCustom.cs `btn_Edit_Click` (ReadRaw), `btn_Restore_Click` (ListBackups) | Đọc registry / file backup chạy đồng bộ trên luồng UI, lệch nhẹ mẫu N6 (I/O trong Task.Run). Dữ liệu nhỏ nên ít ảnh hưởng. | Bọc `Task.Run` nếu muốn nhất quán tuyệt đối. |
| minor | UC_ConfigEnvCustom.cs `btn_Edit_Click` (biến thường, Machine) | Không kiểm elevated trước `NeedsExtraWarning`, nên người dùng không Admin vẫn bị hỏi cảnh báo rồi mới nhận lỗi quyền từ editor. Kết quả đúng (không backup, báo Administrator), chỉ UX. | Kiểm `RequiresElevation && !IsElevated` sớm như nhánh cốt lõi. |
| minor | UC_ConfigEnvCustom.cs `btn_Edit_Click` | `_activeGrid ?? dgv_System` thừa (đã gán trong ctor). | Bỏ `?? dgv_System`. |

Không có blocker/major.

## 2. Đánh giá theo tiêu chí
- Đúng spec: đạt. Hai lưới (SplitContainer ngang), tiêu đề User có `UserDomainName\UserName`, Type Cốt lõi/BaseRepo/Khác, Tìm kiếm/Mask/Làm mới áp cả hai lưới, 6 key `BaseRepo__*` hiện và xóa như biến thường. Thêm/Sửa/Xóa/Khôi phục đều suy scope từ `_activeGrid`; `_activeGrid` cập nhật ở `Enter`, nhãn đậm + ▶. Sửa: cốt lõi kiểm elevated (chỉ khi RequiresElevation) → gõ lại key → ReadRaw(scope) → `IsListKey` mở `FrmEnvListEditor(key, raw, scope)`, còn lại `FrmCustomEnvEditor(..., scope, isCoreEdit: true)`; biến thường giữ `NeedsExtraWarning`, không đòi gõ lại key. Xóa chặn cốt lõi ở cả hai scope. Khôi phục: cảnh báo khi Machine không elevated, picker theo scope mọi key (thêm col_Key), gõ lại `entry.Key`, `RestoreVariable(entry)`. Machine truyền `SysConfig.EnvKeys` làm defaultKeys, User truyền null (khớp cả `CreateVariable` chỉ chặn default key ở Machine). Việc bỏ `ValidateKey` ở editor là chấp nhận được: `CreateVariable` đã tự kiểm tên, trùng trong cùng scope (ReadRaw) và ném `ArgumentException`, editor bắt và giữ form mở.
- Ghi qua hàm N4-1: đạt. Grep `UpdateCoreValue|RestoreCoreValue|GetCustomEnvs|dgv_Envs` trong code UI mới = 0 (chỉ còn định nghĩa cũ trong `CustomEnvService.cs`, không sửa theo phạm vi). UI chỉ gọi `CreateVariable/UpdateVariable/DeleteVariable/RestoreVariable/GetVariables/ListBackups(scope)/ReadRaw`.
- Mẫu N6: đạt. UC dùng `RunWithLoadingAsync(() => Task.Run(...))` cho tải/xóa/khôi phục, `_isBusy` bọc try/finally cho Refresh/Add/Edit/Delete/Restore. Form dùng `RunSavingAsync(() => Task.Run(...))`, đọc control trước, `OnSavingStateChanged`/`IsSaving` giữ nguyên, mở khóa trong finally của Frm_Base kể cả khi lỗi.
- Xử lý lỗi: đạt. `KeyNotFoundException` báo + tải lại (UC Delete/Edit; form trả Abort), Unauthorized/Security báo quyền theo scope, `ArgumentException` được bắt riêng ở editor. Thông báo chỉ nêu key, không nêu value; không `Console/Log/Debug`.
- Đúng convention: đạt. Bố cục nằm trong Designer, tên control `dgv_System/dgv_User/lbl_*Title/col_Sys*/col_User*/spc_Containers`, sự kiện nối trong Designer, property public duy nhất `SelectedEntry` có `DesignerSerializationVisibility(Hidden)`, không `.resx` mới, đúng 6 file code ở mục 3 (cộng spec/log).
- Hiệu quả: chấp nhận được; chỉ các minor ở trên.
- An toàn: không log value, không đụng HKLM khi thiếu quyền (chặn trong dịch vụ), backup nằm trong dịch vụ N4-1.
- Dễ bảo trì và đủ test: tự động PASS (build 0 Error, harness 29/29). AC4–AC9 thủ công chưa chạy (đúng theo spec mục 9), người dùng cần chạy checklist trong `04-test-report.md` (Administrator, sau `reg export`) trước khi đánh ✅.
- Hồi quy: tab Mặc định và `UC_ConfigEnv*` không nằm trong diff; `UC_ConfigEnv` chỉ nhúng UC, không tham chiếu lưới.

<!-- TOOLING:BEGIN -->
## 2b. Phạm vi ảnh hưởng
Không dùng CodeGraph (như spec 2b). Grep xác nhận `dgv_Envs` không còn tham chiếu; `FrmCustomEnvEditor`/`FrmEnvListEditor`/`FrmEnvRestorePicker` chỉ được `UC_ConfigEnvCustom` gọi, đã cập nhật đủ chữ ký mới. Không nơi gọi nào bị vỡ.
<!-- TOOLING:END -->

## 3. Đề xuất sau (minor, không chặn)
- Xử lý 5 minor ở mục 1 (đặc biệt Font tĩnh và đặt `_activeGrid` từ `sender`).
- Các hàm cũ chỉ-Machine (`UpdateCoreValue`, `RestoreCoreValue`, `GetCustomEnvs`, `Create/UpdateValue/Delete`) giờ không còn nơi gọi trong UI; cân nhắc dọn ở task riêng (ngoài phạm vi N4-2).

VERDICT: APPROVE
