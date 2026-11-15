# N4-2 — ConfigEnv: giao diện hai lưới System / User · Coding Spec

- **Trạng thái duyệt:** ✅ đã duyệt bởi Minh Thuận ngày 2026-11-16
- **Yêu cầu gốc:** `../N4-configenv-hai-luoi-user-system/01-requirement.md` (đã chốt 2026-11-16) · **Phụ thuộc:** **N4-1** (hàm dịch vụ theo scope; phải xong và verify trước) và **N5** (4 form ConfigEnv đã chuyển sang Designer, `FrmCustomEnvEditor` đã ở file riêng) · **Làm sau N4-1 và N5**
- **Bổ sung 2026-11-16 (✅ đã duyệt lại bởi Minh Thuận ngày 2026-11-16):** phụ thuộc thêm **N6**; mọi Lưu/Xóa/Khôi phục/Thêm trong các handler và form mới phải giữ mẫu của N6: gọi dịch vụ qua `FrmMain.RunWithLoadingAsync(() => Task.Run(...))`, đọc control ở luồng UI trước khi chạy nền, khóa form và chặn đóng khi đang lưu, cờ `_isSaving` chống bấm lặp. Không đổi `FrmMain.RunWithLoadingAsync`.
- **Sửa đổi 2026-11-16 (✅ đã duyệt lại bởi Minh Thuận ngày 2026-11-16):** thêm phụ thuộc N5; `FrmEnvRestorePicker` thêm cột Key và tham số scope nên sửa cả `FrmEnvRestorePicker.Designer.cs`; `FrmCustomEnvEditor` nay là file riêng `FrmCustomEnvEditor.cs` (không còn trong `UC_ConfigEnvCustom.cs`); control đặt tên theo `.claude/rules/convention-winforms.md`.

## 1. Phạm vi
Đổi tab **Tùy chỉnh** từ một lưới thành **hai lưới trên/dưới**: **System** (Machine) và **User**, chung thanh công cụ. Gồm:
- Hai lưới hiện **mọi** biến (kể cả 6 key `BaseRepo__*` mặc định), tiêu đề rõ; lưới User ghi tên tài khoản đang chạy ứng dụng.
- Tìm kiếm, Hiện/Ẩn giá trị, Làm mới áp cho cả hai lưới. **Thêm/Sửa/Xóa/Khôi phục tác động lưới đang được chọn** (lưới nhận focus gần nhất; mặc định System).
- Sửa/Xóa/Khôi phục đi qua hàm theo scope của N4-1 (tự backup). Biến cốt lõi ở cả hai scope: cấm xóa, sửa phải gõ lại tên key, `Path`/`PATHEXT`/`PSModulePath` mở `FrmEnvListEditor`. Biến thường không cần gõ lại tên key.
- Khôi phục: chọn bản backup của scope đang chọn (mọi key, kể cả biến đã bị xóa) → gõ lại tên key → `RestoreVariable`.
- Biến trùng tên ở hai scope: **không xử lý** (Windows tự ghép).

## 2. Ngoài phạm vi
- Mọi thay đổi tầng dịch vụ (`EnvRegistryService`, `EnvBackupService`, `CustomEnvService`): đã làm ở N4-1, task này chỉ **gọi**. Nếu thiếu hàm cần thiết thì **dừng và báo**, không tự sửa.
- Tab Mặc định (`UC_ConfigEnv*`), hai nút "Tải lên tập trung"/"Đồng bộ về cục bộ", `FrmEnvTypeKeyConfirm`, `system.config`, thêm package, test project, đổi bố cục tab Mặc định.

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph. `dgv_Envs` và mọi handler (Add/Edit/Delete/Restore/Search/ToggleMask/CellFormatting/CellDoubleClick) nằm trong `UC_ConfigEnvCustom` và do `UC_ConfigEnv` chứa; `FrmEnvRestorePicker` chỉ được `btn_Restore_Click` gọi; `FrmEnvListEditor` chỉ được `btn_Edit_Click` gọi.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `dgv_Envs` → `dgv_System` + `dgv_User` (Designer) | Toàn bộ handler trong `UC_ConfigEnvCustom.cs`; `UC_ConfigEnv` chỉ nhúng control, không tham chiếu lưới | Có (trong task) |
| `FrmCustomEnvEditor(...)` thêm tham số `EnvScope` (file riêng sau N5) | Chỉ `UC_ConfigEnvCustom.cs` gọi | Có |
| `FrmEnvListEditor(key, rawValue)` thêm tham số `EnvScope` mặc định `Machine` | chỉ `btn_Edit_Click` | Có |
| `FrmEnvRestorePicker(string key)` → picker theo scope, mọi key | chỉ `btn_Restore_Click` | Có |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.Designer.cs` | Sửa | Thay `dgv_Envs` bằng `dgv_System` + `dgv_User` (+ 2 nhãn tiêu đề, bố cục trên/dưới, ví dụ `SplitContainer` ngang hoặc `TableLayoutPanel`), giữ thanh công cụ; đánh lại `TabIndex` |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` | Sửa | Nạp/lọc/hiển thị hai danh sách, theo dõi lưới đang chọn, refactor Add/Edit/Delete/Restore theo `EnvScope`, truyền scope khi gọi `FrmCustomEnvEditor` |
| `SystemManagement/Views/ConfigEnv/FrmCustomEnvEditor.cs` | Sửa | Nhận `EnvScope`, gọi `CreateVariable`/`UpdateVariable` theo scope (logic sinh giá trị giữ nguyên) |
| `SystemManagement/Views/ConfigEnv/FrmEnvListEditor.cs` | Sửa | Nhận `EnvScope`, lưu qua `CustomEnvService.UpdateVariable(scope, ...)` |
| `SystemManagement/Views/ConfigEnv/FrmEnvRestorePicker.cs` | Sửa | Chọn bản backup theo scope (mọi key); sau N5 form đã dùng Designer |
| `SystemManagement/Views/ConfigEnv/FrmEnvRestorePicker.Designer.cs` | Sửa | Thêm cột Key vào danh sách bản backup (chỉ đổi cột, không đổi layout khác) |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại. Không có file `.resx` mới.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
| Thành phần | Hành vi | Lỗi |
|---|---|---|
| Lưới **System** (`dgv_System`) | `CustomEnvService.GetVariables(EnvScope.Machine)`; cột Key, Value (che `••••••••` khi đang Mask), Type | Lỗi đọc registry → `FrmMain.ThrowError` |
| Lưới **User** (`dgv_User`) | `GetVariables(EnvScope.User)`; tiêu đề có `Environment.UserDomainName\Environment.UserName` | như trên |
| Cột Type | Biến cốt lõi → "Cốt lõi"; key bắt đầu `BaseRepo__` (kể cả 6 key mặc định) → "BaseRepo"; còn lại → "Khác". Dòng cốt lõi tô xám, giữ như cũ | — |
| Tìm kiếm | Lọc theo Key (không phân biệt hoa thường) trên **cả hai** lưới | — |
| Hiện/Ẩn giá trị | Một cờ `_isMasked` chung cho cả hai lưới; mặc định Ẩn | — |
| Làm mới | Tải lại cả hai lưới | — |
| **Thêm** | `FrmCustomEnvEditor(scope)` theo lưới đang chọn → `CreateVariable(scope, ...)`; Machine truyền `FrmMain.SysConfig?.EnvKeys` làm `defaultKeys`; User không có default key | Trùng trong cùng scope, thiếu quyền |
| **Sửa** | Cốt lõi: Machine thì `IsElevated`; → `FrmEnvTypeKeyConfirm` → lấy giá trị thô (từ dòng đang chọn, hoặc `EnvRegistryService.ReadRaw(scope, key)` nếu cần) → list key mở `FrmEnvListEditor(key, raw, scope)`, còn lại `FrmCustomEnvEditor(isEditMode, isCoreEdit, scope)` → `UpdateVariable(scope, ...)`. Thường: nếu `NeedsExtraWarning` thì hỏi Yes/No như cũ → `FrmCustomEnvEditor` → `UpdateVariable` | Biến không còn → báo + tải lại; không quyền → báo cần Administrator (Machine) |
| **Xóa** | Cốt lõi bị chặn như cũ; còn lại hỏi Yes/No (thông điệp nêu sẽ tự backup; 6 key `BaseRepo__*` xóa như biến thường, không chặn) → `DeleteVariable(scope, key)` | như trên |
| **Khôi phục** | Machine không elevated → cảnh báo. `FrmEnvRestorePicker(scope)` liệt kê `ListBackups(scope)` (cột Thời gian, Key, Lý do, Kiểu, 80 ký tự đầu của giá trị; picker không che giá trị, như N2-1); không có backup → thông báo; chọn → `FrmEnvTypeKeyConfirm(entry.Key, ...)` → `RestoreVariable(entry)` → tải lại | như N2-1 |
| Nhận biết lưới đang chọn | Biến `_activeGrid` cập nhật ở `Enter`/`GotFocus` của từng lưới; nhãn tiêu đề của lưới đang chọn in đậm hoặc có dấu ▶; Add/Edit/Delete/Restore dùng lưới này; báo "Vui lòng chọn một biến" khi lưới đang chọn không có dòng | — |
| Double-click dòng | Sửa dòng đó trong lưới tương ứng | — |

## 6. Tên theo convention
- `dgv_System`, `dgv_User`, `lbl_SystemTitle`, `lbl_UserTitle` (Designer, theo đặt tên hiện có `dgv_Envs`, `txt_Search`, `btn_*`); cột `col_SysKey`/`col_SysValue`/`col_SysType`, `col_UserKey`/`col_UserValue`/`col_UserType`.
- Field private `_systemItems`, `_userItems`, `_activeGrid`, `_isMasked`; handler `dgv_System_CellDoubleClick`, `dgv_User_CellDoubleClick`, `dgv_*_Enter`, `dgv_*_CellFormatting` (có thể dùng chung một hàm tham số hóa).
- `FrmCustomEnvEditor(bool isEditMode, string key, string currentValue, EnvScope scope, bool isCoreEdit = false)`; `FrmEnvListEditor(string key, string rawValue, EnvScope scope = EnvScope.Machine)`; `FrmEnvRestorePicker(EnvScope scope)` với property `SelectedEntry`.
- Không có property public trên `UserControl` hoặc form mới ngoài `SelectedEntry` (tránh `WFO1000`; nếu có property public phải gắn `[DesignerSerializationVisibility(Hidden)]`).
- Comment tiếng Việt giải thích lý do. Không `Console.*`/`Log.*`/`Debug.*`, thông báo lỗi chỉ nêu key, không nêu value.

## 7. Luồng xử lý dự kiến
1. `UC_ConfigEnvCustom_Load` → `LoadDataAsync`: `GetVariables(Machine)` và `GetVariables(User)` trong `RunWithLoadingAsync` → `ApplyFilter` đổ hai lưới (cùng bộ lọc và cờ mask).
2. Người dùng bấm vào một lưới → `_activeGrid` đổi. Thanh công cụ thao tác lưới này: scope = Machine nếu `_activeGrid == dgv_System`, ngược lại User.
3. **Sửa/Xóa/Khôi phục:** như bảng mục 5; mọi ghi đi qua hàm N4-1, nên backup, kiểm elevated (Machine), giữ kind, broadcast đã nằm trong dịch vụ. UI chỉ: chọn dòng, hỏi xác nhận, gọi, báo kết quả (nhắc mở lại terminal), tải lại.
4. Lỗi: `UnauthorizedAccessException`/`SecurityException` → báo cần Administrator (Machine) hoặc báo lỗi quyền chung (User); `KeyNotFoundException` → báo + tải lại; còn lại `FrmMain.ThrowError(ex.Message)`.

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | Build 0 Error, không `WFO1000`; `dgv_Envs` không còn tham chiếu; Designer mở được | Build |
| AC2 | Mở tab Tùy chỉnh: lưới System liệt kê cả 6 key `BaseRepo__*` (nếu đang có ở Machine) và mọi biến Machine; lưới User liệt kê mọi biến của tài khoản hiện tại; hai tiêu đề rõ, tiêu đề User có tên tài khoản | Thủ công |
| AC3 | Tìm kiếm lọc cả hai lưới; Hiện/Ẩn giá trị đổi cả hai; Làm mới tải lại cả hai | Thủ công |
| AC4 | Thêm biến `N4_TEST_UI` vào lưới User: xuất hiện ở lưới User, **không** ở System; vào System: ngược lại; trùng tên trong cùng lưới bị chặn, trùng tên giữa hai lưới được phép (không cảnh báo) | Thủ công |
| AC5 | Sửa `N4_TEST_UI` ở User: có file `Backups/Env/User_N4_TEST_UI_*.txt` (Reason=Edit) chứa giá trị cũ; Xóa: có file Reason=Delete và biến mất khỏi lưới; Khôi phục bản Delete: biến trở lại đúng giá trị + kiểu | Thủ công |
| AC6 | Biến cốt lõi ở User (vd `TEMP`, nếu có): xóa bị chặn; sửa đòi gõ lại tên key; `Path` User mở `FrmEnvListEditor`; Machine không elevated: sửa/xóa/khôi phục báo cần Administrator và không tạo backup | Thủ công |
| AC7 | Sửa một key `BaseRepo__*` mặc định ở System (sau `reg export`): sửa được, có backup; xóa được (hỏi Yes/No), khôi phục tạo lại biến | Thủ công (Administrator) |
| AC8 | Add/Edit/Delete/Restore luôn tác động lưới đang chọn (đổi lưới rồi thao tác) | Thủ công |
| AC9 | Không hồi quy: sửa biến thường Machine vẫn có xác nhận `NeedsExtraWarning`; tab Mặc định hoạt động như cũ | Thủ công |
| AC10 | `git diff` chỉ gồm 6 file ở mục 3 (trên nền N4-1); không log giá trị; không file `.resx` mới | Build/Review |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1`.
- Test thủ công (Administrator): chạy `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env_machine.reg` và `reg export "HKCU\Environment" env_user.reg` **trước**; chỉ thử bằng biến `N4_TEST_*` hoặc biến an toàn (`NUMBER_OF_PROCESSORS`); với 6 key `BaseRepo__*` phải khôi phục lại ngay bằng nút Khôi phục; sau khi xong đối chiếu với hai file `.reg`.
- Tester không tự thao tác UI; chỉ build + đọc code + kiểm cấu trúc lưới, ghi rõ AC thủ công là "chưa chạy".
