# N2-2 — ConfigEnv: trình sửa từng mục cho Path/PATHEXT/PSModulePath · Test Report

- **Ngày:** 2026-11-15 · **Vòng:** 1
- Phương pháp: harness console tạm ngoài repo (compile trực tiếp `EnvListService.cs`, không đụng registry/env thật, không gọi `UpdateCoreValue`/`RestoreCoreValue`/`WriteRaw`/Broadcast) + đọc code đối chiếu. Repo chưa có test project (spec mục 2 loại trừ).

## 1. Đối chiếu tiêu chí nghiệm thu
| Tiêu chí | Cách kiểm | Kết quả |
|---|---|---|
| AC1 | Harness: `Split("a;b;;c")`=a,b,"",c; Normalize=a,b,c; Join=`a;b;c`, không `;` cuối | ✅ PASS |
| AC2 | Harness: round-trip `%SystemRoot%\system32;C:\dir` giữ nguyên, `%...%` không bị mở rộng | ✅ PASS |
| AC3 | Harness: `C:\A` vs `c:\a` trùng; trả một lần dù lặp 3 lần; trùng sau Trim | ✅ PASS |
| AC4 | Harness: bỏ qua mục có `%`, trả thư mục không tồn tại, mục rỗng bị bỏ qua, ký tự không hợp lệ không ném lỗi. `PATHEXT` bỏ qua kiểm path: `EnvListService` không tự biết key (3 mục `.COM;.EXE;.BAT` sẽ bị coi là thiếu), nhưng `FrmEnvListEditor` có `isPathExt` và gán rỗng ở cả `UpdateWarningDisplay` lẫn `BtnSave_Click` (đã đọc code) | ✅ PASS |
| AC5 | Harness: rỗng/null/toàn trống -> lỗi; đúng 2047 -> null (hợp lệ); 2048 -> lỗi; tính cả dấu `;` (1000+1046+1=2047 ok, 1000+1047+1=2048 lỗi) | ✅ PASS |
| AC6 | Cần Administrator + registry thật | ⬜ Chưa chạy, cần thủ công. Bằng code: giá trị thô lấy qua `GetCoreRawValue`, `Split` giữ `%...%`; ghi qua `UpdateCoreValue` -> `WriteRaw(key, value, current.Kind)` giữ kiểu |
| AC7 | Cần UI | ⬜ Chưa chạy, cần thủ công. Code Lên/Xuống/Xóa/Cập nhật (`BtnUp/Down/Delete/Update_Click`) dùng hoán đổi/Insert/RemoveAt đúng chỉ số, lưu theo thứ tự ListBox: hợp lý |
| AC8 | Cần UI | ⬜ Chưa chạy, cần thủ công. Code: có duplicates/missing -> `ThrowQuestion`, `!confirmed` -> return trước `UpdateCoreValue`: đúng |
| AC9 | Cần Administrator | ⬜ Chưa chạy, cần thủ công. Code: `UpdateCoreValue` gọi `CreateBackup(..., "Edit")` trước `WriteRaw`, backup lỗi thì dừng |
| AC10 | Cần UI | ⬜ Chưa chạy, cần thủ công. Code: trong `btn_Edit_Click` thứ tự `IsElevated` -> `FrmEnvTypeKeyConfirm` (khác OK thì return) -> `GetCoreRawValue` -> mới `new FrmEnvListEditor`; nhánh list nằm sau cả hai bước chặn |
| AC11 | Cần UI | ⬜ Chưa chạy, cần thủ công. Code: nhánh list `return` sớm, luồng core không phải list (`FrmCustomEnvEditor`) và nhánh biến thường không bị đổi trong diff |
| AC12 | `git diff`/status + build | ✅ PASS (xem dưới) |

## 2. Kết quả chạy
- Harness: 37 kiểm tra · Đạt: 37 · Lỗi: 0 (harness chưa được dọn: lệnh xóa bị từ chối quyền; thư mục tạm nằm ngoài repo, trong scratchpad của phiên, có thể xóa tay).
- Build: `dotnet build BaseRepo.slnx -m:1` -> **0 Error**, 710 Warning (cảnh báo có sẵn), **không có WFO1000**.
- `git status`: chỉ `UC_ConfigEnvCustom.cs` (sửa), `EnvListService.cs`, `FrmEnvListEditor.cs` (mới) — đúng danh sách mục 3 spec.
- Không có `Console.*`/`Log.*`/`Debug.*`/`Trace.*` trong thư mục `Views/ConfigEnv`: không log giá trị.
- `UpdateCoreValue` không từ chối list key (chỉ cần `IsProtected` + elevated), khớp lưu ý spec.
- Lưu chỉ đi qua `CustomEnvService.UpdateCoreValue(_key, joinedValue)`; Hủy chỉ đặt `DialogResult.Cancel` + Close, không ghi; `KeyNotFoundException` -> `Abort` để lưới tải lại.

## 3. Test mới đã thêm
- Không thêm vào repo (harness tạm nằm ngoài repo, chưa xóa được; repo không có test project, spec không cho thêm).

## 4. Nhận xét (không phải lỗi chặn)
| Điểm | Mô tả | Đánh giá |
|---|---|---|
| Normalize Trim đổi nội dung mục | `" C:\a "` -> `C:\a`; mọi mục khác không đổi. Kiểm chứng bằng harness | Chấp nhận được: khoảng trắng đầu/cuối của mục Path vô hiệu trong thực tế, và đây là quy tắc "chuẩn hóa" ở spec. Round-trip AC2 chỉ đảm bảo khi mục không có khoảng trắng đầu/cuối |
| Mục nhập chứa `;` | Người dùng gõ `a;b` vào ô nhập sẽ là 1 mục trong ListBox nhưng thành 2 mục khi lưu/mở lại; cảnh báo/trùng tính theo mục chưa tách | Nhẹ, cân nhắc chặn/tách `;` ở lần sau; không thuộc AC |
| Cảnh báo đường dẫn tính theo từng mục đã Trim | Khớp với giá trị thực sẽ lưu | OK |

## 5. Phần chưa chạy (cần người làm thủ công)
AC6, AC7, AC8, AC9, AC10, AC11 (UI/Administrator/registry thật). Nhớ `reg export` trước và khôi phục `Path` sau khi test AC6/AC9.

## 6. Lỗi
Không có lỗi trong phạm vi đã chạy.

VERDICT: PASS
