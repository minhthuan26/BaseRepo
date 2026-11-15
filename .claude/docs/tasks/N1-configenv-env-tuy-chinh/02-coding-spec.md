# N1 — ConfigEnv: env tùy chọn (thêm / sinh value / sửa / xóa) · Coding Spec

- **Trạng thái duyệt:** ⬜ chờ duyệt
- Yêu cầu đã chốt: [01-requirement.md](01-requirement.md) (2026-11-15). Chỉ một task, không tách nhỏ (một lát cắt UI + logic, ~6 file).

## 1. Phạm vi
Thêm vào màn hình ConfigEnv (SystemManagement) một `TabControl` hai tab: **Mặc định** (6 hàng hiện có, giữ nguyên hành vi) và **Tùy chỉnh** (view mới `UC_ConfigEnvCustom`). Tab Tùy chỉnh liệt kê mọi biến môi trường **Machine** trừ 6 key mặc định, có ô tìm kiếm, và các nút Thêm, Sửa value, Xóa, Sinh value, Làm mới. Logic validate/liệt kê/ghi/xóa/sinh tách thành lớp thuần không phụ thuộc WinForms để sau này test được (M1).

## 2. Ngoài phạm vi
- Hai nút "Đồng bộ về cục bộ" / "Tải lên tập trung" (giữ nguyên, không gắn handler).
- Đổi tên key trực tiếp, User scope, lưu danh sách vào `system.config` hoặc file riêng; không đổi `SystemConfig`/`system.config`.
- Không sửa `CommonLibs` (`EnvironmentHelper` giữ nguyên); không thêm package mới.
- Không sửa logic 6 hàng mặc định (kể cả `btn_AutoAll` sinh 2 khóa RSA không khớp, `Responsive()` chưa cài đặt): chỉ di chuyển control vào tab.
- Không dựng test project xUnit (M1).

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph (khảo sát bằng đọc code qua `ba-analyst`). Coder nên chạy `codegraph_impact` trên `UC_ConfigEnv` trước khi sửa Designer.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `UC_ConfigEnv.Designer.cs` (thêm TabControl, chuyển panel3..panel9 vào tab Mặc định) | `UC_ConfigEnv.cs` dùng các control theo tên (`txt_*`, `btn_*`); `UC_Home` chỉ `new UC_ConfigEnv()` | Tên control giữ nguyên nên `.cs` không đổi logic |
| `FrmMain.SysConfig.EnvKeys` | Chỉ đọc để biết 6 key mặc định cần loại khỏi danh sách | Không đổi |
| `EnvironmentHelper.Set/Delete` | Dùng lại (Machine) | Không đổi |
| `EnvironmentHelper.GetAll/Exists/Update/Create` | **Không dùng**: `GetAll` gộp Machine+User+Process, `Exists` bỏ qua `target` | Dùng trực tiếp `Environment.*` với `Machine` |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Views/ConfigEnv/EnvValueGenerator.cs` | Tạo | Lớp static thuần: sinh AES base64, GUID urn, chuỗi ngẫu nhiên, kiểm tra tham số độ dài |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` | Tạo | Validate key/value, liệt kê Machine env (trừ 6 key mặc định), thêm, sửa value, xóa, cờ khóa/cảnh báo biến hệ thống |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` | Tạo | UserControl tab Tùy chỉnh: lưới, ô tìm kiếm, nút chức năng, luồng sinh RSA |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.Designer.cs` | Tạo | Designer của control trên |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.cs` | Sửa | Chỉ khởi tạo/gắn `UC_ConfigEnvCustom` vào tab Tùy chỉnh (tối thiểu) |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.Designer.cs` | Sửa | Thêm `TabControl` (2 tab); chuyển panel hiện có vào tab Mặc định, giữ nguyên tên control |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại. `UC_ConfigEnvCustom.resx`/`UC_ConfigEnv.resx` chỉ được sinh/đổi tự động bởi Designer nếu cần, không sửa tay.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
Không có API. Giao diện tab **Tùy chỉnh**:

| Thành phần | Hành vi | Lỗi/ràng buộc |
|---|---|---|
| Lưới Key / Value (read-only, value che `••••` mặc định) | Liệt kê Machine env trừ 6 key mặc định, sắp theo Key (không phân biệt hoa thường) | Biến hệ thống cốt lõi hiện mờ/ghi chú "chỉ xem" |
| Ô tìm kiếm | Lọc theo Key (chứa, không phân biệt hoa thường) khi gõ | Rỗng = hiện tất cả |
| Nút Hiện/Ẩn value | Bật/tắt che value toàn lưới | — |
| Nút Thêm | Mở form nhập Key, Value (tay hoặc Sinh), Lưu | Theo R3, R8, value không rỗng |
| Nút Sửa value | Dòng đang chọn → đổi Value (tay hoặc Sinh) | Chặn nếu biến cốt lõi (R10); biến không có tiền tố `BaseRepo__` → xác nhận cảnh báo |
| Nút Xóa | Dòng đang chọn → xác nhận Yes/No → `EnvironmentHelper.Delete` | Như Sửa; luôn có xác nhận |
| Nút Làm mới | Tải lại từ máy | — |
| Chọn kiểu Sinh | AES base64 (độ dài bit: 128/256/512, mặc định 512), GUID urn (tiền tố, mặc định `urn:api:`), Chuỗi ngẫu nhiên (độ dài 8–256, mặc định 32), Cặp RSA 2048 | Ngoài khoảng → báo lỗi nêu khoảng hợp lệ |

Cặp RSA: một lần bấm sinh cả private và public; mở SaveFileDialog lần lượt cho private rồi public (đuôi `.key`/`.pem` theo hành vi hiện tại của `UC_ConfigEnv`); value của env đang tạo/sửa là đường dẫn file **private**; đường dẫn file public hiển thị trong thông báo `ThrowInfo` và không tự tạo thêm env. Hủy dialog = giữ nguyên value, không báo lỗi.

## 6. Tên theo convention
Theo pattern sẵn có của SystemManagement (không có `convention-*.md` cho WinForms; `convention-backend.md` chỉ áp dụng cho backend): PascalCase class/method; field private `_camelCase`; control `txt_*`, `btn_*`, `dgv_*`; namespace `SystemManagement.Views.ConfigEnv`; chuỗi UI tiếng Việt; lỗi qua `FrmMain.ThrowError`, thông tin qua `FrmMain.ThrowInfo`, hỏi qua `FrmMain.ThrowQuestion`, tải qua `FrmMain.RunWithLoadingAsync`. `UC_ConfigEnvCustom` implement `IBasePage` kèm `[DesignerSerializationVisibility(Hidden)]` cho `BaseUC`/`Title` (analyzer `WFO1000`) **nếu** pattern UC hiện tại yêu cầu; nếu chỉ là control con nhúng trong tab thì kế thừa `UserControl` thường (coder đối chiếu `UC_Home`/`UC_ConfigEnv` và báo lại lựa chọn).
- `EnvValueGenerator`: `GenerateAesKey(int bits)`, `GenerateUrn(string prefix)`, `GenerateRandomString(int length)`.
- `CustomEnvService`: `List<CustomEnvItem> GetCustomEnvs(IEnumerable<string> defaultKeys)`, `string? ValidateKey(string key, IEnumerable<string> defaultKeys)`, `string? ValidateValue(string value)`, `void Create(...)`, `void UpdateValue(...)`, `void Delete(...)`, `bool IsProtected(string key)`, `bool NeedsExtraWarning(string key)`. Validate trả `null` nếu hợp lệ, hoặc thông điệp lỗi tiếng Việt.
- `CustomEnvItem` (record nhỏ đặt cùng file `CustomEnvService.cs`): `Key`, `Value`, `IsProtected`, `NeedsExtraWarning`.

## 7. Luồng xử lý dự kiến
1. Mở ConfigEnv → `UC_ConfigEnv` dựng TabControl → tab Tùy chỉnh gọi `CustomEnvService.GetCustomEnvs(FrmMain.SysConfig.EnvKeys)`: `Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine)`, loại 6 key mặc định (so khớp không phân biệt hoa thường), gán cờ `IsProtected`/`NeedsExtraWarning`, đổ vào lưới.
2. Thêm: nhập Key → `ValidateKey` (R3 trùng mặc định hoặc đã có trong Machine env, R8) → nhập/Sinh Value → `ValidateValue` (không rỗng) → `RunWithLoadingAsync(Create)` → `Environment.SetEnvironmentVariable(key, value, Machine)` → `ThrowInfo` → tải lại.
3. Sửa value: chặn nếu `IsProtected`; nếu `NeedsExtraWarning` hỏi xác nhận có nêu tên key; xác minh biến vẫn tồn tại trên máy trước khi ghi (không tồn tại → `ThrowError`, tải lại).
4. Xóa: như bước 3 + hộp xác nhận Yes/No bắt buộc cho mọi biến → `EnvironmentHelper.Delete(key)`.
5. Lỗi `SecurityException`/`UnauthorizedAccessException` → `ThrowError` "Thiếu quyền ghi biến môi trường Machine (cần chạy với quyền Administrator)", danh sách giữ nguyên.
6. **Không log value**; thông báo lỗi chỉ nêu Key, không nêu Value.

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `ValidateKey` từ chối: rỗng/khoảng trắng, chứa `=`, ký tự điều khiển, khoảng trắng đầu/cuối, dài >255, trùng 6 key mặc định (khác hoa thường vẫn coi là trùng), trùng key đã có trong Machine env; chấp nhận key hợp lệ | Unit (chờ M1), Thủ công |
| AC2 | `ValidateValue("")` và chỉ khoảng trắng bị từ chối | Unit (chờ M1), Thủ công |
| AC3 | `GetCustomEnvs` không chứa 6 key mặc định và không chứa biến chỉ có ở scope User/Process | Thủ công (tạo biến User `N1_TEST_USER` → không thấy trong tab) |
| AC4 | Thêm key `N1_TEST_A` với value sinh bằng từng kiểu → đọc lại bằng `Environment.GetEnvironmentVariable(..., Machine)` ra đúng value | Thủ công |
| AC5 | `GenerateAesKey(512/256/128)` trả base64 giải mã đúng 64/32/16 byte; độ dài ngoài {128,256,512} bị từ chối | Unit (chờ M1), Thủ công |
| AC6 | `GenerateRandomString(n)`: đúng độ dài n, chỉ chữ-số, n ngoài 8–256 bị từ chối; hai lần gọi cho kết quả khác nhau | Unit (chờ M1), Thủ công |
| AC7 | `GenerateUrn("urn:api:")` bắt đầu bằng tiền tố và theo sau là GUID 32 ký tự hex | Unit (chờ M1), Thủ công |
| AC8 | Cặp RSA: key private và public ghi ra file ghép đúng cặp (import private rồi public bằng `RsaCypher.Import*` thành công, public sinh từ cùng một lần sinh); hủy dialog không đổi value | Thủ công |
| AC9 | Sửa value `N1_TEST_A` → đọc lại ra value mới; xóa → biến không còn trên máy, danh sách tải lại không còn dòng đó | Thủ công |
| AC10 | Xóa luôn hỏi Yes/No; chọn No → biến còn nguyên; biến không có tiền tố `BaseRepo__` hiện cảnh báo thêm nêu tên key | Thủ công |
| AC11 | Biến cốt lõi (`Path`, `TEMP`, ...) hiện trong lưới nhưng nút Sửa/Xóa bị chặn với thông báo | Thủ công |
| AC12 | Sửa/xóa biến vừa bị xóa bên ngoài app: báo lỗi, app không crash, danh sách tải lại | Thủ công |
| AC13 | Chạy không quyền admin: thêm/sửa/xóa báo lỗi quyền, danh sách không đổi | Thủ công |
| AC14 | Ô tìm kiếm lọc theo key không phân biệt hoa thường; xóa chữ thì hiện lại đủ | Thủ công |
| AC15 | Tab Mặc định hoạt động y như trước (đọc, kiểm tra màu, Auto, Lưu 6 key) | Thủ công (hồi quy) |
| AC16 | `dotnet build BaseRepo.slnx` 0 Error, không phát sinh warning `WFO1000`; log (Serilog/log4net/console) không chứa value | Build, Thủ công |
| AC17 | Sau khi test xong, mọi biến `N1_TEST_*` đã được xóa khỏi máy | Thủ công |

Checklist thủ công chạy trên máy Windows có quyền Administrator, dùng key giả tiền tố `N1_TEST_`, **không** đụng biến thật; `Path`/`TEMP` chỉ kiểm tra bị chặn, không thử sửa.

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx` · Test: `dotnet test BaseRepo.slnx` (chưa có test project, xem M1; AC đánh "Unit (chờ M1)" kiểm tay lần này).
- Rủi ro cần coder báo lại nếu gặp: (1) Designer chỉnh tay dễ hỏng, nên backup trước; (2) Obfuscar/`PublishSingleFile` không ảnh hưởng vì không dùng reflection/XmlSerializer mới; (3) lỗi quyền khi môi trường dev chạy không admin.
- Đường lùi: tất cả thay đổi gói trong 1 commit trên nhánh `feature/N1-configenv-env-tuy-chinh`, revert commit là hoàn nguyên; mọi biến test được xóa theo AC17.
