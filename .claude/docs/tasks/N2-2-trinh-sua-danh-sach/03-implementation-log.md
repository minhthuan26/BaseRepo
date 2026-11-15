# N2-2 — ConfigEnv: trình sửa từng mục cho Path/PATHEXT/PSModulePath · Implementation Log

- **Người thực hiện:** Antigravity (MCP `use_antigravity`, `mode=accept-edits`, model yêu cầu `gemini-3.8-flash-medium`, `warning: null`), điều phối và kiểm tra bởi Claude · **Ngày:** 2026-11-15 · **Vòng:** 1
- Commit gốc: `903d19210294d9cee4cc259280f515e204f4388d` (N2-1 đã commit ở `c73bdbb` + docs `903d192`) · Nhánh: `feature/N1-configenv-env-tuy-chinh`.
- Chuẩn bị: người dùng chọn "Commit N2-1 rồi chạy N2-2". Cây sạch trước khi giao (còn `redis_8003.conf` bị xóa và `bash.exe.stackdump`, ngoài phạm vi). Build nền: 0 Error, 913 Warning.
- Lưu ý: N2-1 vẫn 🟡 (chưa chạy tay checklist Administrator); N2-2 xây trên code N2-1 chưa được verify thủ công.

## 1. File đã tạo/sửa
Đối chiếu `git status`/`git diff --stat`: đúng 3 file được phép (1 sửa, 2 tạo), không file nào khác.

| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Tạo/Sửa | Tóm tắt |
|---|---|---|
| `EnvListService.cs` | Tạo | Static thuần: `Split`, `Normalize`, `Join`, `FindDuplicates` (OrdinalIgnoreCase, mỗi mục một lần), `FindMissingPaths` (bỏ qua mục có `%`, `Directory.Exists` bọc try), `Validate` (rỗng hoặc > 2047 → lỗi) |
| `FrmEnvListEditor.cs` | Tạo | Form code-only: ListBox, ô nhập, Thêm/Cập nhật/Xóa/Lên/Xuống, vùng cảnh báo trực tiếp (số mục, độ dài/2047, trùng, không tồn tại), Lưu/Hủy; Lưu: Normalize → Validate → cảnh báo Yes/No → `UpdateCoreValue` |
| `UC_ConfigEnvCustom.cs` | Sửa | Bỏ cảnh báo "chưa hỗ trợ"; sau elevated + `FrmEnvTypeKeyConfirm` + `GetCoreRawValue`, nhánh `IsListKey` mở `FrmEnvListEditor`; OK → thông báo nhắc mở lại terminal + `LoadDataAsync`; Abort → tải lại lưới |

## 2. Quyết định kỹ thuật
- Không sửa `CustomEnvService.cs`: `UpdateCoreValue` đã chấp nhận list key (đúng như ghi chú ở N2-1).
- `PATHEXT` bỏ qua kiểm đường dẫn tồn tại ở cả vùng cảnh báo và lúc Lưu.
- Giá trị thô `%...%` được giữ nguyên (Split/Join không mở rộng biến).
- Không có `Console.*`/`Log.*`/`Debug.*`/`Trace.*` trong hai file mới (grep rỗng).

## 3. Điểm lệch so với spec
Coder báo 0 lệch. Claude đọc cả 3 file và không thấy lệch về luồng hay API. Ghi nhận nhỏ (không phải lệch): `Normalize` trim từng mục nên một mục có khoảng trắng đầu/cuối sẽ bị cắt khi lưu; hành vi hợp lý cho Path nhưng đổi chuỗi so với bản gốc.

## 4. Kết quả build
- Coder tự báo: `dotnet build BaseRepo.slnx -m:1` 0 Error, không `WFO1000`; tự chạy harness ngoài repo cho AC1–AC5 (PASS, theo lời coder).
- Claude chạy lại độc lập: ✅ **0 Error**, 913 Warning (bằng nền), không `WFO1000`.

## 5. Việc cần chú ý cho Tester / Reviewer
- Không tin lời coder về AC1–AC5: Tester dựng harness riêng.
- AC6–AC11 cần UI/Administrator: chỉ đối chiếu qua đọc code, ghi rõ chưa chạy.
- Xem xét: `FindMissingPaths` nuốt mọi exception thành "không tồn tại"; `_btnCancel` vừa có `DialogResult.Cancel` vừa có handler Close; `ThrowError` sau `KeyNotFoundException` đóng form với `Abort`; `Validate` không kiểm ký tự `\r\n` (mục nhập từ ô TextBox một dòng nên khó xảy ra, và `UpdateCoreValue` vẫn chặn).
