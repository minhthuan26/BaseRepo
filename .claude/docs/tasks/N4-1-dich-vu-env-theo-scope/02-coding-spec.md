# N4-1 — ConfigEnv: dịch vụ biến môi trường theo phạm vi User/Machine + backup mọi thao tác · Coding Spec

- **Trạng thái duyệt:** ✅ đã duyệt bởi Minh Thuận ngày 2026-11-16
- **Yêu cầu gốc:** `../N4-configenv-hai-luoi-user-system/01-requirement.md` (đã chốt 2026-11-16) · **Phụ thuộc:** N2-1 (registry/backup/khôi phục), N2-2 · **Làm trước N4-2**

## 1. Phạm vi
Mở rộng tầng dịch vụ của ConfigEnv để làm việc với **hai phạm vi** (`Machine` = HKLM, `User` = HKCU) và với **mọi biến** (không chỉ 12 biến cốt lõi). **Không đổi giao diện** (N4-2 làm). Gồm:
- `EnvScope { Machine, User }`; đọc/ghi/xóa/liệt kê registry thô theo scope, giữ `RegistryValueKind`.
- File backup có trường `Scope`; tên file phân biệt scope; liệt kê backup theo scope (mọi key).
- Hàm dịch vụ mới trong `CustomEnvService`: liệt kê, tạo, sửa, xóa, khôi phục cho **bất kỳ** biến ở **bất kỳ** scope, tự backup trước sửa/xóa/khôi phục.
- Thêm `Backups/` vào `.gitignore` (quyết định người dùng: backup chứa secret plaintext).
- Các hàm N1/N2 hiện có **giữ nguyên chữ ký và hành vi** (Machine) để UI hiện tại tiếp tục biên dịch và chạy; N4-2 mới chuyển UI sang hàm mới.

## 2. Ngoài phạm vi
- Mọi thay đổi giao diện (`UC_ConfigEnvCustom*`, `FrmEnvRestorePicker`, `FrmEnvListEditor`, `FrmCustomEnvEditor`): thuộc N4-2.
- Đổi tab Mặc định, `EnvironmentHelper` (CommonLibs), `system.config`, thêm package, test project, mã hóa/tự xóa backup, biến Process, hive người dùng khác.

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph. Nơi gọi hiện có (UI): `UC_ConfigEnvCustom.cs`, `FrmEnvListEditor.cs`, `FrmEnvRestorePicker.cs` gọi `ReadRaw/WriteRaw`, `EnvBackupService.CreateBackup/ListBackups`, `CustomEnvService.*`; task này **không sửa** các file UI và **không đổi chữ ký** hàm cũ.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `EnvRegistryService` (thêm overload có `EnvScope`) | `CustomEnvService`, `EnvBackupService` | Có (trong task) |
| `EnvBackupEntry` (thêm `Scope` cuối, mặc định `Machine`) | `FrmEnvRestorePicker`, `UC_ConfigEnvCustom` đọc thuộc tính, không dựng entry | Không (tham số mặc định) |
| `EnvBackupService.TryRead/ListBackups` | file backup cũ N2-1 (không có `Scope`) | Có: coi là `Machine` |
| `CustomEnvItem` (thêm tham số cuối có mặc định) | `UC_ConfigEnvCustom` | Không (tham số mặc định) |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Views/ConfigEnv/EnvRegistryService.cs` | Sửa | `EnvScope`, overload `ReadRaw/WriteRaw/DeleteRaw/ListAll` theo scope; hàm cũ gọi overload với `Machine` |
| `SystemManagement/Views/ConfigEnv/EnvBackupService.cs` | Sửa | `Scope` trong entry/file, tên file theo scope, `ListBackups(EnvScope)` mọi key, `TryRead` tương thích file cũ |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` | Sửa | `GetVariables`, `CreateVariable`, `UpdateVariable`, `DeleteVariable`, `RestoreVariable`, `RequiresElevation`; `CustomEnvItem` thêm `Scope`, `Kind` |
| `.gitignore` | Sửa | Thêm dòng `Backups/` (mục riêng, có chú thích) |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
Không có giao diện. API nội bộ (static):

| Hàm | Hành vi | Lỗi |
|---|---|---|
| `EnvRegistryService.ReadRaw(EnvScope, string key)` | Đọc thô + kind; `null` nếu biến không có; không nuốt lỗi đọc | `InvalidOperationException` khi không mở được khóa |
| `EnvRegistryService.WriteRaw(EnvScope, key, value, kind)` | Ghi giữ kind | như `ReadRaw`, `ArgumentException` key rỗng |
| `EnvRegistryService.DeleteRaw(EnvScope, key)` | Xóa giá trị; không có thì không ném | như trên |
| `EnvRegistryService.ListAll(EnvScope)` | Trả `List<(string Key, EnvRegistryValue Value)>` mọi biến (thô, mọi kind) | `InvalidOperationException` |
| `CustomEnvService.GetVariables(EnvScope)` | Liệt kê **mọi** biến scope đó, sắp xếp theo Key không phân biệt hoa thường; không loại default key; `IsProtected`, `NeedsExtraWarning` như cũ; `Value` là chuỗi thô | lỗi registry ném lên |
| `CustomEnvService.CreateVariable(EnvScope, key, value, defaultKeys)` | Kiểm key (không rỗng, không trắng đầu/cuối, ≤255, không `=`/ký tự điều khiển), kiểm trùng **trong cùng scope** (qua `ReadRaw`), **chỉ ở `Machine`** chặn trùng default key; `ValidateValue`; cấm `\r\n`; ghi `String` (hoặc `ExpandString` nếu value chứa `%`); broadcast; **không backup** | `ArgumentException`, `UnauthorizedAccessException` (Machine không elevated) |
| `CustomEnvService.UpdateVariable(EnvScope, key, newValue)` | Mọi key (core hay thường): elevated (chỉ Machine) → `ValidateValue` + cấm `\r\n` → `ReadRaw` (null → `KeyNotFoundException`) → kind phải là `String`/`ExpandString` → **`CreateBackup(scope, "Edit")`** → `WriteRaw` giữ kind → broadcast. Backup lỗi thì không ghi | như N2-1 |
| `CustomEnvService.DeleteVariable(EnvScope, key)` | `IsProtected` → `InvalidOperationException`; elevated (Machine); `ReadRaw` (null → `KeyNotFoundException`); kind hợp lệ; **`CreateBackup(scope, "Delete")`** → `DeleteRaw` → broadcast. Backup lỗi thì không xóa | như trên |
| `CustomEnvService.RestoreVariable(EnvBackupEntry)` | Validate entry (key không rỗng/không trắng đầu-cuối, `ValidateValue`, cấm `\r\n`, kind `String`/`ExpandString`); elevated nếu `entry.Scope == Machine`; `ReadRaw` hiện tại: nếu **có** → kiểm kind rồi `CreateBackup("BeforeRestore")`; nếu **không có** (đã bị xóa) → bỏ qua bước backup, tạo lại; `WriteRaw(entry.Scope, ...)` → broadcast. **Không** yêu cầu biến phải là cốt lõi | như trên |
| `CustomEnvService.RequiresElevation(EnvScope)` | `true` chỉ với `Machine` | — |
| `EnvBackupService.CreateBackup(EnvScope, key, rawValue, kind, reason)` | Ghi `Scope=<Machine\|User>` vào file; tên file `<Scope>_<safeKey>_<yyyyMMdd_HHmmss>[_n].txt`; hàm 4 tham số cũ gọi overload với `Machine` | như N2-1 |
| `EnvBackupService.ListBackups(EnvScope)` | Mọi bản backup hợp lệ của scope, mới nhất trước; file hỏng bị bỏ qua | không ném |

Định dạng file backup (giá trị giả):
```
Key=EXAMPLE_VAR
Scope=User
Kind=ExpandString
CreatedAt=2026-11-16T09:15:00
Reason=Delete
Value=%USERPROFILE%\tools
```
`Scope=` là dòng header tùy chọn khi đọc: **thiếu thì coi là `Machine`**; giá trị lạ thì file bị bỏ qua. `Value=` vẫn là dòng cuối, đọc đến hết file.

## 6. Tên theo convention
- `enum EnvScope { Machine, User }` (namespace `SystemManagement.Views.ConfigEnv`, đặt trong `EnvRegistryService.cs`).
- `EnvRegistryService`: `ReadRaw(EnvScope, string)`, `WriteRaw(EnvScope, ...)`, `DeleteRaw(EnvScope, string)`, `ListAll(EnvScope)`. Hàm cũ (`ReadRaw(string)`, `WriteRaw(string,...)`) giữ nguyên, ủy quyền `EnvScope.Machine`. Hằng đường dẫn: `MachineEnvironmentKeyPath` (giá trị cũ), `UserEnvironmentKeyPath = "Environment"`.
- `EnvBackupEntry(FilePath, Key, Value, Kind, CreatedAt, Reason, EnvScope Scope = EnvScope.Machine)`.
- `CustomEnvItem(Key, Value, IsProtected, NeedsExtraWarning, EnvScope Scope = EnvScope.Machine, RegistryValueKind Kind = RegistryValueKind.String)`.
- Field/biến private `_camelCase`, comment tiếng Việt giải thích lý do (theo style hiện có). Không có `Console.*`/`Log.*`/`Debug.*` và không log giá trị.

## 7. Luồng xử lý dự kiến
1. **Sửa/Xóa:** `Update/DeleteVariable(scope, key, ...)` → kiểm quyền → `ReadRaw(scope, key)` → backup (`Edit`/`Delete`) → `WriteRaw`/`DeleteRaw` → `BroadcastSettingChange`.
2. **Khôi phục:** `RestoreVariable(entry)` → validate → đọc hiện tại → nếu còn thì backup `BeforeRestore` → ghi lại theo `entry.Scope` → broadcast.
3. **Liệt kê backup:** `ListBackups(scope)` quét `Backups/Env/*.txt`, `TryRead` từng file (file cũ không `Scope` = `Machine`).
4. Hàm N1/N2 cũ (`UpdateCoreValue`, `RestoreCoreValue`, `GetCustomEnvs`, `Create`, `UpdateValue`, `Delete`, `ListBackups(string key)`, `CreateBackup` 4 tham số) **không bị đổi hành vi**.

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `TryRead` file backup cũ (không `Scope`) trả entry với `Scope = Machine`; file có `Scope=User` trả `User`; `Scope=Foo` trả `null` | Unit/harness (chờ M1) |
| AC2 | `CreateBackup(User, "N4_TEST_A", ...)` tạo file `User_N4_TEST_A_<ts>.txt` có dòng `Scope=User`; gọi hai lần cùng giây không ghi đè (`_1`) | Unit/harness |
| AC3 | `ListBackups(EnvScope.User)` chỉ trả backup `User`, mới nhất trước, bỏ file hỏng; `ListBackups(Machine)` trả cả file cũ không `Scope` | Unit/harness |
| AC4 | Với key `N4_TEST_*` ở **HKCU** (không cần elevated): `CreateVariable(User, ...)` → `GetVariables(User)` thấy key, kind đúng; trùng trong cùng scope bị chặn; `Value` chứa `%` được ghi `ExpandString` | Harness (registry thật, chỉ key thử) |
| AC5 | `UpdateVariable(User, "N4_TEST_A", ...)`: tạo file backup `Edit` chứa giá trị cũ, giá trị mới ghi đúng, kind giữ nguyên (ExpandString vẫn ExpandString, `%...%` không bị mở rộng) | Harness |
| AC6 | `DeleteVariable(User, "N4_TEST_A")`: tạo backup `Delete`, `ReadRaw` sau đó trả `null`; xóa biến không tồn tại ném `KeyNotFoundException`; xóa biến cốt lõi (vd `Path`) ném `InvalidOperationException` **trước khi chạm registry** | Harness |
| AC7 | `RestoreVariable` backup `Delete` của User: tạo lại biến đúng giá trị + kind; `RestoreVariable` khi biến còn: có thêm backup `BeforeRestore` rồi ghi đè; entry key có khoảng trắng đầu/cuối hoặc value có `\n` bị từ chối, registry không đổi | Harness |
| AC8 | Machine không elevated: `Create/Update/Delete/RestoreVariable(Machine...)` ném `UnauthorizedAccessException` **trước khi** backup/ghi; Machine elevated: chỉ test với key `N4_TEST_*` và dọn sạch | Harness (nếu elevated) / đọc code |
| AC9 | Hàm cũ vẫn như N2-1: `UpdateCoreValue`/`RestoreCoreValue`/`GetCustomEnvs` còn đúng chữ ký; build toàn solution 0 Error; không `WFO1000` | Build |
| AC10 | `git diff` chỉ gồm 4 file ở mục 3; `.gitignore` có `Backups/`; `git check-ignore SystemManagement/bin/Debug/net10.0-windows/Backups/Env/x.txt` báo bị ignore; không `Console./Log./Debug.` | Build/Review |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1`.
- Tester dựng harness console tạm **ngoài repo** (compile trực tiếp 3 file service); **chỉ** dùng key `N4_TEST_*` ở HKCU (an toàn, không cần elevated) và HKLM **chỉ khi** đang elevated; dọn sạch key và file backup của harness sau khi chạy. Tuyệt đối không ghi/xóa biến thật (`Path`, `TEMP`, `BaseRepo__*`...) và không chạy trên HKLM biến thật.
