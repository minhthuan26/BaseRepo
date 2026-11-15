# N6 — ConfigEnv: loader và chạy nền cho Lưu/Xóa/Khôi phục · Code Review

- **Reviewer:** Claude (chế độ B, tự review) · **Ngày:** 2026-11-16 · **Vòng:** 1

## 1. Phát hiện
| Mức độ | File:dòng | Vấn đề | Gợi ý sửa |
|---|---|---|---|
| minor | FrmCustomEnvEditor.cs:83-84 | Guard `_isSaving` ở `btn_Generate_Click` nằm ngoài spec và thực tế không bao giờ chạm tới: `SetSavingState(true)` đã vô hiệu `grp_Gen` (kéo theo `btn_Generate`) nên không thể click khi đang lưu. Là code thừa. | Xóa guard (hoặc ghi vào log là cố ý phòng thủ). Không chặn merge. |
| minor | FrmCustomEnvEditor.cs:176-181 | Trùng lặp: sau khi `grp_Gen.Enabled` đổi, toggle thêm `btn_Generate`, `cbo_GenType`, `cbo_AesBits`, `txt_GuidPrefix`, `nud_RandomLength` là dư (control con tự bị vô hiệu theo cha). Vô hại, vì designer không đặt `Enabled=false` cho control nào nên restore về `true` đúng trạng thái gốc. | Chỉ giữ `grp_Gen.Enabled`. |
| minor | FrmEnvListEditor.cs:290-327 | Khe giữa hai pha: sau pha 1 form được mở khóa (`finally`) rồi mới hiện hộp Yes/No (`ThrowQuestion`, chủ sở hữu là `FrmMain.Instance`, không phải form này). Nếu form vẫn tương tác được lúc hộp hiện, người dùng có thể bấm Lưu lần nữa (handler tái nhập, `_isSaving=false`) hoặc sửa danh sách; bản ghi dùng snapshot `normalized`/`joinedValue` cũ. Hành vi này **đã có sẵn ở HEAD** (hộp hỏi cũng nằm giữa hai bước) nên không phải hồi quy. | Cân nhắc giữ `_isSaving = true` xuyên suốt đến hết hộp hỏi, hoặc kiểm chứng thủ công khi chạy AC7. |
| minor | FrmCustomEnvEditor.cs:249-265, 326-350; FrmEnvListEditor.cs:~345-380 | Thứ tự `ThrowInfo` đổi: trước đây gọi trong `try`, nay gọi sau `finally` (khi form đã mở khóa). Nội dung và điều kiện giữ nguyên, nhưng trong lúc hộp thông báo hiện, form đã mở khóa (cùng ghi chú trên về chủ sở hữu hộp). Do yêu cầu "Close() không bị OnFormClosing chặn" nên hợp lý. | Chấp nhận; thử thủ công bấm Lưu liên tiếp khi hộp thành công đang hiện. |
| nit | FrmEnvListEditor.cs:176 (không thuộc diff) | `UpdateWarningDisplay` (đã có) vẫn gọi `FindMissingPaths` đồng bộ trên luồng UI mỗi lần danh sách đổi (thêm/sửa/xóa/di chuyển, và lúc mở form). Với đường dẫn mạng vẫn đứng UI, nên N6 chỉ giải quyết đúng lúc Lưu. | Ngoài phạm vi N6; ghi vào roadmap nếu muốn xử lý (task riêng). |
| nit | SetSavingState (cả 2 form) | `UseWaitCursor` đổi cần di chuột để con trỏ cập nhật (quirk WinForms). | Không cần sửa. |

Không có blocker/major.

## 2. Đánh giá theo tiêu chí
- **Đúng spec:**
  - AC2: các lời gọi `UpdateCoreValue` (2 nơi), `UpdateValue`, `Create`, `Delete`, `RestoreCoreValue` và `FindMissingPaths` (khi Lưu) đều nằm trong `Task.Run` bên trong `RunWithLoadingAsync`. Đạt.
  - AC3: lambda nền chỉ dùng `key`/`val`/`joinedValue`/`normalized`/`sysDefaultKeys`/`deleteKey` đã đọc trước, và `_key` (field `readonly string`); không chạm control UI. `missingPaths` gán trong nền, đọc sau `await` nên an toàn bộ nhớ. `selectedEntry` ở Khôi phục (UC_ConfigEnvCustom) lấy trước lambda (không đổi, không phải control). Đạt.
  - `sysDefaultKeys` hoist ra ngoài lambda từ `FrmMain.SysConfig` (trước đó cũng đọc ở UI thread): đúng tinh thần AC3.
  - AC4/AC9: `git diff` chỉ gồm đúng 3 file cho phép; không đụng `FrmMain.cs`, Designer, resx, `CustomEnvService`.
- **Hành vi nghiệp vụ không đổi (so với HEAD):**
  - Thứ tự validate giữ nguyên (core: ValidateValue → CR/LF; thường: ValidateKey (khi thêm) → ValidateValue).
  - Thông báo giữ nguyên (cập nhật core có kèm dòng "mở lại terminal"; "Cập nhật"/"Thêm" biến thường; thông báo lỗi chỉ nêu message, không log giá trị).
  - Nhánh catch: `UnauthorizedAccess/Security` giữ form (không đặt cờ), `KeyNotFoundException` → `Abort` + `Close`, `Exception` chung giữ form. Đạt. `DialogResult.OK` + `Close` ở thành công đạt.
  - FrmEnvListEditor: thêm `using System.Security;` và rút gọn `System.Security.SecurityException` → `SecurityException` (tương đương, cùng file nên chấp nhận).
- **Cờ `_isSaving`:** luôn trả `false` trong `finally` (cả pha FindMissingPaths lẫn pha ghi), `Close()` chỉ gọi sau `finally` nên `OnFormClosing` không tự chặn, kể cả nhánh thành công và Abort. Hủy: nút bị vô hiệu (và `CancelButton` bị vô hiệu nên Esc không đóng được; `btn_Cancel.DialogResult=Cancel` không click được); X/Alt+F4 bị `OnFormClosing` chặn; thêm guard trong `btn_Cancel_Click`. Lưu lặp: guard ở đầu `btn_Save_Click` + nút bị vô hiệu.
- **Khôi phục trạng thái:**
  - FrmEnvListEditor: `SetSavingState(false)` gọi `UpdateButtonStates()` (tính theo selection/text) nên Up/Down/Delete/Update/Add khôi phục đúng.
  - FrmCustomEnvEditor: designer không đặt `Enabled=false` ở control nào; chế độ Sửa dùng `txt_Key.ReadOnly` (không phải Enabled) nên bật lại `Enabled=true` vẫn giữ ReadOnly và BackColor; `isCoreEdit` ẩn bằng `Visible=false` trên `grp_Gen` (cờ Visible không bị đụng) nên không bị mở nhầm. Đạt.
- **Race / vòng đời:** `RunWithLoadingAsync` có `Task.Yield()` + `finally HideLoading`, ngoại lệ từ `Task.Run` truyền qua `await` vào catch. `async void` trong pha FindMissingPaths không có catch: ngoại lệ (nếu có) vẫn nổi lên `ThreadException` giống hành vi đồng bộ ở HEAD, form được mở khóa nhờ `finally`. Form bị Dispose khi đang await: `OnFormClosing` chặn đóng, nên rủi ro thấp (ngoại trừ tắt máy/thoát ứng dụng cũng bị chặn — chấp nhận được trong thao tác ghi vài giây).
- **Đúng convention:** `_isSaving`/`SetSavingState`/`OnFormClosing` khớp spec mục 6 và `convention-winforms.md`; comment tiếng Việt nêu lý do; không `Console.*`/`Log.*`.
- **Hiệu quả:** việc chậm (ghi registry, broadcast, FindMissingPaths) đã rời luồng UI; không thêm cấp phát đáng kể.
- **An toàn:** không log giá trị; thông báo lỗi chỉ nêu key/message; không đổi quyền hay đường dẫn ghi.
- **Dễ bảo trì và đủ test:** không có test UI tự động (đúng spec); AC6–AC8 cần chạy thủ công (quyền Administrator). Xem 04-test-report.md.

<!-- TOOLING:BEGIN -->
## 2b. Phạm vi ảnh hưởng
Không dùng CodeGraph. Đã đọc `FrmMain.RunWithLoadingAsync` (FrmMain.cs:88): chữ ký/hành vi không đổi; nơi gọi khác (`UC_ConfigEnv`, `FrmMain`) không bị ảnh hưởng. Nơi mở 2 form (`UC_ConfigEnvCustom`) chỉ đọc `DialogResult` (OK/Abort/Cancel) — giữ nguyên.
<!-- TOOLING:END -->

## 3. Đề xuất sau (minor, không chặn)
- Xóa guard thừa ở `btn_Generate_Click` và các toggle con thừa trong `SetSavingState` của FrmCustomEnvEditor.
- Khi chạy thủ công AC7, thử bấm Lưu lần hai khi hộp Yes/No hoặc hộp thành công đang hiện để xác nhận không tái nhập.
- Task riêng: đưa `FindMissingPaths` trong `UpdateWarningDisplay` ra khỏi luồng UI.

VERDICT: APPROVE
