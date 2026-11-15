# N1 — ConfigEnv: env tùy chọn · Implementation Log

- **Người thực hiện:** Antigravity (qua MCP `use_antigravity`, `mode=accept-edits`), điều phối và kiểm tra bởi Claude · **Ngày:** 2026-11-15 · **Vòng:** 1
- Commit gốc: `ea03dfe8660ac5364083fb964261bab4d4e36021` · Nhánh: `feature/N1-configenv-env-tuy-chinh` (chưa commit thay đổi của coder).
- Model yêu cầu: `gemini-3.8-flash-medium` (id có trong `antigravity_models`). Kết quả job kèm cảnh báo "Model 'gemini-3.8-flash-medium' was not found in `agy models` output" nên **chưa xác nhận được model nào thực sự chạy**.

## 1. File đã tạo/sửa
Đối chiếu bằng `git status`/`git diff --stat`: đúng 6 file được phép, không có file nào khác trong `SystemManagement/` bị đổi.

| Đường dẫn | Tạo/Sửa | Tóm tắt |
|---|---|---|
| `SystemManagement/Views/ConfigEnv/EnvValueGenerator.cs` | Tạo | Static: `GenerateAesKey(bits)` (128/256/512), `GenerateUrn(prefix)`, `GenerateRandomString(length)` (8–256) |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` | Tạo | `CustomEnvItem` record; `GetCustomEnvs`, `ValidateKey`, `ValidateValue`, `Create`, `UpdateValue`, `Delete`, `IsProtected`, `NeedsExtraWarning` (Machine scope, danh sách biến cốt lõi bị khóa) |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` | Tạo | UserControl tab Tùy chỉnh: lọc, hiện/ẩn value, Thêm/Sửa/Xóa, luồng RSA; chứa thêm `EnvRowViewModel` (private) và `FrmCustomEnvEditor` (internal Form) |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.Designer.cs` | Tạo | Toolbar + DataGridView (Key, Giá trị che, Loại) |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.Designer.cs` | Sửa | Thêm `TabControl` (`tabDefault`, `tabCustom`), chuyển panel3..panel9 vào tab Mặc định (+59/−7 dòng gộp cả hai file sửa) |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.cs` | Sửa | Khởi tạo `UC_ConfigEnvCustom` (`Dock=Fill`) gắn vào `tabCustom` (+4 dòng) |

## 2. Quyết định kỹ thuật
- Dùng `Environment.GetEnvironmentVariables/GetEnvironmentVariable(..., Machine)` trực tiếp cho liệt kê/validate/ghi; chỉ dùng `EnvironmentHelper.Delete` cho xóa (đúng phần 2b của spec, tránh `GetAll`/`Exists` sai scope).
- Form nhập/sửa value là `FrmCustomEnvEditor`, đặt cùng file `UC_ConfigEnvCustom.cs` để không vượt danh sách 6 file.
- `GenerateAesKey` dùng `RandomNumberGenerator.GetBytes(bits/8)` + `Convert.ToBase64String`, không gọi `Cypher.AesCypher.GenerateAesKey`.
- Không có lệnh log/console nào trong code mới (kiểm bằng grep: không có `Log.`/`ILogger`/`Console.`/`Debug.`/`Trace.`).

## 3. Điểm lệch so với spec
- Spec mục 3 liệt kê 6 file và nói form nhập "mở form"; coder thêm 2 class phụ (`EnvRowViewModel`, `FrmCustomEnvEditor`) trong `UC_ConfigEnvCustom.cs` thay vì file riêng. Không vượt phạm vi file, chấp nhận.
- Thông tin mà Tester/Reviewer cần kiểm lại thay vì tin: cảnh báo model, và việc `GenerateAesKey` không dùng lại `Cypher` (xem mục 5).

## 4. Kết quả build
- Coder tự báo: `dotnet build BaseRepo.slnx` 0 Error, không có `WFO1000`.
- Claude chạy lại độc lập: `dotnet build BaseRepo.slnx` ✅ **0 Error**, 664 Warning (cơ sở trước khi giao: 913 Warning, 0 Error; không có `WFO1000`).

## 5. Việc cần chú ý cho Tester / Reviewer
- **Độ dài AES key:** đối chiếu `Cypher.AesCypher.GenerateAesKey(512)` (dùng ở AccessKey/RefreshKey hiện tại) trả bao nhiêu byte; code mới sinh 64 byte cho 512 bit (AC5). Nếu `Cypher` hiểu `512` khác thì mặc định "như AccessKey" của yêu cầu có thể lệch.
- `using CommonLibs.Utils;` thừa trong `EnvValueGenerator.cs` (không dùng).
- Luồng RSA (AC8): cần kiểm một lần bấm sinh cả cặp khớp nhau, value là đường dẫn file private, hủy dialog không đổi value; đối chiếu với `RsaCypher.GenerateKeys/ExportPrivateKey/ExportPublicKey`.
- UI/Designer và các luồng ghi Machine env (AC3, AC4, AC9–AC15) **không test tự động được** (chưa có test project, cần quyền Administrator); chỉ review code và test lớp thuần.
- Hồi quy tab Mặc định (AC15): kiểm các control `txt_*`/`btn_*` vẫn nằm trong tab Mặc định, tên không đổi, handler không mất.
- Tuyệt đối không chạy thao tác ghi/xóa biến Machine **thật** khi test tự động; chỉ test hàm không có tác dụng phụ.
