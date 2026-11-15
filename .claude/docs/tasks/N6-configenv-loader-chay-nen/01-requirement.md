# N6 — ConfigEnv: loader và chạy nền cho Lưu/Xóa/Khôi phục · Phân tích yêu cầu

- **Người viết:** BA (skill `ba-requirements`) · **Ngày:** 2026-11-16 · **Module:** 6 (SystemManagement)
- **Trạng thái:** ✅ đã chốt bởi Minh Thuận ngày 2026-11-16 (qua hỏi đáp: chọn "Loader + chạy nền cho mọi Lưu/Xóa/Khôi phục", "N5 đạt, chỉ còn việc loader")

## 1. Bối cảnh và mục tiêu
Khi bấm **Lưu** trong form sửa danh sách (`FrmEnvListEditor`) ứng dụng trông như bị đơ. Nguyên nhân gốc (đã đọc code):
- `FrmEnvListEditor.btn_Save_Click` gọi `CustomEnvService.UpdateCoreValue` **trực tiếp**, không qua loader.
- Ngay cả nơi đã có loader, `FrmMain.RunWithLoadingAsync` chạy phần việc **trên luồng UI** (`await Task.Yield(); await action();` với action đồng bộ trả `Task.CompletedTask`), nên ghi registry + broadcast `WM_SETTINGCHANGE` (chờ tới 5 giây) làm UI đứng và thanh marquee không chạy.
- Loader là `progressBar1` của cửa sổ chính, còn các form sửa là hộp thoại modal đè lên.
Mục tiêu: khi Lưu/Xóa/Khôi phục, UI **không đứng**, người dùng **thấy đang xử lý**, và không thể bấm lặp gây ghi hai lần.

## 2. Yêu cầu gốc
> "form chạy ổn nhưng cần thay đổi các hành vi sau: 1. khi lưu chưa gọi loader khiến app trông như bị đơ"

## 3. Hiện trạng
- `FrmEnvListEditor.btn_Save_Click` (đồng bộ, không loader).
- `FrmCustomEnvEditor.btn_Save_Click` (async): 3 nhánh (Sửa cốt lõi `UpdateCoreValue`, Sửa `UpdateValue`, Thêm `Create`) đã bọc `RunWithLoadingAsync` nhưng việc chạy trên luồng UI.
- `UC_ConfigEnvCustom.cs`: Xóa (`CustomEnvService.Delete`) và Khôi phục (`RestoreCoreValue`) đã bọc `RunWithLoadingAsync`, cùng vấn đề; `LoadDataAsync` cũng dùng loader (đọc nhanh, ngoài phạm vi).
- `FrmMain.RunWithLoadingAsync(Func<Task>)` còn được `UC_ConfigEnv.cs` và `FrmMain.cs` dùng; các action ở đó có thể chạm control UI nên **không** đổi hàm này.

## 4. Luồng nghiệp vụ
1. Người dùng bấm Lưu (form sửa danh sách / form thêm-sửa biến) hoặc Xóa / Khôi phục ở màn hình chính.
2. Hiện loader; các nút của form đang mở bị khóa; con trỏ chờ; không đóng được form trong lúc ghi.
3. Phần việc ghi (registry, backup, broadcast) chạy ở **luồng nền**.
4. Xong: ẩn loader, mở khóa; thành công → thông báo + đóng form + tải lại lưới như hiện nay; lỗi → thông báo như hiện nay và mở khóa để thử lại.

## 5. Quy tắc nghiệp vụ
- R1: Mọi Lưu/Xóa/Khôi phục trong ConfigEnv gọi dịch vụ qua `FrmMain.RunWithLoadingAsync(() => Task.Run(...))`. **Không đổi** `FrmMain.RunWithLoadingAsync`.
- R2: Trong khi đang lưu, form khóa mọi nút (Lưu, Hủy, Thêm, Cập nhật, Xóa, Lên, Xuống, sinh giá trị...) và ô nhập; bật `UseWaitCursor`; **chặn đóng form** (kể cả nút X) cho đến khi xong. Khi lỗi thì mở khóa lại.
- R3: Bấm Lưu nhiều lần liên tiếp chỉ ghi **một lần** (cờ đang lưu).
- R4: `FrmEnvListEditor` bước kiểm đường dẫn tồn tại khi Lưu (`FindMissingPaths`) cũng chạy ở luồng nền dưới loader (tránh đơ với đường dẫn mạng).
- R5: Hành vi nghiệp vụ không đổi: cùng thứ tự kiểm tra, cùng thông báo, cùng nhánh lỗi (`UnauthorizedAccessException`/`SecurityException`, `KeyNotFoundException` → Abort, lỗi khác); exception ném từ luồng nền vẫn được bắt đúng nhánh.
- R6: Mọi thao tác chạm control UI vẫn ở luồng UI (đọc giá trị từ control **trước** khi `Task.Run`).

## 6. Trường hợp biên và lỗi
| Tình huống | Hành vi mong đợi |
|---|---|
| Lỗi quyền/biến không còn/lỗi khác khi ghi | Thông báo như cũ, loader tắt, form mở khóa (trừ nhánh Abort: đóng form) |
| Người dùng bấm X khi đang ghi | Không đóng được; sau khi xong mới đóng nếu thành công |
| Bấm Lưu hai lần nhanh | Chỉ một lần ghi, một file backup |
| `ThrowInfo/ThrowWarning/ThrowError` trong lúc loader | Giữ cách làm hiện tại (các hàm đó tự `HideLoading`) |

## 7. Dữ liệu và bảo mật
Không đổi: không log giá trị biến, thông báo lỗi chỉ nêu key, không đổi cơ chế backup/xác nhận/elevated.

## 8. Phụ thuộc
- Sau N5 (4 form đã ở Designer, đã ✅). **Làm trước N4-1/N4-2** (N4-2 viết lại các handler này và phải giữ mẫu chạy nền).

## 9. Câu hỏi còn mở
- (trống)

## 10. Ngoài phạm vi
- Đổi `FrmMain.RunWithLoadingAsync`, loader trong tab Mặc định (`UC_ConfigEnv.cs`), `LoadDataAsync`, đổi giao diện/màu, thanh tiến trình riêng trong form, hủy thao tác giữa chừng, thêm package, test project.
