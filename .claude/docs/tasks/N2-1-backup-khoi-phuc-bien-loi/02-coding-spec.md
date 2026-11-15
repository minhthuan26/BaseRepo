# N2-1 — ConfigEnv: mở khóa sửa biến cốt lõi + backup + khôi phục · Coding Spec

- **Trạng thái duyệt:** ⬜ chờ duyệt
- **Yêu cầu gốc:** `../N2-configenv-sua-bien-loi/01-requirement.md` (đã chốt 2026-11-15) · **Phụ thuộc:** N1 (code đã xong, chưa commit) · **Task kế tiếp:** N2-2

## 1. Phạm vi
Cho phép **sửa value** các biến cốt lõi **không phải dạng danh sách** (`ComSpec`, `SystemRoot`, `windir`, `SystemDrive`, `OS`, `TEMP`, `TMP`, `PROCESSOR_ARCHITECTURE`, `NUMBER_OF_PROCESSORS`) trong tab Tùy chỉnh, và thêm nút **Khôi phục** cho cả 12 biến cốt lõi.
- Mở khóa = tiến trình đang elevated + người dùng **gõ lại đúng tên key**.
- Trước mỗi lần ghi (sửa hoặc khôi phục) **bắt buộc backup** giá trị hiện tại ra `Backups/Env/` (cạnh file exe); backup lỗi thì không ghi.
- Ghi qua registry Machine, **giữ nguyên kiểu giá trị** (`REG_EXPAND_SZ`/`REG_SZ`), giá trị đọc dạng thô (không mở rộng `%...%`). Sau khi ghi broadcast `WM_SETTINGCHANGE`.
- Ba biến dạng danh sách (`Path`, `PATHEXT`, `PSModulePath`) **vẫn khóa sửa** trong task này (báo "dùng trình sửa danh sách", làm ở N2-2) nhưng **khôi phục được** nếu đã có backup.

## 2. Ngoài phạm vi
- Trình sửa từng mục cho `Path`/`PATHEXT`/`PSModulePath` (N2-2).
- Xóa biến cốt lõi (vẫn chặn). Đổi hành vi biến thường của N1. Ghi `system.config`, DB, thêm package.
- Tự xóa/dọn file backup; mã hóa file backup; thư mục backup của module khác (chỉ tạo `Backups/Env/`).
- Thêm test project (M1 chưa làm) và chỉnh `.gitignore`.

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph; đã `Grep` toàn `SystemManagement/` để tìm nơi dùng `CustomEnvService`/`IsProtected`/`FrmCustomEnvEditor`: chỉ có `UC_ConfigEnvCustom.cs` và `CustomEnvService.cs`. Không có `Backups`/`DllImport`/`Microsoft.Win32` sẵn trong `SystemManagement`.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `CustomEnvService` (thêm method, giữ nguyên `Delete`/`IsProtected`/`UpdateValue`/`Create`) | `UC_ConfigEnvCustom.cs` | Có (cùng task) |
| `FrmCustomEnvEditor` (thêm tham số tùy chọn `isCoreEdit`) | `btn_Add_Click`, `btn_Edit_Click` | Có; hai chỗ gọi cũ giữ nguyên hành vi (giá trị mặc định `false`) |
| `UC_ConfigEnvCustom.Designer.cs` thêm `btn_Restore` | Toolbar | Có |
| Giá trị đọc thô thay cho giá trị đã mở rộng khi mở form sửa biến cốt lõi | Chỉ luồng sửa cốt lõi mới | Không ảnh hưởng luồng N1 |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Views/ConfigEnv/EnvRegistryService.cs` | Tạo | Đọc/ghi giá trị thô + kiểu ở registry Machine; broadcast `WM_SETTINGCHANGE` |
| `SystemManagement/Views/ConfigEnv/EnvBackupService.cs` | Tạo | `EnvBackupEntry` record; tạo backup, liệt kê backup theo key, đọc/parse file backup |
| `SystemManagement/Views/ConfigEnv/FrmEnvTypeKeyConfirm.cs` | Tạo | Hộp thoại cảnh báo + bắt gõ lại đúng tên key |
| `SystemManagement/Views/ConfigEnv/FrmEnvRestorePicker.cs` | Tạo | Hộp thoại chọn một bản backup của một key |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` | Sửa | Thêm `IsElevated`, `IsListKey`, `UpdateCoreValue`, `RestoreCoreValue`, `GetCoreRawValue` |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` | Sửa | Luồng Sửa cho biến cốt lõi, handler `btn_Restore_Click`, tham số `isCoreEdit` của `FrmCustomEnvEditor` |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.Designer.cs` | Sửa | Thêm nút `btn_Restore` |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
Không có API. Giao diện:
| Thành phần | Hành vi | Lỗi |
|---|---|---|
| Nút **Sửa value** với biến cốt lõi không phải list | Kiểm tra elevated → `FrmEnvTypeKeyConfirm` → `FrmCustomEnvEditor(isCoreEdit: true)` hiện giá trị **thô**, ẩn nhóm "Sinh giá trị" → Lưu gọi `UpdateCoreValue` | Không elevated, gõ sai key, backup lỗi, thiếu quyền, biến không còn |
| Nút **Sửa value** với `Path`/`PATHEXT`/`PSModulePath` | Cảnh báo "Biến dạng danh sách sẽ có trình sửa riêng, chưa hỗ trợ ở phiên bản này" | — |
| Nút **Khôi phục** (mới, `x=555`, rộng 80) | Chọn dòng cốt lõi → elevated → `FrmEnvRestorePicker` → `FrmEnvTypeKeyConfirm` → `RestoreCoreValue` | Chọn biến thường, chưa có backup, file backup hỏng |

## 6. Tên theo convention
Theo `convention-backend.md` (PascalCase, field `_camelCase`, tên file = tên class, comment giải thích "vì sao") và style N1 (namespace `SystemManagement.Views.ConfigEnv`, `FrmMain.Throw*` để báo lỗi, không log giá trị).
- `EnvRegistryService` (static): `ReadRaw(string key)` → `EnvRegistryValue?` (record `EnvRegistryValue(string Value, RegistryValueKind Kind)`), `WriteRaw(string key, string value, RegistryValueKind kind)`, `BroadcastSettingChange()`.
- `EnvBackupService` (static): `BackupDirectory`, `CreateBackup(string key, string rawValue, RegistryValueKind kind, string reason)` → đường dẫn file, `ListBackups(string key)` → `List<EnvBackupEntry>` (mới nhất trước), `TryRead(string path)` → `EnvBackupEntry?`; record `EnvBackupEntry(string FilePath, string Key, string Value, RegistryValueKind Kind, DateTime CreatedAt, string Reason)`.
- `CustomEnvService` thêm: `IsElevated()`, `IsListKey(string key)` (`Path`, `PATHEXT`, `PSModulePath`, không phân biệt hoa thường), `GetCoreRawValue(string key)`, `UpdateCoreValue(string key, string newValue)`, `RestoreCoreValue(EnvBackupEntry entry)`.
- `FrmEnvTypeKeyConfirm(string key, string title, string message)` → `DialogResult`; `FrmEnvRestorePicker(string key)` với property `SelectedEntry`.

## 7. Luồng xử lý dự kiến
**Sửa biến cốt lõi (không list):**
1. `btn_Edit_Click`: `IsListKey` → cảnh báo và dừng. `IsProtected` (không list): `!IsElevated()` → báo cần chạy Administrator, dừng.
2. `FrmEnvTypeKeyConfirm` nêu hậu quả toàn máy; nút OK chỉ bật khi ô nhập khớp **đúng** tên key (so sánh `Ordinal`, phân biệt hoa thường). Hủy → dừng.
3. Mở `FrmCustomEnvEditor(isEditMode: true, key, CustomEnvService.GetCoreRawValue(key), isCoreEdit: true)`; `GetCoreRawValue` ném `KeyNotFoundException` nếu biến không còn.
4. `BtnSave_Click` (nhánh `isCoreEdit`): `ValidateValue`, thêm kiểm tra không chứa `\r`/`\n`; gọi `UpdateCoreValue(key, val)`.
5. `UpdateCoreValue`: kiểm `IsProtected` (key không phải biến cốt lõi ném `InvalidOperationException`; **chấp nhận cả key danh sách** vì N2-2 dùng lại, việc chặn sửa list key ở task này nằm ở UI bước 1), `IsElevated()` (không thì `UnauthorizedAccessException`), `ReadRaw` (null → `KeyNotFoundException`), `EnvBackupService.CreateBackup(..., reason: "Edit")` (ném lỗi → dừng, không ghi), `WriteRaw(key, newValue, oldKind)`, `BroadcastSettingChange()`.
6. UI báo thành công kèm nhắc "mở lại terminal/ứng dụng để nhận giá trị mới", tải lại danh sách.

**Khôi phục:** `btn_Restore_Click`: dòng chọn phải `IsProtected` (không thì cảnh báo) → `!IsElevated()` dừng → `FrmEnvRestorePicker(key)` (`ListBackups`, bỏ qua file hỏng hoặc có `Key` khác key đang chọn, mới nhất trước; cột Thời gian, Lý do, Kiểu, 80 ký tự đầu của giá trị) → `FrmEnvTypeKeyConfirm` → `RestoreCoreValue(entry)`: kiểm quyền, `ReadRaw` hiện tại (null → vẫn cho khôi phục như tạo lại biến, bỏ qua backup bước này), backup giá trị hiện tại (`reason: "BeforeRestore"`), `WriteRaw(entry.Key, entry.Value, entry.Kind)`, broadcast.

**Định dạng file backup** `Backups/Env/<key>_<yyyyMMdd_HHmmss>.txt`, UTF-8, tên key đã thay ký tự không hợp lệ cho tên file; tên trùng giây thì thêm hậu tố `_1`, `_2`... Nội dung:
```
Key=<key>
Kind=<ExpandString|String>
CreatedAt=<yyyy-MM-ddTHH:mm:ss>
Reason=<Edit|BeforeRestore>
Value=<giá trị thô, đến hết dòng>
```
`TryRead` trả `null` nếu thiếu trường, `Kind` không parse được, hoặc không phải `ExpandString`/`String`.

## 8. Tiêu chí nghiệm thu
Hàm "key-agnostic" (`EnvRegistryService`, `EnvBackupService`) được kiểm bằng key thử `N2_TEST_*`; **không** dùng biến hệ thống thật cho kiểm tự động.
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `ReadRaw` trả `null` cho key không tồn tại; với key thử kiểu `ExpandString` chứa `%SystemRoot%\x` trả đúng chuỗi thô (không mở rộng) và `Kind == ExpandString` | Unit/harness (chờ M1) |
| AC2 | `WriteRaw` ghi key thử kiểu `ExpandString` rồi `ReadRaw` thấy cùng `Value` và `Kind`; ghi `String` giữ `String` | Unit/harness (chờ M1) |
| AC3 | `CreateBackup` tạo file đúng thư mục `Backups/Env/` (tự tạo nếu chưa có), đúng tên `<key>_<yyyyMMdd_HHmmss>.txt`, nội dung đúng định dạng mục 7; hai lần gọi trong cùng giây không ghi đè nhau | Unit/harness (chờ M1) |
| AC4 | `ListBackups(key)` chỉ trả backup của đúng key, mới nhất trước, bỏ qua file hỏng/thiếu trường/`Kind` sai/`Key` khác tên trong file | Unit/harness (chờ M1) |
| AC5 | `IsListKey("path")` = true (không phân biệt hoa thường), `IsListKey("TEMP")` = false; `UpdateCoreValue("N2_TEST_X", ...)` (key không phải biến cốt lõi) ném `InvalidOperationException`, không tạo backup, không ghi; `UpdateCoreValue` không phân biệt list/không list (UI mới chặn list key) | Unit/harness (chờ M1) |
| AC6 | `UpdateCoreValue` không ghi registry nếu `CreateBackup` ném lỗi (thư mục backup không ghi được) | Thủ công/harness |
| AC7 | `FrmEnvTypeKeyConfirm`: nút OK chỉ bật khi gõ đúng tên key (phân biệt hoa thường); sai thì không đóng với OK | Thủ công |
| AC8 | Không elevated: Sửa/Khôi phục biến cốt lõi bị chặn kèm thông báo; không có backup, không ghi | Thủ công (chạy không elevated bằng cách `runas`/tạm bỏ manifest trên máy thử) |
| AC9 | Sửa một biến cốt lõi an toàn (ví dụ `NUMBER_OF_PROCESSORS`, **sau khi** `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env.reg`): có file backup mới, giá trị mới hiện trong lưới sau Làm mới, kiểu registry không đổi; form sửa hiển thị giá trị thô | Thủ công (cần Administrator) |
| AC10 | Khôi phục bản backup vừa tạo: giá trị và kiểu về như cũ, sinh thêm backup `BeforeRestore`; sau test, giá trị biến thật đúng như trước khi test | Thủ công (cần Administrator) |
| AC11 | Sửa `Path`/`PATHEXT`/`PSModulePath` hiện cảnh báo "chưa hỗ trợ", không mở form sửa; `Xóa` mọi biến cốt lõi vẫn bị chặn như N1 | Thủ công |
| AC12 | Khôi phục khi biến chưa có backup: báo chưa có bản sao lưu, không đổi gì; chọn dòng không phải biến cốt lõi: cảnh báo | Thủ công |
| AC13 | Luồng N1 không hồi quy: Thêm/Sửa/Xóa biến thường `N1_TEST_*` vẫn như cũ; form sinh value hiện đủ nhóm "Sinh giá trị" | Thủ công |
| AC14 | Không có lệnh `Console.*`/`Log.*`/`Debug.*`/`Trace.*` ghi giá trị biến; thông báo lỗi chỉ nêu key | Review/grep |
| AC15 | `git diff` chỉ gồm 7 file ở mục 3; build 0 Error, không phát sinh `WFO1000` | Build |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx` (nếu compiler hết bộ nhớ, thêm `-m:1`). Chưa có test project nên `dotnet test` chưa áp dụng; tester dựng harness console tạm **ngoài repo** cho AC1–AC6 như ở N1.
- Rủi ro cần kiểm khi chạy: `AppContext.BaseDirectory` khi publish `PublishSingleFile` phải trỏ thư mục chứa exe (giả định, chưa kiểm); nếu lệch thì báo lại, không tự đổi vị trí backup.
