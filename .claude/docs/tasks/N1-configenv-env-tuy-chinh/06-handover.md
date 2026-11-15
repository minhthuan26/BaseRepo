# N1 — ConfigEnv: env tùy chọn (thêm / sinh value / sửa / xóa) · Bàn giao

- **Ngày:** 2026-11-15 · **Trạng thái:** ✅ xong (code + review + người dùng xác nhận chạy tay checklist đạt ngày 2026-11-15) · **Sprint:** chưa gán

## 1. Tóm tắt
Màn hình ConfigEnv của SystemManagement giờ có `TabControl` hai tab. Tab **Mặc định** giữ nguyên 6 hàng cũ. Tab **Tùy chỉnh** liệt kê mọi biến môi trường **Machine** (trừ 6 key mặc định) với ô tìm kiếm, value được che, và các nút Thêm, Sửa value, Xóa, Làm mới. Khi thêm/sửa, người dùng nhập value tay hoặc sinh tự động (AES key base64, GUID dạng urn, chuỗi ngẫu nhiên, cặp RSA 2048 ghi file). Xóa là xóa thật khỏi máy, luôn có xác nhận Yes/No. Một danh sách 12 biến hệ thống cốt lõi (`Path`, `TEMP`...) chỉ cho xem, khóa sửa/xóa. Không đổi DB, `system.config`, `CommonLibs`, và không thêm package.

## 2. Giải thích luồng xử lý
**Thuật ngữ:** *Machine env* = biến môi trường cấp máy Windows (`EnvironmentVariableTarget.Machine`), ghi cần quyền Administrator (`app.manifest` đã đặt `requireAdministrator`). *Biến cốt lõi* = 12 biến hệ thống bị khóa sửa/xóa (`CustomEnvService.CoreProtectedKeys`, `CustomEnvService.cs:13`).

```
Home → UC_ConfigEnv (TabControl)
        ├─ tabDefault: 6 hàng cũ (không đổi)
        └─ tabCustom: UC_ConfigEnvCustom
              ├─ Load/Làm mới → CustomEnvService.GetCustomEnvs → Environment.GetEnvironmentVariables(Machine)
              ├─ Thêm/Sửa → FrmCustomEnvEditor (nhập/Sinh) → ValidateKey/ValidateValue → Create/UpdateValue → SetEnvironmentVariable(Machine)
              └─ Xóa → ThrowQuestion (Yes/No) → CustomEnvService.Delete → EnvironmentHelper.Delete(Machine)
```

1. **Dựng tab.** `UC_ConfigEnv()` gọi `InitializeComponent()` rồi gắn `UC_ConfigEnvCustom { Dock = Fill }` vào `tabCustom` (`UC_ConfigEnv.cs`, +4 dòng). Designer thêm `tabControl1`/`tabDefault`/`tabCustom` và chuyển `panel3..panel9` vào `tabDefault`, giữ nguyên tên control và handler (`UC_ConfigEnv.Designer.cs:67-119`).
2. **Tải danh sách.** `UC_ConfigEnvCustom_Load` (`UC_ConfigEnvCustom.cs:26`) → `LoadDataAsync` (`:31`) → `FrmMain.RunWithLoadingAsync` gọi `CustomEnvService.GetCustomEnvs(defaultKeys)` (`CustomEnvService.cs:45`) với `defaultKeys = FrmMain.SysConfig.EnvKeys`. Hàm đọc **chỉ scope Machine**, bỏ 6 key mặc định (không phân biệt hoa thường), gắn cờ `IsProtected`/`NeedsExtraWarning`, sắp theo key. `ApplyFilter` (`:50`) lọc theo ô tìm kiếm, `dgv_Envs_CellFormatting` (`:183`) che value, `btn_ToggleMask_Click` (`:73`) bật/tắt che.
3. **Thêm/Sửa.** `btn_Add_Click` (`:85`) / `btn_Edit_Click` (`:94`, chặn biến cốt lõi và hỏi xác nhận nếu key không có tiền tố `BaseRepo__`, `:111`) mở `FrmCustomEnvEditor` (`:207`). Trong form: `BtnGenerate_Click` (`:426`) gọi `EnvValueGenerator.GenerateAesKey/GenerateUrn/GenerateRandomString`; `GenerateRsaKeyPair` (`:455`) sinh cả cặp RSA một lần, mở `SaveFileDialog` cho private rồi public (`:465`, `:475`), hủy dialog thì dừng, không đổi value. `BtnSave_Click` (`:504`) validate bằng `ValidateKey` (rỗng, khoảng trắng đầu/cuối, `=`, ký tự điều khiển, >255, trùng key mặc định hoặc key đã có, `CustomEnvService.cs:70`) và `ValidateValue` (không rỗng, `:104`), rồi `Create` (`:112`) hoặc `UpdateValue` (`:125`, kiểm biến còn tồn tại, ném `KeyNotFoundException` nếu không).
4. **Xóa.** `btn_Delete_Click` (`:126`) → `FrmMain.ThrowQuestion` (`:145`, thêm cảnh báo nêu tên key nếu thiếu tiền tố `BaseRepo__`) → `CustomEnvService.Delete` (`:144`, chặn biến cốt lõi, kiểm tồn tại) → `EnvironmentHelper.Delete(key, Machine)`.
5. **Lỗi.** `SecurityException`/`UnauthorizedAccessException` → thông báo thiếu quyền Administrator (`:160`, `:552`); `KeyNotFoundException` → biến không còn trên máy, tải lại danh sách (`:164`, `:556`); lỗi khác → `ThrowError`. Thông báo chỉ nêu **key**, không nêu value; code mới không có lệnh log/console nào.
6. **Quyền/DB.** Không có DB. Quyền = quyền Administrator của tiến trình (không có phân quyền nội bộ).

## 3. File đã thay đổi
| Đường dẫn | Vai trò |
|---|---|
| `SystemManagement/Views/ConfigEnv/EnvValueGenerator.cs` (tạo) | Sinh value: AES base64 (128/256/512 bit), GUID urn, chuỗi ngẫu nhiên 8–256 (CSPRNG) |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` (tạo) | Liệt kê/validate/ghi/xóa Machine env, danh sách biến cốt lõi, cờ cảnh báo |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` (tạo) | Giao diện tab Tùy chỉnh + `FrmCustomEnvEditor` (form nhập/sinh, RSA) |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.Designer.cs` (tạo) | Toolbar + DataGridView |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.Designer.cs` (sửa) | Thêm TabControl, chuyển panel cũ vào tab Mặc định |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnv.cs` (sửa) | Gắn `UC_ConfigEnvCustom` vào tab Tùy chỉnh (+4 dòng) |

## 4. Thay đổi DB
Không có.

## 5. Cách chạy và kiểm tra
- Build: `dotnet build BaseRepo.slnx` → 0 Error (Claude chạy lại độc lập sau khi coder xong; tester gặp 2 lỗi `OutOfMemoryException` của compiler ở `AdministrativeUnit.Api` ở lần build đầu, không liên quan N1, chạy lại với `-m:1` thì 0 Error).
- Test tự động: **không có test project** (M1 xUnit chưa làm, spec cấm thêm package). Tester dựng harness console tạm ở scratchpad ngoài repo: 49/49 assertion đạt cho `EnvValueGenerator`/`CustomEnvService` (phần không tác dụng phụ).
- **Kiểm tra thủ công (ĐÃ CHẠY, người dùng xác nhận đạt ngày 2026-11-15; chỉ có xác nhận chung, không có biên bản từng AC):** chạy `SystemManagement` bằng tài khoản Administrator, dùng biến giả tiền tố `N1_TEST_`, theo checklist ở mục 8 của [04-test-report.md](04-test-report.md) (AC3 phần User scope, AC4, AC8–AC15, AC17). Chỉ thử Sửa/Xóa trên `N1_TEST_*`; với `Path`/`TEMP` chỉ kiểm tra bị chặn. Xóa hết biến `N1_TEST_*` khi xong (AC17).

## 6. Kết quả kiểm chứng
- Test: PASS cho phần tự động kiểm được (xem `04-test-report.md`); 11 AC cần UI/quyền Administrator: người dùng chạy tay và xác nhận đạt (2026-11-15), chưa có biên bản từng AC.
- Review: APPROVE, không có blocker/major (xem `05-review.md`).
- Đã kiểm độc lập: diff chỉ đúng 6 file được phép; không có lệnh log/console trong code mới.

## 7. Rủi ro và lưu ý
- Bố cục tab/Dock và các luồng ghi Machine env thật đã được người dùng chạy tay xác nhận ổn (2026-11-15). Ghi env Machine có thể làm UI đứng vài giây.
- Danh sách khóa chỉ gồm 12 biến; các biến hệ thống khác (`USERNAME`, `PROCESSOR_IDENTIFIER`...) chỉ bị cảnh báo thêm khi sửa/xóa, không bị chặn. Chỉ thử với `N1_TEST_*`.
- File RSA private ghi plaintext, ACL mặc định, ở thư mục hiện hành theo mặc định của hộp thoại. Code gốc `UC_ConfigEnv` làm y hệt nên không vi phạm spec; AC8 cần xác nhận file nạp lại được bằng `RsaCypher.Import*` (file ghi bằng `Utf8ToByteArray(base64)`).
- Thông tin model: job Antigravity yêu cầu `gemini-3.8-flash-medium` nhưng kèm cảnh báo "không tìm thấy trong `agy models`", nên chưa xác nhận được model nào thực sự chạy. Code đã được kiểm độc lập (diff, build, tester, reviewer), không dựa vào lời tự báo cáo.
- Chỉ một vòng code, không có vòng sửa lỗi.

## 8. Việc tồn đọng / đề xuất tiếp theo
- ~~Chạy tay checklist (xem mục 5)~~ **Đã xong:** người dùng xác nhận đạt 2026-11-15, N1 đánh ✅.
- Minor từ review (chưa sửa, ngoài phạm vi): form Sửa hiện value thật bằng plaintext dù lưới đang che (`UC_ConfigEnvCustom.cs:118`); chưa giới hạn độ dài value (`CustomEnvService.cs`); using thừa (`CommonLibs.Utils` trong `EnvValueGenerator.cs:1`; `System.Data`, `System.ComponentModel`, `SystemManagement.Views` trong `UC_ConfigEnvCustom.cs`).
- Khi có M1 (xUnit): chuyển 5 AC đánh "Unit (chờ M1)" (AC1, AC2, AC5, AC6, AC7) thành test thật cho `EnvValueGenerator`/`CustomEnvService`.
- Thay đổi chưa commit; nhánh `feature/N1-configenv-env-tuy-chinh` (gốc `ea03dfe`). Cần bạn quyết định commit (có thể dùng skill `commit-helper`).

## 9. Liên kết
- Yêu cầu: `01-requirement.md` · Spec: `02-coding-spec.md` · Log: `03-implementation-log.md` · Test: `04-test-report.md` · Review: `05-review.md`
