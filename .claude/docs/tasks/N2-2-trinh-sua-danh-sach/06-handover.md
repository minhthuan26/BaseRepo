# N2-2 — ConfigEnv: trình sửa từng mục cho Path/PATHEXT/PSModulePath · Bàn giao

- **Ngày:** 2026-11-15 · **Trạng thái:** ✅ xong (người dùng chạy tay checklist đạt 2026-11-16) · **Sprint:** chưa gán

## 1. Tóm tắt
Nút **Sửa value** với ba biến dạng danh sách `Path`, `PATHEXT`, `PSModulePath` không còn bị chặn bằng cảnh báo "chưa hỗ trợ". Sau khi qua bước elevated và gõ lại đúng tên key (như N2-1), form **`FrmEnvListEditor`** mở ra, hiện từng mục `;` một dòng để **thêm, cập nhật, xóa, lên/xuống**. Vùng cảnh báo trực tiếp cho biết số mục, độ dài chuỗi ghép/2047, mục trùng (không phân biệt hoa thường) và đường dẫn không tồn tại (không áp dụng cho `PATHEXT`, bỏ qua mục có `%`). Khi Lưu: bỏ mục rỗng, chặn nếu danh sách rỗng hoặc dài hơn 2047 ký tự, có cảnh báo thì hỏi Có/Không, rồi ghi qua `CustomEnvService.UpdateCoreValue` của N2-1 (backup bắt buộc, giữ kiểu registry, đọc/ghi giá trị thô `%...%`, broadcast). Không đổi DB, `CustomEnvService.cs`, `system.config`, không thêm package.

## 2. Giải thích luồng xử lý
**Thuật ngữ:** *biến danh sách* = `Path`, `PATHEXT`, `PSModulePath` (`CustomEnvService.IsListKey`). *Giá trị thô* = chuỗi registry chưa mở rộng `%...%`.

```
btn_Edit_Click (UC_ConfigEnvCustom)
 └─ biến cốt lõi → IsElevated → FrmEnvTypeKeyConfirm (gõ lại key) → GetCoreRawValue
     ├─ IsListKey → FrmEnvListEditor(key, rawValue) → Lưu
     │     Normalize → Validate → FindDuplicates + FindMissingPaths (bỏ qua với PATHEXT) → hỏi Có/Không
     │     → CustomEnvService.UpdateCoreValue(key, Join(items))  [backup Edit → WriteRaw giữ Kind → Broadcast]
     └─ không phải list → FrmCustomEnvEditor(isCoreEdit)  (luồng N2-1, không đổi)
```

1. **Mở form.** Nhánh `IsListKey` trong `btn_Edit_Click` nằm **sau** `IsElevated`, `FrmEnvTypeKeyConfirm` và `GetCoreRawValue`, rồi `return` nên luồng N2-1 cho biến không phải list không đổi. Kết quả `OK` → thông báo kèm nhắc mở lại terminal + tải lại lưới; `Abort` (biến không còn) → tải lại lưới; `Cancel` → không làm gì.
2. **`EnvListService`** (static, thuần, không UI/registry): `Split` (giữ thứ tự và mục rỗng), `Normalize` (Trim, bỏ rỗng), `Join` (nối `;`, không `;` cuối), `FindDuplicates` (`OrdinalIgnoreCase`, mỗi mục một lần), `FindMissingPaths` (bỏ qua mục có `%`, `Directory.Exists`), `Validate` (rỗng hoặc > 2047 → lỗi, hợp lệ → `null`).
3. **`FrmEnvListEditor(key, rawValue)`:** ListBox + ô nhập + Thêm/Cập nhật/Xóa/Lên/Xuống + vùng cảnh báo + Lưu/Hủy. `PATHEXT` nhận diện bằng `isPathExt` nên không kiểm đường dẫn, cả lúc hiển thị lẫn lúc lưu. Form **không** chạm registry trực tiếp; Hủy/đóng cửa sổ không ghi.
4. **Lỗi khi Lưu:** `UnauthorizedAccessException`/`SecurityException` → báo cần Administrator, giữ form để thử lại; `KeyNotFoundException` → báo, đóng form với `Abort`; lỗi khác → `FrmMain.ThrowError`. Không log giá trị.
5. **DB:** không có.

## 3. File đã thay đổi
| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Vai trò |
|---|---|
| `EnvListService.cs` (tạo) | Logic tách/ghép/kiểm tra danh sách `;` |
| `FrmEnvListEditor.cs` (tạo) | Form sửa từng mục, cảnh báo, Lưu |
| `UC_ConfigEnvCustom.cs` (sửa, +16/−7) | Nhánh `IsListKey` mở `FrmEnvListEditor` thay cho cảnh báo |

## 4. Thay đổi DB
Không có.

## 5. Cách chạy và kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1` → **0 Error**, 913 Warning, không `WFO1000` (Claude chạy lại độc lập).
- Test tự động: repo **chưa có test project** (M1). Tester dựng harness console tạm ngoài repo, compile trực tiếp `EnvListService.cs`: **37/37** kiểm tra AC1–AC5 (+ biên 2047/2048, null/rỗng, trùng hoa thường, `%...%`, PATHEXT-style).
- **Kiểm tra thủ công (CHƯA CHẠY):** AC6–AC11 trên máy Windows có Administrator. Trước khi thử: `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env.reg`; thêm **một thư mục thật** vào `Path`, kiểm tra lưới thấy mục mới ở cuối, các mục `%...%` cũ vẫn là `%...%` khi mở lại form, kiểu vẫn `REG_EXPAND_SZ`; thử Lên/Xuống/Xóa/Cập nhật, cảnh báo trùng/không tồn tại (chọn Không thì không ghi), file `Path_*.txt` ở `Backups/Env/`, nút **Khôi phục** trả `Path` về giá trị trước, không elevated/gõ sai key thì không mở form; hồi quy sửa biến cốt lõi không phải list và biến thường (N1). Sau khi xong **khôi phục `Path` đúng như `env.reg`**.

## 6. Kết quả kiểm chứng
- Test: **PASS** cho phần chạy/đọc code được (37/37); AC6–AC11 chưa chạy, **không tính là PASS** (`04-test-report.md`).
- Review: **APPROVE** vòng 1, 0 blocker/major (`05-review.md`).
- Đã kiểm độc lập: diff đúng 3 file của spec; grep `Console./Log./Debug./Trace.` rỗng; không sửa `CustomEnvService.cs`.

## 7. Rủi ro và lưu ý
- **N2-2 xây trên N2-1 chưa chạy tay:** ghi registry thật bằng code này (cả `UpdateCoreValue` lẫn backup/khôi phục) chưa từng được thử. `Path` là biến nhạy cảm: sai có thể làm hỏng lệnh toàn máy, nên bắt buộc `reg export` trước.
- Sửa biến Machine ảnh hưởng toàn máy; tiến trình đang chạy chỉ nhận giá trị mới sau khi mở lại. Ghi registry/broadcast chạy trên luồng UI nên có thể đứng UI vài giây (cùng pattern N1/N2-1).
- `Normalize` Trim từng mục: mục có khoảng trắng đầu/cuối bị cắt khi lưu (hợp lý cho Path).
- Chỉ **một vòng code**, không vòng sửa. Model `gemini-3.8-flash-medium` được nhận (`warning: null`), không xác nhận độc lập được model thực chạy.
- Thư mục harness tạm của Tester chưa dọn (lệnh xóa bị từ chối quyền), nằm ngoài repo trong scratchpad của phiên; xóa tay được.

## 8. Việc tồn đọng / đề xuất tiếp theo
- **Chạy tay checklist** N2-1 và N2-2 (Administrator), rồi mới đánh ✅.
- Minor chưa sửa (không chặn), nên gom vào một vòng nhỏ sau:
  1. Nhập `a;b` vào ô nhập thành 1 mục trong ListBox nhưng 2 mục khi lưu, nên cảnh báo/độ dài tính sai; nên chặn hoặc tách `;` ở `BtnAdd_Click`/`BtnUpdate_Click`.
  2. `Directory.Exists` chạy đồng bộ trên luồng UI sau mỗi thao tác; đường dẫn UNC/ổ mạng offline có thể đứng UI. Nên cache theo mục hoặc chạy nền.
  3. Lưu khi giá trị không đổi vẫn tạo backup, ghi registry và broadcast; nên so với `rawValue` gốc và bỏ qua.
- Nit: `FindMissingPaths` coi mục có dấu nháy `"C:\..."` là thiếu và có `try/catch` trần thừa; `FindDuplicates` dùng thừa `Dictionary` và không coi `C:\A` với `C:\A\` là trùng; logic Normalize + kiểm tra bị lặp ở `UpdateWarningDisplay` và `BtnSave_Click`; `new Font` không Dispose; Hủy khi sửa dở không hỏi xác nhận.
- Khi có M1 (xUnit): chuyển AC1–AC5 thành test thật cho `EnvListService`.
- Thay đổi chưa commit (3 file code + docs N2-2); nhánh `feature/N1-configenv-env-tuy-chinh`, commit gốc `903d192`. Cần bạn quyết định commit (gợi ý `feat(N2-2): add per-item editor for list env vars`).

## 9. Liên kết
- Yêu cầu: `../N2-configenv-sua-bien-loi/01-requirement.md` · Spec: `02-coding-spec.md` · Log: `03-implementation-log.md` · Test: `04-test-report.md` · Review: `05-review.md`
