# N1 — ConfigEnv: env tùy chọn · Code Review

- **Reviewer:** Claude (chế độ B, tự review) · **Ngày:** 2026-11-15 · **Vòng:** 1

## 1. Phát hiện
Không có blocker/major. Chỉ có minor:

| Mức độ | File:dòng | Vấn đề | Gợi ý sửa |
|---|---|---|---|
| minor | UC_ConfigEnvCustom.cs:118,261 | Form Sửa nạp `selected.Value` thật vào `_txtValue` (hiện plaintext) dù lưới đang che value: người dùng mở Sửa là thấy value, vô hiệu hóa việc che mặc định. | Đặt `UseSystemPasswordChar`/che trong editor khi `_isMasked`, hoặc chấp nhận có chủ đích và ghi chú trong handover. |
| minor | UC_ConfigEnvCustom.cs:469,488 | Cặp RSA ghi file private ở thư mục đang chạy (mặc định `Directory.GetCurrentDirectory()`), ACL mặc định, plaintext. Giống hệt `UC_ConfigEnv.cs:167,181` gốc nên không vi phạm spec, nhưng vẫn là rủi ro. | Ghi vào "Đề xuất sau": cảnh báo người dùng chọn nơi an toàn, hoặc siết ACL. |
| minor | CustomEnvService.cs:122,141 | Không giới hạn độ dài value (Windows Machine env tối đa khoảng 32767 ký tự). Value dài sẽ ném exception, được bắt bởi catch chung và báo `ex.Message` (không chứa value). | Thêm kiểm tra độ dài trong `ValidateValue` (hoặc để sau). |
| minor | EnvValueGenerator.cs:1 | `using CommonLibs.Utils;` thừa (tester đã báo). Cũng thừa trong UC_ConfigEnvCustom.cs: `System.Data`, `System.ComponentModel`, `SystemManagement.Views`. | Xóa các using thừa. |
| minor | UC_ConfigEnvCustom.cs:198,207 | Hai class phụ (`EnvRowViewModel`, `FrmCustomEnvEditor`) nằm chung file UC. Chấp nhận trong phạm vi 6 file của spec (tách file sẽ vượt danh sách được phép); UC vẫn là class đầu tiên nên Designer mở bình thường. | Nếu sau này gọn hơn thì tách `FrmCustomEnvEditor.cs`, cần spec/task riêng. |
| minor | UC_ConfigEnvCustom.cs:36-40 | `RunWithLoadingAsync` thực ra chạy `action()` đồng bộ trên UI thread (sau `Task.Yield`), nên Get/Set env chặn UI trong lúc chạy. Không có deadlock hay cập nhật control từ thread nền; đúng pattern gốc của FrmMain. | Không cần sửa. |

## 2. Đánh giá theo tiêu chí
- Đúng spec: chỉ đúng 6 file được phép (`git status` xác nhận). Diff `UC_ConfigEnv.Designer.cs` chỉ thêm `tabControl1/tabDefault/tabCustom`, chuyển panel3..panel9 vào `tabDefault`; `panel2` giữ Dock Fill; Suspend/ResumeLayout cân đối (3 cặp mới); không đổi tên control hay handler; `UC_ConfigEnv.cs` chỉ thêm 4 dòng gắn UC custom. Không đổi logic 6 hàng mặc định. Hai nút Đồng bộ/Tải lên không đụng tới.
- Đúng convention: tên `_camelCase`, `txt_/btn_/dgv_`, chuỗi UI tiếng Việt, dùng `FrmMain.ThrowError/Info/Warning/Question`, `UC_ConfigEnvCustom` kế thừa `UserControl` thường (control con nhúng, không cần `IBasePage`; build không có WFO1000 theo tester).
- Hiệu quả: tải env 1 lần, lọc trong bộ nhớ; không N+1. Async đúng pattern, không `.Result/.Wait`.
- An toàn: dùng đúng scope Machine, không dùng `EnvironmentHelper.GetAll/Exists/Update/Create`; `Delete` dùng `EnvironmentHelper.Delete(key, Machine)` (đã xác nhận chữ ký). Không ghi value rỗng/khoảng trắng (cả Create và UpdateValue). Kiểm tra tồn tại trước khi Sửa/Xóa (`CustomEnvService.cs:137,152`). Chặn biến cốt lõi ở cả UI và service. Xóa luôn có Yes/No; cảnh báo nêu tên key khi không có `BaseRepo__`. Thông báo lỗi chỉ nêu key; không log; value không vào tooltip/clipboard/ThrowError/ThrowInfo (ThrowInfo RSA chỉ có đường dẫn file). Bắt `SecurityException`/`UnauthorizedAccessException`. Sinh ngẫu nhiên bằng `RandomNumberGenerator` (GetBytes/GetString), GUID `:N`. Hủy dialog RSA `return` trước khi ghi file hoặc đổi value.
- Dễ bảo trì và đủ test: logic thuần tách thành `static` nên test được sau (M1); harness tester 49/49 PASS phần tự động. Không TODO, không code chết. Trùng lặp nhẹ với `UC_ConfigEnv` ở đoạn SaveFileDialog RSA, chấp nhận vì ngoài phạm vi.

<!-- TOOLING:BEGIN -->
## 2b. Phạm vi ảnh hưởng
Không dùng CodeGraph. Đọc code: `UC_ConfigEnv` chỉ được `UC_Home` khởi tạo bằng `new` (không đổi ký hiệu public); tên control cũ được giữ nên handler trong `UC_ConfigEnv.cs` không vỡ. `CommonLibs` không bị sửa. Chạy tay còn lại cần xác nhận: AC3 (User scope), AC4, AC8 (nạp private/public bằng `RsaCypher.Import*`; file ghi bằng `Utf8ToByteArray(base64)` giống code gốc), AC9-AC15, AC17, và hiển thị TabControl/Dock đúng khi chạy thật (Designer chưa kiểm bằng mắt).
<!-- TOOLING:END -->

## 3. Đề xuất sau (minor, không chặn)
- Che value trong form Sửa khi lưới đang ở chế độ che.
- Cảnh báo vị trí lưu file private RSA / siết ACL (cả `UC_ConfigEnv` gốc).
- Giới hạn độ dài value; xóa using thừa.
- Rủi ro cho người chạy tay: cần quyền Admin; kiểm tra `SetEnvironmentVariable` Machine có thể làm UI đứng vài giây (broadcast thay đổi); các biến hệ thống không nằm trong danh sách khóa (VD `USERNAME`, `PROCESSOR_IDENTIFIER`) chỉ bị cảnh báo, không chặn; chỉ thử với `N1_TEST_*`.

VERDICT: APPROVE
