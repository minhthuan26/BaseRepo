# N2-2 — ConfigEnv: trình sửa từng mục cho Path/PATHEXT/PSModulePath · Coding Spec

- **Trạng thái duyệt:** ⬜ chờ duyệt
- **Yêu cầu gốc:** `../N2-configenv-sua-bien-loi/01-requirement.md` (đã chốt 2026-11-15) · **Phụ thuộc:** **N2-1** (dùng lại `UpdateCoreValue`, `IsListKey`, `FrmEnvTypeKeyConfirm`, backup/registry) · **Làm sau N2-1**

## 1. Phạm vi
Thêm form **sửa từng mục** cho ba biến dạng danh sách `Path`, `PATHEXT`, `PSModulePath` và nối vào nút **Sửa value** (thay cho cảnh báo "chưa hỗ trợ" của N2-1).
- Mỗi mục `;` một dòng: **thêm, sửa, xóa, lên/xuống**.
- Bỏ mục rỗng khi lưu; cảnh báo mục **trùng** (không phân biệt hoa thường) và mục là **đường dẫn không tồn tại** (chỉ cảnh báo, không chặn; mục chứa `%...%` không kiểm tồn tại; `PATHEXT` không kiểm đường dẫn vì là danh sách phần mở rộng).
- Chặn lưu khi danh sách rỗng hoặc chuỗi ghép dài quá **2047** ký tự.
- Lưu đi qua `CustomEnvService.UpdateCoreValue` của N2-1 (elevated, gõ lại tên key, backup, ghi giữ kiểu registry, broadcast).

## 2. Ngoài phạm vi
- Áp dụng cho biến khác chứa `;` ngoài 3 key cố định.
- Xóa biến, hoàn tác nhiều bước trong form, kéo-thả đổi thứ tự (dùng nút Lên/Xuống).
- Thay đổi backup/khôi phục (đã có ở N2-1), thêm package, test project.

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph. Chỉ `UC_ConfigEnvCustom.btn_Edit_Click` gọi vào luồng sửa; `IsListKey` và `UpdateCoreValue` đã được N2-1 cung cấp, task này không sửa `CustomEnvService`.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `UC_ConfigEnvCustom.btn_Edit_Click` (nhánh `IsListKey`) | Luồng sửa biến cốt lõi ở N2-1 | Có: thay cảnh báo bằng mở `FrmEnvListEditor` |
| `EnvListService` (mới, thuần logic) | Không có nơi gọi cũ | Không |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Views/ConfigEnv/EnvListService.cs` | Tạo | Tách/ghép chuỗi `;`, kiểm tra trùng, kiểm đường dẫn tồn tại, kiểm độ dài (logic thuần, không UI, không registry) |
| `SystemManagement/Views/ConfigEnv/FrmEnvListEditor.cs` | Tạo | Form danh sách mục + nút Thêm/Sửa/Xóa/Lên/Xuống, hiện cảnh báo, Lưu |
| `SystemManagement/Views/ConfigEnv/UC_ConfigEnvCustom.cs` | Sửa | Nhánh `IsListKey` trong `btn_Edit_Click` mở `FrmEnvListEditor` sau bước elevated + gõ lại tên key |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
| Thành phần | Hành vi | Lỗi |
|---|---|---|
| Nút **Sửa value** với `Path`/`PATHEXT`/`PSModulePath` | Elevated → `FrmEnvTypeKeyConfirm` → `FrmEnvListEditor(key, CustomEnvService.GetCoreRawValue(key))` | Như N2-1 |
| `FrmEnvListEditor` | Một `ListBox` các mục (giá trị thô, giữ `%...%`), ô nhập mục, nút Thêm / Cập nhật mục chọn / Xóa / Lên / Xuống; vùng cảnh báo; Lưu / Hủy | Danh sách rỗng, quá dài, không ghi được |
| Lưu | Hiện danh sách cảnh báo (trùng/không tồn tại) và hỏi Yes/No nếu có; Yes → `UpdateCoreValue(key, EnvListService.Join(items))` | Lỗi như N2-1 |

## 6. Tên theo convention
- `EnvListService` (static): `Split(string rawValue)` → `List<string>` (giữ nguyên thứ tự và mục rỗng), `Normalize(IEnumerable<string>)` (Trim, bỏ rỗng), `Join(IEnumerable<string>)` → chuỗi nối `;`, `FindDuplicates(IEnumerable<string>)`, `FindMissingPaths(IEnumerable<string>)` (bỏ qua mục có `%`), `Validate(IEnumerable<string> items)` → `string?` lỗi chặn.
- `FrmEnvListEditor(string key, string rawValue)` kế thừa `Form`, namespace `SystemManagement.Views.ConfigEnv`; field private `_camelCase`.

## 7. Luồng xử lý dự kiến
1. `btn_Edit_Click` với `IsListKey(key)`: `!IsElevated()` dừng → `FrmEnvTypeKeyConfirm` → mở `FrmEnvListEditor` với `GetCoreRawValue(key)`.
2. Form tách bằng `Split`, hiển thị từng mục. Người dùng chỉnh: Thêm (cuối danh sách), Cập nhật (mục chọn), Xóa, Lên/Xuống.
3. Bấm **Lưu**: `Normalize` → `Validate` (rỗng/quá dài chặn); `FindDuplicates` + `FindMissingPaths` (chỉ `Path`/`PSModulePath`) → nếu có thì liệt kê và hỏi xác nhận; đồng ý → `UpdateCoreValue(key, Join(items))` (backup `Edit`, ghi giữ kiểu, broadcast).
4. Thành công: thông báo kèm nhắc mở lại terminal; đóng form; lưới tải lại. Lỗi quyền/biến không còn xử lý như N2-1.

> Lưu ý cho coder: theo spec N2-1, `UpdateCoreValue` đã chấp nhận mọi biến cốt lõi (kể cả list), chỉ UI N2-1 chặn list key. Nếu thực tế `UpdateCoreValue` đang từ chối list key thì **dừng và báo lại**, không tự sửa `CustomEnvService.cs` (ngoài danh sách file).

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `Split("a;b;;c")` trả 4 phần tử `a,b,"",c`; `Normalize` trả `a,b,c`; `Join` trả `a;b;c`; không thêm `;` cuối | Unit/harness (chờ M1) |
| AC2 | Round-trip `Join(Normalize(Split(x)))` giữ nguyên chuỗi chứa `%SystemRoot%\system32;C:\dir` | Unit/harness (chờ M1) |
| AC3 | `FindDuplicates` coi `C:\A` và `c:\a` là trùng, không phân biệt hoa thường; trả mỗi mục trùng một lần | Unit/harness (chờ M1) |
| AC4 | `FindMissingPaths` bỏ qua mục chứa `%`, trả mục là thư mục không tồn tại; không áp dụng cho `PATHEXT` | Unit/harness (chờ M1) |
| AC5 | `Validate` trả lỗi khi danh sách rỗng hoặc chuỗi ghép > 2047 ký tự; trả `null` khi hợp lệ | Unit/harness (chờ M1) |
| AC6 | Thêm một thư mục thật vào `Path` qua form (sau `reg export` sao lưu registry): lưới sau Làm mới thấy mục mới ở cuối; các mục `%...%` cũ không bị mở rộng thành đường dẫn cứng (form sửa lại vẫn thấy `%...%`); kiểu registry vẫn `REG_EXPAND_SZ` | Thủ công (cần Administrator) |
| AC7 | Lên/Xuống/Xóa/Cập nhật mục đổi đúng thứ tự/nội dung trong form và trong giá trị lưu | Thủ công |
| AC8 | Mục trùng hoặc không tồn tại: hiện cảnh báo, chọn Không thì không ghi, chọn Có thì ghi | Thủ công |
| AC9 | Có file backup `Path_*.txt` ở `Backups/Env/` sau khi lưu; nút Khôi phục (N2-1) trả `Path` về giá trị trước khi sửa (đối chiếu từng mục) | Thủ công (cần Administrator) |
| AC10 | Không elevated hoặc gõ sai tên key: không mở form, không ghi, không backup | Thủ công |
| AC11 | Luồng N2-1 và N1 không hồi quy (sửa biến cốt lõi không phải list, sửa/xóa biến thường) | Thủ công |
| AC12 | `git diff` chỉ gồm các file ở mục 3 (trên nền N2-1); build 0 Error; không `WFO1000`; không log giá trị | Build/Review |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx` (`-m:1` nếu compiler hết bộ nhớ). Tester dựng harness console tạm **ngoài repo** cho AC1–AC5 (chỉ `EnvListService`, không chạm registry thật).
- Trước khi test thủ công AC6/AC9: chạy `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env.reg`, và sau khi xong **khôi phục lại `Path` đúng như trước** (đối chiếu với `env.reg`).
