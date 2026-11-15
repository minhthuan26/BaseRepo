# N1 — ConfigEnv: env tùy chọn (thêm / sinh value / sửa / xóa) · Phân tích yêu cầu

- **Người viết:** BA (skill `ba-requirements`) · **Ngày:** 2026-11-15 · **Module:** 6 (SystemManagement)

## 1. Bối cảnh và mục tiêu
Màn hình ConfigEnv của SystemManagement hiện chỉ quản lý đúng 6 biến môi trường mặc định (AccessKey, RefreshKey, Audience, Issuer, PrivateKey, PublicKey), khoá cứng theo chỉ số trong code và Designer. Người vận hành (dev/DevOps cài đặt hệ thống BaseRepo trên máy Windows) cần tự thêm các biến môi trường khác mà không phải sửa code, đồng thời có công cụ sinh value an toàn như đang có cho 6 key mặc định.
Thành công khi: người dùng tạo, sinh value, sửa, xóa được một biến tùy chọn ngay trên màn hình, và kết quả phản ánh đúng trạng thái biến Machine thật.

## 2. Yêu cầu gốc
> ở module SystemManagement, view ConfigEnv mình cần mở rộng chức năng, ngoài các cấu hình env mặc định thì cho phép tạo cấu hình env tùy chọn bằng cách nhập key, sinh value, xóa chỉnh sửa

## 3. Hiện trạng
- `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.cs` (+ `.Designer.cs`): WinForms thuần, 6 hàng cố định (panel3..panel8), truy cập `FrmMain.SysConfig.EnvKeys[0..5]` theo chỉ số và `switch` index→field.
- `Config/system.config` (XML): `EnvKeys` là `List<string>` chỉ chứa tên 6 key; chỉ đọc (`FileHelper.ReadXML`), chưa có code ghi.
- `CommonLibs/Utils/EnvironmentHelper.cs`: bọc get/set env, target mặc định **Machine** (cần quyền admin); có `GetAll/Get/Set/Delete/Exists/Update/Create` (`Exists` = giá trị không rỗng; `Delete` chưa được UI dùng).
- Sinh value hiện có: `Cypher.AesCypher.GenerateAesKey` + `CypherHelper.ByteArrayToBase64` (AccessKey/RefreshKey); `urn:api:`/`urn:auth:` + GUID (Audience/Issuer); `Cypher.RsaCypher.GenerateKeys(2048)` ghi file, value env là đường dẫn file (PrivateKey/PublicKey).
- `FrmMain` có sẵn `RunWithLoadingAsync`, `ThrowInfo/Warning/Error/Question` (hộp Yes/No).
- Hai nút "Đồng bộ về cục bộ" / "Tải lên tập trung" đang trống, **không** thuộc yêu cầu này.
- Chưa có test project (M1 xUnit chưa làm).

## 4. Luồng nghiệp vụ
1. Người dùng mở ConfigEnv: có 2 tab. Tab **Mặc định** giữ nguyên 6 hàng hiện tại. Tab **Tùy chỉnh** hiển thị danh sách (Key, Value) đọc lại từ biến Machine trên máy, kèm các nút chức năng.
2. **Thêm:** nhập Key → nhập Value tay, hoặc chọn kiểu sinh rồi bấm Sinh → Lưu ghi vào biến Machine.
3. **Sửa:** chọn một dòng → đổi Value (nhập tay hoặc sinh lại) → Lưu. Key cố định; muốn đổi tên key thì xóa rồi tạo mới.
4. **Xóa:** chọn dòng → hộp xác nhận Yes/No → xóa thật biến Machine khỏi máy.
5. Sau mỗi thao tác danh sách tải lại từ máy.

Kiểu sinh value (đều cho phép nhập tay thay thế):
- **AES key base64** (độ dài chọn được, mặc định như AccessKey hiện tại).
- **GUID dạng urn** (tiền tố tùy chọn, mặc định `urn:api:`).
- **Chuỗi ngẫu nhiên** chữ-số, độ dài người dùng chọn.
- **Cặp RSA 2048:** ghi file private/public qua SaveFileDialog; value là đường dẫn file được chọn (theo cách PrivateKey/PublicKey hiện tại).

## 5. Quy tắc nghiệp vụ
- R1: Danh sách tùy chọn lấy từ biến môi trường Machine trên máy, không lưu file danh sách riêng, không ghi `system.config`.
- R2: Key nhập tự do (không bắt buộc tiền tố `BaseRepo__`).
- R3: Không cho tạo key trùng với 6 key mặc định hoặc key đã tồn tại.
- R4: 6 key mặc định không xuất hiện trong tab Tùy chỉnh và không được xóa.
- R5: Xóa là xóa thật biến Machine (`EnvironmentHelper.Delete`), luôn có hộp xác nhận Yes/No.
- R6: Sửa chỉ đổi Value; Key chỉ nhập khi tạo mới.
- R7: Ghi/xóa chạy qua `RunWithLoadingAsync`, báo kết quả bằng `ThrowInfo/ThrowError`.
- R8: Tên key hợp lệ theo quy tắc biến môi trường Windows: không rỗng, không chứa `=`, không ký tự điều khiển, không khoảng trắng đầu/cuối, tối đa 255 ký tự.

## 6. Trường hợp biên và lỗi
| Tình huống | Hành vi mong đợi |
|---|---|
| Key rỗng / có `=` / quá dài | Từ chối, báo lỗi rõ, không ghi |
| Key trùng key mặc định hoặc đã có | Từ chối, báo "đã tồn tại" |
| Value rỗng | Từ chối (vì `Exists` coi rỗng là chưa đặt, Windows xóa biến khi gán rỗng) |
| Không có quyền admin khi ghi Machine | Báo lỗi quyền, danh sách không đổi |
| Biến bị xóa/sửa bên ngoài app khi đang mở | Tải lại danh sách khi thao tác; sửa/xóa biến không còn tồn tại thì báo lỗi, không crash |
| Hủy SaveFileDialog khi sinh RSA | Không đổi value, không báo lỗi |
| Sinh RSA: private/public là hai lần sinh độc lập | Sinh cả cặp trong một lần bấm để khớp nhau |
| Độ dài sinh ngoài khoảng cho phép | Từ chối, nêu khoảng hợp lệ |

## 7. Dữ liệu và bảo mật
- Value có thể là secret (khóa, mật khẩu): **không ghi log**, không đưa vào thông báo lỗi.
- Che value mặc định bằng ký tự `•` trong danh sách, có nút hiện/ẩn khi cần xem.
- Ghi Machine env cần quyền admin (app đã có `app.manifest`, cần xác nhận trong spec).
- Xóa thật là thao tác không hoàn tác: bắt buộc xác nhận.
- Không dùng dữ liệu sản xuất khi test: kiểm tra thủ công dùng key giả có tiền tố dễ nhận ra.

## 8. Phụ thuộc
- Không phụ thuộc task khác. Liên quan M16 (verify ConfigEnv trên net10) nhưng độc lập.
- Không cần 🔑.

## 9. Câu hỏi còn mở
_Không còn._ Đã chốt 2026-11-15: tab Tùy chỉnh liệt kê **tất cả** biến Machine ngoài 6 key mặc định, kèm ô tìm kiếm và cảnh báo thêm khi sửa/xóa biến không có tiền tố `BaseRepo__` (xem R9–R10).

Bổ sung sau khi chốt (từ khảo sát code):
- R9: Chỉ liệt kê biến thuộc scope **Machine** (`Environment.GetEnvironmentVariables(Machine)`). Không dùng `EnvironmentHelper.GetAll()` vì hàm đó gộp cả User và Process.
- R10: Sửa/xóa biến không có tiền tố `BaseRepo__` phải qua hộp xác nhận có cảnh báo riêng, nêu tên key. Một danh sách nhỏ biến hệ thống cốt lõi (`Path`, `PATHEXT`, `ComSpec`, `SystemRoot`, `windir`, `SystemDrive`, `OS`, `TEMP`, `TMP`, `PSModulePath`, `PROCESSOR_ARCHITECTURE`, `NUMBER_OF_PROCESSORS`) chỉ cho **xem**, khóa sửa/xóa, vì xóa/sửa sai làm hỏng Windows. Đây là đề xuất của BA, bạn có thể bỏ khi duyệt spec.

## 10. Ngoài phạm vi
- Hai nút "Đồng bộ về cục bộ" / "Tải lên tập trung".
- Đổi tên key trực tiếp, hỗ trợ User scope, lưu danh sách vào `system.config` hoặc file riêng.
- Dựng test project xUnit (M1); lần này tách logic thuần để test được sau, nghiệm thu bằng build xanh + checklist chạy tay trên Windows.
- Sửa lỗi có sẵn ở `Responsive()` chưa cài đặt và hành vi btn_AutoAll hiện tại.
