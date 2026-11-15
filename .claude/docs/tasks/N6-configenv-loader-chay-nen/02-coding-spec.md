# N6 — ConfigEnv: loader và chạy nền cho Lưu/Xóa/Khôi phục · Coding Spec

- **Trạng thái duyệt:** ✅ đã duyệt bởi Minh Thuận ngày 2026-11-16
- **Yêu cầu gốc:** `01-requirement.md` cùng thư mục · **Phụ thuộc:** N5 (đã ✅) · **Làm trước N4-1/N4-2**

## 1. Phạm vi
Làm cho Lưu/Xóa/Khôi phục trong ConfigEnv không làm đứng UI: bọc lời gọi dịch vụ bằng `FrmMain.RunWithLoadingAsync(() => Task.Run(...))`, khóa form và chặn đóng form khi đang lưu, chống bấm lặp. **Không đổi hành vi nghiệp vụ, thông báo, thứ tự kiểm tra.**

## 2. Ngoài phạm vi
- `FrmMain.cs` (`RunWithLoadingAsync`, `ShowLoading`, `HideLoading`), `UC_ConfigEnv.cs` (tab Mặc định), `LoadDataAsync`, `CustomEnvService`/`EnvRegistryService`/`EnvBackupService`, file `.Designer.cs` và `.resx`.
- Đổi giao diện, thêm control, thêm package, test project.

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph. `FrmMain.RunWithLoadingAsync` còn được `UC_ConfigEnv.cs:298` và `FrmMain.cs:232` gọi nên **không đổi**; chỉ đổi cách các nơi trong ConfigEnv truyền action vào.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `FrmEnvListEditor.btn_Save_Click` (sync → async, loader, khóa form) | Nơi gọi form (`UC_ConfigEnvCustom.btn_Edit_Click`) chỉ đọc `DialogResult` | Không |
| `FrmCustomEnvEditor.btn_Save_Click` (3 nhánh thêm `Task.Run`, khóa form) | `UC_ConfigEnvCustom.cs` chỉ đọc `DialogResult` | Không |
| `UC_ConfigEnvCustom` Xóa/Khôi phục (thêm `Task.Run`) | Không | Không |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
Thư mục `SystemManagement/Views/ConfigEnv/`:

| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `FrmEnvListEditor.cs` | Sửa | `btn_Save_Click` thành `async void`; loader + `Task.Run` cho `FindMissingPaths` và `UpdateCoreValue`; khóa form khi lưu; `OnFormClosing` chặn khi đang lưu; cờ `_isSaving` |
| `FrmCustomEnvEditor.cs` | Sửa | 3 nhánh trong `btn_Save_Click` đổi lambda thành `() => Task.Run(() => ...)`; khóa form khi lưu; `OnFormClosing` chặn khi đang lưu; cờ `_isSaving` |
| `UC_ConfigEnvCustom.cs` | Sửa | Xóa (`Delete`) và Khôi phục (`RestoreCoreValue`) đổi lambda thành `() => Task.Run(() => ...)` |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại. Không sửa `.Designer.cs`, `.resx`, `FrmMain.cs`.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
Chữ ký công khai giữ nguyên. Hành vi mới:

| Thành phần | Hành vi |
|---|---|
| `FrmEnvListEditor` Lưu | Đọc danh sách từ `lst_Items` ở luồng UI → `Normalize`/`Validate` (nhanh, UI) → `FindMissingPaths` ở luồng nền dưới loader → nếu có cảnh báo hỏi Yes/No (UI) → ghi ở luồng nền dưới loader → thông báo thành công + `DialogResult.OK` + `Close()` |
| `FrmCustomEnvEditor` Lưu | Validate như cũ (UI), đọc `txt_Key.Text`/`txt_Value.Text` **trước** `Task.Run`; `UpdateCoreValue`/`UpdateValue`/`Create` ở luồng nền dưới loader |
| Xóa/Khôi phục (`UC_ConfigEnvCustom`) | Gọi `Delete`/`RestoreCoreValue` ở luồng nền dưới loader |
| Trạng thái "đang lưu" | `_isSaving = true` → khóa mọi nút và ô nhập của form (đặt `Enabled=false` cho control thao tác, gồm cả Hủy), `UseWaitCursor = true`; `OnFormClosing` đặt `e.Cancel = true` khi `_isSaving`; xong (kể cả lỗi) trả `_isSaving = false`, mở khóa, tắt wait cursor trong `finally` |

## 6. Tên theo convention
- Field private `_isSaving`; method private `SetSavingState(bool saving)` trong mỗi form (khóa/mở khóa control, `UseWaitCursor`), `override OnFormClosing(FormClosingEventArgs e)`.
- Handler giữ tên `btn_Save_Click`. Không thêm property public. Không thêm control. Comment tiếng Việt giải thích lý do (vì sao chạy nền, vì sao khóa form).
- Không `Console.*`/`Log.*`/`Debug.*`; thông báo lỗi chỉ nêu key.

## 7. Luồng xử lý dự kiến
1. Bấm Lưu: nếu `_isSaving` thì thoát ngay (chống bấm lặp).
2. Lấy dữ liệu từ control (UI), validate như cũ; lỗi validate thì thông báo như cũ và return (không khóa form).
3. `SetSavingState(true)`; `try { await FrmMain.RunWithLoadingAsync(() => Task.Run(() => dịch vụ(...))); ... } catch ... finally { SetSavingState(false); }`. Các `catch` giữ nguyên (`SecurityException`/`UnauthorizedAccessException`, `KeyNotFoundException` → `DialogResult.Abort` + `Close()`, `Exception`).
4. Thành công: `ThrowInfo` như cũ, `DialogResult.OK`, `Close()`. **Chú ý:** `Close()` chỉ được gọi sau khi `_isSaving = false` (đặt `_isSaving` về `false` trước `Close()` hoặc để `OnFormClosing` chỉ chặn khi đang giữa thao tác ghi) để form không tự chặn việc đóng của chính nó.
5. Xóa/Khôi phục ở `UC_ConfigEnvCustom`: chỉ đổi lambda, giữ nguyên phần còn lại.

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `dotnet build BaseRepo.slnx -m:1`: 0 Error, không `WFO1000` | Build |
| AC2 | Mọi lời gọi `UpdateCoreValue`/`UpdateValue`/`Create`/`Delete`/`RestoreCoreValue` trong `FrmEnvListEditor.cs`, `FrmCustomEnvEditor.cs`, `UC_ConfigEnvCustom.cs` nằm trong `Task.Run` bên trong `RunWithLoadingAsync` (grep/đọc code); `FindMissingPaths` ở `btn_Save_Click` cũng vậy | Review/grep |
| AC3 | Không có truy cập control UI bên trong lambda `Task.Run` (đọc giá trị control trước khi chạy nền) | Review |
| AC4 | `FrmMain.cs`, `.Designer.cs`, `.resx`, `CustomEnvService.cs` không bị sửa (`git diff`) | Review |
| AC5 | Các nhánh `catch` và thông báo giữ nguyên; `_isSaving` luôn được đặt lại trong `finally`; `Close()` không bị chính `OnFormClosing` chặn | Review |
| AC6 | Lưu `Path` (hoặc biến thử) ở form danh sách: thấy loader chạy, cửa sổ vẫn kéo/di chuyển được, các nút bị khóa, không đóng được bằng X trong lúc ghi; xong thì đóng form như cũ | Thủ công (Administrator) |
| AC7 | Bấm Lưu hai lần nhanh chỉ tạo **một** file backup; lỗi (vd không phải Administrator) thì form mở khóa lại | Thủ công |
| AC8 | Thêm/Sửa biến (form thêm-sửa), Xóa, Khôi phục: loader chạy, UI không đứng, kết quả như trước | Thủ công |
| AC9 | `git diff` chỉ gồm 3 file ở mục 3 | Build/Review |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1`.
- Tester/Reviewer: không có test tự động cho UI; AC2–AC5 kiểm bằng đọc code/grep; AC6–AC8 ghi "chưa chạy, cần thủ công" (trước khi thử `reg export` registry, chỉ thử biến an toàn, khôi phục lại ngay).
