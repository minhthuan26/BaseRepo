# N2-1 — ConfigEnv: mở khóa sửa biến cốt lõi + backup + khôi phục · Implementation Log

- **Người thực hiện:** Antigravity (qua MCP `use_antigravity`, `mode=accept-edits`), điều phối và kiểm tra bởi Claude · **Ngày:** 2026-11-15 · **Vòng:** 1
- Commit gốc: `18bdd8ece44389e21638e23addbf1e97a72ed2e3` · Nhánh: `feature/N1-configenv-env-tuy-chinh` (thay đổi của coder chưa commit).
- Model yêu cầu: `gemini-3.8-flash-medium`. Lần này kết quả job **không** có cảnh báo "model not found" (`warning: null`), tham số `--model gemini-3.8-flash-medium` có trong lệnh agy; vẫn không có cách xác nhận độc lập model thực chạy ngoài việc agy nhận tham số.
- Chuẩn bị: working tree đã được làm sạch bằng 3 commit docs (`ae6ee4c`, `4e34f3f`, `18bdd8e`) theo lựa chọn của người dùng; còn lại ngoài phạm vi: `redis_8003.conf` bị xóa và `bash.exe.stackdump`. Build nền trước khi giao: 0 Error, 913 Warning.

## 1. File đã tạo/sửa
Đối chiếu bằng `git status` và `git diff --stat`: đúng 7 file được phép (3 sửa, 4 tạo), không có `.resx` mới, không sửa `.csproj`, không có thư mục `Backups/` hay `Logs/` sinh ra trên đĩa (chưa chạy ứng dụng).

| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Tạo/Sửa | Tóm tắt |
|---|---|---|
| `EnvRegistryService.cs` | Tạo | `EnvRegistryValue(Value, Kind)`; `ReadRaw` (HKLM `...\Session Manager\Environment`, `DoNotExpandEnvironmentNames`), `WriteRaw` giữ kiểu, `BroadcastSettingChange` (P/Invoke `SendMessageTimeout`, `WM_SETTINGCHANGE`, tham số `"Environment"`, timeout 5000 ms, nuốt lỗi) |
| `EnvBackupService.cs` | Tạo | `EnvBackupEntry`; `BackupDirectory` = `AppContext.BaseDirectory\Backups\Env`; `CreateBackup`, `TryRead`, `ListBackups` (mới nhất trước), chống trùng tên cùng giây bằng hậu tố `_1`, `_2` |
| `FrmEnvTypeKeyConfirm.cs` | Tạo | Hộp xác nhận mạnh, OK chỉ bật khi gõ đúng tên key (`Ordinal`) |
| `FrmEnvRestorePicker.cs` | Tạo | Chọn một bản backup (Thời gian, Lý do, Kiểu, 80 ký tự đầu), `SelectedEntry` |
| `CustomEnvService.cs` | Sửa | Thêm `ListKeys`, `IsListKey`, `IsElevated`, `GetCoreRawValue`, `UpdateCoreValue`, `RestoreCoreValue`; giữ nguyên `IsProtected`, `Create`, `UpdateValue`, `Delete` |
| `UC_ConfigEnvCustom.cs` | Sửa | Luồng Sửa cho biến cốt lõi, `btn_Restore_Click`, `FrmCustomEnvEditor(..., isCoreEdit)` |
| `UC_ConfigEnvCustom.Designer.cs` | Sửa | Nút `btn_Restore` ("Khôi phục", `Point(555, 8)`, `Size(80, 32)`, tooltip) |

## 2. Quyết định kỹ thuật
- Ghi/đọc biến cốt lõi đi qua registry (`Registry.LocalMachine`), không dùng `Environment.SetEnvironmentVariable`, để giữ kiểu `REG_EXPAND_SZ` và không mở rộng `%...%`.
- `UpdateCoreValue`: kiểm `IsProtected` (không phải biến cốt lõi thì `InvalidOperationException`; **chấp nhận cả key danh sách** để N2-2 dùng lại) → `IsElevated` (không thì `UnauthorizedAccessException`) → `ValidateValue` + không chứa `\r`/`\n` → `ReadRaw` (null thì `KeyNotFoundException`) → `CreateBackup(reason: "Edit")` → `WriteRaw(key, value, kindCũ)` → `BroadcastSettingChange`. Backup lỗi thì ném, **không ghi**.
- `RestoreCoreValue`: kiểm entry, `IsProtected`, `IsElevated` → nếu biến còn thì backup `BeforeRestore` → ghi đúng giá trị và kiểu từ backup → broadcast; nếu biến không còn thì bỏ qua bước backup (đúng spec).
- Không có lệnh `Console.*`/`Log.*`/`Debug.*`/`Trace.*` (kiểm bằng grep: rỗng). `Environment.SetEnvironmentVariable` còn lại chỉ ở `Create`/`UpdateValue` của N1 (biến thường).

## 3. Điểm lệch so với spec
Coder báo 0 điểm lệch. Claude đọc `CustomEnvService.cs`, `EnvRegistryService.cs`, `EnvBackupService.cs` và không thấy lệch về thứ tự bước hay API. Chưa đọc kỹ `UC_ConfigEnvCustom.cs`, `FrmEnvTypeKeyConfirm.cs`, `FrmEnvRestorePicker.cs`: giao cho Reviewer.

## 4. Kết quả build
- Coder tự báo: `dotnet build BaseRepo.slnx -m:1` 0 Error, không `WFO1000`.
- Claude chạy lại độc lập: ✅ **0 Error**, 664 Warning (build tăng dần, không phải ngưỡng so sánh với 913 của build sạch), đếm `WFO1000` = 0.

## 5. Việc cần chú ý cho Tester / Reviewer
- **`EnvRegistryService.ReadRaw` nuốt mọi exception và trả `null`:** lỗi quyền hoặc I/O sẽ bị hiểu thành "biến không còn tồn tại" (`KeyNotFoundException`) ở `GetCoreRawValue`/`UpdateCoreValue`. Đánh giá có chấp nhận được không (spec không nói cụ thể).
- **`TryRead` đọc `Value=` chỉ một dòng:** giá trị có xuống dòng sẽ bị cắt. Spec cấm xuống dòng ở `UpdateCoreValue`, nhưng `RestoreCoreValue` ghi lại nguyên giá trị từ file; kiểm chứng với file backup bị sửa tay.
- **`TryRead` có ba nhánh `DateTime.TryParse`**, nhánh cuối dùng culture hiện hành: kiểm tra lỗi parse với định dạng ngày lạ.
- **`ListBackups` sắp xếp** `CreatedAt` giảm dần rồi `FilePath` giảm dần: hậu tố `_10` so với `_2` theo chuỗi có thể đảo thứ tự (chỉ khi > 9 backup cùng giây, bỏ qua được).
- **`AppContext.BaseDirectory`** khi `PublishSingleFile` chưa kiểm (giả định đã nêu trong spec).
- Tuyệt đối **không** gọi `UpdateCoreValue`/`RestoreCoreValue`/`WriteRaw`/`BroadcastSettingChange` trên key hệ thống thật khi test tự động; chỉ test hàm không tác dụng phụ hoặc key thử `N2_TEST_*` ở registry (và phải xóa sạch sau test).
- AC cần UI/quyền Administrator (AC6–AC13 trong spec) không test tự động được; Reviewer đối chiếu qua đọc code.

## 6. Vòng sửa 1 (sau Review vòng 1: REQUEST_CHANGES, 1 major)
- **Lý do:** `05-review.md` vòng 1: `EnvRegistryService.ReadRaw` nuốt mọi exception và trả `null`, nên `RestoreCoreValue` có thể bỏ qua backup `BeforeRestore` mà vẫn ghi đè (vi phạm R5). Kèm các minor liên quan an toàn mà reviewer đề nghị sửa cùng vòng.
- **Công cụ code:** Antigravity (`gemini-3.8-flash-medium`, `warning: null`), 1 vòng sửa; chỉ 4 file được phép: `EnvRegistryService.cs`, `EnvBackupService.cs`, `CustomEnvService.cs`, `UC_ConfigEnvCustom.cs` không đổi, `UC_ConfigEnvCustom.Designer.cs`.
- **Đã sửa:**
  1. `ReadRaw` chỉ trả `null` khi giá trị thực sự không có; `OpenSubKey` null thì ném `InvalidOperationException`; mọi lỗi đọc khác ném tiếp. Các nơi gọi trong `UC_ConfigEnvCustom.cs` đã có `catch` chung nên không cần sửa.
  2. `RestoreCoreValue`: chỉ bỏ qua backup khi `ReadRaw` trả `null`; lỗi đọc thì dừng, không ghi. Thứ tự giữ nguyên: kiểm tra → `ReadRaw` → `CreateBackup` → `WriteRaw` → broadcast.
  3. Kiểm `Kind` (chỉ `String`/`ExpandString`) trước khi backup/ghi ở cả `UpdateCoreValue` và `RestoreCoreValue`.
  4. `RestoreCoreValue` validate dữ liệu từ file: `ValidateValue`, cấm `\r`/`\n`, từ chối `Key` có khoảng trắng đầu/cuối.
  5. `TryRead` đọc `Value=` từ sau marker đến **hết file** (không cắt một dòng); `CreatedAt` chỉ còn `TryParseExact` + `InvariantCulture`.
  6. `TabIndex` toolbar đánh lại duy nhất: ToggleMask 2, Restore 3, Refresh 4, Add 5, Edit 6, Delete 7.
- **Cố ý không làm:** bọc `Task.Run` quanh `RunWithLoadingAsync` (cùng kiểu với N1, sẽ nêu lại cho Reviewer), các nit (sắp xếp `_10` vs `_2`, style `object? sender`).
- **Claude kiểm lại độc lập:** diff vẫn đúng 7 file của spec (không file mới ngoài danh sách); đọc lại `ReadRaw`, `UpdateCoreValue`, `RestoreCoreValue`, `TryRead` đúng như mô tả; grep `Console./Log./Debug./Trace.` rỗng; `dotnet build BaseRepo.slnx -m:1`: **0 Error**, 913 Warning, `WFO1000` = 0.
- **Việc cho Tester vòng 2:** thêm test cho `TryRead` giá trị nhiều dòng (đọc đúng toàn bộ, không cắt), `ReadRaw` với key thử không tồn tại vẫn trả `null`, và `RestoreCoreValue` từ chối entry có `Key` khoảng trắng hoặc `Value` có xuống dòng (chỉ với key KHÔNG phải biến cốt lõi thật để tránh ghi thật; hoặc đọc code nếu không an toàn).
