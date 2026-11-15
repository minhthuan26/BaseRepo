# N1 — ConfigEnv: env tùy chọn · Test Report

- **Ngày:** 2026-11-15 · **Vòng:** 1
- Phương pháp: repo chưa có test project xUnit và spec cấm thêm. Dùng harness console tạm ở scratchpad (ngoài repo): `C:\Users\X1503Z~1\AppData\Local\Temp\claude\81b2bab8ed1c5b58\54f0e1d4-3bef-445c-ab54-f2537d0b86da\scratchpad\n1-harness` (biên dịch trực tiếp `EnvValueGenerator.cs`, `CustomEnvService.cs` + ProjectReference `CommonLibs`), assert thủ công. Không tạo file nào trong repo.
- An toàn: KHÔNG gọi `Create/UpdateValue/Delete` hay lệnh ghi/xóa env. Chỉ gọi `EnvValueGenerator.*`, `ValidateKey/ValidateValue/IsProtected/NeedsExtraWarning`, `GetCustomEnvs` (chỉ đọc). Value env của máy không được in ra.

## 1. Build
- `dotnet build BaseRepo.slnx`: lần đầu có 2 lỗi `OutOfMemoryException` của compiler tại `AdministrativeUnit.Api` (không liên quan N1, lỗi môi trường tạm thời). Chạy lại với `-m:1`: **0 Error**, 855 Warning, không có `WFO1000`. `SystemManagement.csproj` riêng: 0 Error.

## 2. Đối chiếu tiêu chí nghiệm thu
| Tiêu chí | Test | Kết quả |
|---|---|---|
| AC1 | Harness: key rỗng, toàn khoảng trắng, khoảng trắng đầu/cuối, có `=`, ký tự điều khiển (`\u0001`, tab), dài 256 → bị từ chối; dài 255 và `N1_TEST_A` → hợp lệ; trùng 6 key mặc định (kể cả khác hoa thường); trùng key Machine khác hoa thường (`pAtH`); `defaultKeys=null` không crash | ✅ |
| AC2 | Harness: `""`, `"  \t"` bị từ chối; `"x"` hợp lệ | ✅ |
| AC3 (phần đọc) | Harness: `GetCustomEnvs` với 6 key mặc định: không chứa 6 key; mọi key đều thuộc Machine; số lượng = Machine trừ mặc định (22 key = 28 − 6); không có key chỉ ở User scope (máy có 6 key User-only, không key nào xuất hiện); sắp xếp theo key không phân biệt hoa thường. 6 key mặc định đều có trên Machine nên việc loại trừ được kiểm thật | ✅ (phần User/UI: Thủ công) |
| AC4 | Cần ghi Machine env + quyền Admin | Thủ công (chưa chạy) |
| AC5 | Harness: 512/256/128 → base64 giải ra đúng 64/32/16 byte; mặc định 512; 0/64/192/384/1024/-1 ném `ArgumentException` | ✅ |
| AC6 | Harness: độ dài 8/32/256 đúng, chỉ chữ-số; 7/257/0/-1 bị từ chối; hai lần gọi khác nhau | ✅ |
| AC7 | Harness: bắt đầu `urn:api:`, phần sau đúng 32 ký tự hex; hai lần khác nhau | ✅ |
| AC8 | Cần SaveFileDialog | Thủ công (chưa chạy); kiểm tĩnh bên dưới |
| AC9–AC10 | Cần UI + ghi/xóa Machine env | Thủ công (chưa chạy); kiểm tĩnh bên dưới |
| AC11 | Cần UI | Thủ công (chưa chạy); kiểm tĩnh: `IsProtected` đúng với `path`/`TEMP` (harness ✅) |
| AC12–AC14 | Cần UI/quyền | Thủ công (chưa chạy) |
| AC15 | Hồi quy tab Mặc định cần UI | Thủ công (chưa chạy); kiểm tĩnh bên dưới |
| AC16 | Build 0 Error, không `WFO1000`; grep không thấy log/console/trace trong 3 file mới | ✅ (phần "log không chứa value" runtime: Thủ công) |
| AC17 | Chỉ liên quan khi chạy thủ công AC4/AC9 | Thủ công (chưa chạy) |

## 3. Đối chiếu độ dài AES (mục 4 yêu cầu)
`Cypher.AesCypher.GenerateAesKey(512)` trả **64 byte** (`RandomNumberGenerator.GetBytes(keySize/8)`, `CommonLibs/Utils/Cypher.cs:44-50`); `EnvValueGenerator.GenerateAesKey(512)` giải base64 ra **64 byte**. Không lệch. Cypher chấp nhận thêm 192 bit, code mới chỉ 128/256/512 đúng spec.

## 4. Kiểm tĩnh UI (đọc code)
- (a) Chặn sửa/xóa biến cốt lõi: `UC_ConfigEnvCustom.cs:103-107` (Sửa), `:135-139` (Xóa), và service ném `InvalidOperationException` ở `CustomEnvService.cs:130,149`. Đạt. Ghi chú: danh sách khóa gồm 12 key (Path, PATHEXT, ComSpec, SystemRoot, windir, SystemDrive, OS, TEMP, TMP, PSModulePath, PROCESSOR_ARCHITECTURE, NUMBER_OF_PROCESSORS); các biến hệ thống khác như `PROCESSOR_IDENTIFIER`, `USERNAME`, `DriverData` không bị khóa nhưng vẫn bị cảnh báo thêm vì không có tiền tố `BaseRepo__`. Đây là quyết định thiết kế, không vi phạm AC11 (chỉ nêu `Path`, `TEMP`, ...).
- (b) Xóa luôn hỏi Yes/No qua `ThrowQuestion` (`:145-147`); thêm cảnh báo nêu tên key khi không có tiền tố `BaseRepo__` (`:141-143`); Sửa cũng có cảnh báo (`:109-116`). Đạt.
- (c) Bắt `SecurityException`/`UnauthorizedAccessException` ở Xóa (`:160`) và Lưu (`:552`), thông báo không nêu value; `KeyNotFoundException` → báo lỗi + tải lại (Xóa `:164-168`, Sửa/Lưu `:556-561` rồi list tải lại do `DialogResult.Abort`). Đạt.
- (d) RSA: sinh cả cặp một lần (`GenerateRsaKeyPair`), hai SaveFileDialog liên tiếp, hủy dialog nào cũng `return` trước khi ghi file/đổi `_txtValue` (`:470-471`, `:480-481`); value = đường dẫn private (`:495`), đường dẫn public chỉ nằm trong `ThrowInfo`. Đạt. Ghi chú: dùng `Utf8ToByteArray(base64)` ghi file, theo cách ghi hiện có; tính tương thích khi nạp bằng `RsaCypher.Import*` thuộc AC8, cần kiểm thủ công.
- (e) Tab Mặc định: diff `UC_ConfigEnv.Designer.cs` chỉ thêm TabControl và chuyển `panel3..panel9` vào `tabDefault`; `UC_ConfigEnv.cs` chỉ thêm 4 dòng (khởi tạo/gắn `UC_ConfigEnvCustom` vào `tabCustom`); không đổi tên control hay handler. Đạt.
- (f) Không log value: grep `Log|Console.|Debug.|Trace.` trong 3 file mới = 0 kết quả; thông báo lỗi chỉ nêu key. Đạt.
- Designer của UC_ConfigEnvCustom nối đủ handler: Load, TextChanged, các nút, CellDoubleClick, CellFormatting.

## 5. Kết quả chạy
- Harness: **49 assertion · Đạt: 49 · Lỗi: 0** (`dotnet run`, exit code 0).
- `dotnet test BaseRepo.slnx`: không chạy (chưa có test project, ngoài phạm vi theo spec).

## 6. Test mới đã thêm
- Không thêm file vào repo. Harness tạm chỉ nằm ở scratchpad (ngoài repo), có thể xóa.

## 7. Lỗi
Không có lỗi ở phần tự động kiểm được. Ghi nhận: `using CommonLibs.Utils;` thừa trong `EnvValueGenerator.cs:1` (không ảnh hưởng, ghi cho reviewer).

## 8. Chưa kiểm được (không tính là PASS)
AC3 (phần tạo biến User thật), AC4, AC8, AC9, AC10, AC11 (thao tác UI), AC12, AC13, AC14, AC15, AC17, và phần runtime của AC16 (log không có value) — cần UI WinForms, quyền Administrator và/hoặc ghi biến Machine thật, không được phép tự động. Checklist cho người chạy tay (key giả `N1_TEST_*`, chạy với quyền Admin, không sửa `Path`/`TEMP`):
1. Tạo biến User `N1_TEST_USER` → không hiện trong tab Tùy chỉnh.
2. Thêm `N1_TEST_A` bằng từng kiểu sinh (AES 128/256/512, URN, chuỗi ngẫu nhiên, RSA) → đọc lại Machine env khớp; RSA: hủy dialog giữ nguyên value, import private/public bằng `RsaCypher.Import*` thành công.
3. Sửa value → đọc lại đúng; Xóa → chọn No giữ nguyên, Yes xóa và list tải lại; biến không tiền tố `BaseRepo__` có cảnh báo nêu tên key.
4. `Path`/`TEMP`: Sửa/Xóa bị chặn có thông báo.
5. Xóa biến bên ngoài app rồi Sửa/Xóa → báo lỗi, app không crash, list tải lại.
6. Chạy không Admin: Thêm/Sửa/Xóa báo lỗi quyền, list không đổi.
7. Ô tìm kiếm lọc không phân biệt hoa thường; xóa chữ hiện lại đủ.
8. Tab Mặc định: đọc, kiểm tra màu, Auto, Lưu 6 key như trước.
9. Xóa mọi biến `N1_TEST_*` khi xong; xem log không chứa value.

VERDICT: PASS
(PASS chỉ áp dụng cho phần tự động kiểm được: AC1, AC2, AC3-đọc, AC5, AC6, AC7, AC16-build/grep và kiểm tĩnh. Các AC ở mục 8 chưa kiểm được, cần chạy tay.)
