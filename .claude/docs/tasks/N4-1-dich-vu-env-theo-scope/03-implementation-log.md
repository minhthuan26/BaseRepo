# N4-1 · Implementation Log

- **Công cụ code:** Antigravity (MCP `use_antigravity`), model yêu cầu `gemini-3.8-flash-medium` (server cảnh báo không khớp tên hiển thị trong `agy models`; id có trong danh sách nên chưa rõ model thực chạy). **Số vòng:** 1 (chưa có vòng sửa).
- **Commit gốc:** `ab68c24`. Build nền xanh (0 Error) trước khi giao.

## File đã đổi (đúng 4 file theo spec, +389/−23)
| File | Thay đổi |
|---|---|
| `SystemManagement/Views/ConfigEnv/EnvRegistryService.cs` | `enum EnvScope`, hằng `MachineEnvironmentKeyPath`/`UserEnvironmentKeyPath`, overload `ReadRaw/WriteRaw/DeleteRaw/ListAll` theo scope; hàm cũ ủy quyền Machine |
| `SystemManagement/Views/ConfigEnv/EnvBackupService.cs` | `EnvBackupEntry` thêm `Scope` (mặc định Machine); `CreateBackup(scope,...)`; tên file `<Scope>_<key>_<ts>`; dòng `Scope=`; `TryRead` thiếu Scope = Machine, giá trị lạ → null; `ListBackups(EnvScope)` |
| `SystemManagement/Views/ConfigEnv/CustomEnvService.cs` | `CustomEnvItem` thêm `Scope`, `Kind`; `RequiresElevation`, `GetVariables`, `CreateVariable`, `UpdateVariable`, `DeleteVariable`, `RestoreVariable` |
| `.gitignore` | Thêm mục `Backups/` kèm chú thích |

## Quyết định kỹ thuật / điểm lệch spec
- Không lệch phạm vi. Không có file UI bị đụng; hàm N1/N2 giữ chữ ký.
- Thứ tự kiểm tra: `CreateVariable` kiểm elevation trước validate key (đúng AC8). `DeleteVariable` kiểm `IsProtected` **trước** elevation và trước khi chạm registry (đúng AC6).
- `git check-ignore` cho đường dẫn `SystemManagement/bin/...` báo ignore nhờ luật `bin/` có sẵn, không phải nhờ dòng mới; Tester nên kiểm thêm `git check-ignore -v Backups/Env/x.txt`.
- Dòng cuối cũ của `.gitignore` được thêm newline cuối, vô hại.

## Kết quả build
`dotnet build BaseRepo.slnx -m:1`: 0 Error (Claude chạy lại độc lập). Không có `Console./Log./Debug.` trong diff.

## Cần Tester/Reviewer chú ý
- AC1–AC8 cần harness ngoài repo; chỉ dùng key `N4_TEST_*` ở HKCU; HKLM chỉ khi elevated.
- Reviewer: `RestoreVariable` đọc registry và validate kind trước `CreateBackup`; backup lỗi thì không ghi.
