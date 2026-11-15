# N2 — ConfigEnv: mở khóa sửa biến hệ thống cốt lõi + trình sửa danh sách · Phân tích yêu cầu

- **Người viết:** BA (skill `ba-requirements`) · **Ngày:** 2026-11-15 · **Module:** 6 (SystemManagement) · **Phụ thuộc:** N1

## 1. Bối cảnh và mục tiêu
Tab **Tùy chỉnh** của ConfigEnv (N1) khóa 12 biến hệ thống cốt lõi (`Path`, `PATHEXT`, `ComSpec`, `SystemRoot`, `windir`, `SystemDrive`, `OS`, `TEMP`, `TMP`, `PSModulePath`, `PROCESSOR_ARCHITECTURE`, `NUMBER_OF_PROCESSORS`) chỉ cho xem. Người vận hành (dev/DevOps, máy Windows) cần sửa được chúng, điển hình là **thêm một thư mục vào `Path`**, mà không phải rời sang `sysdm.cpl`.
Thành công khi: người dùng thêm/sửa/xóa/đổi thứ tự một mục của `Path` từng mục một, hệ thống tự sao lưu giá trị cũ, kết quả ghi đúng kiểu registry (`REG_EXPAND_SZ`) và không làm hỏng các mục `%...%`.

## 2. Yêu cầu gốc
> ok, để sửa các biến này hãy thêm quyền đăng nhập bằng tài khoản có quyền admin hoặc chạy ứng dụng với quyền admin

(Câu hỏi trước đó: "vì sao path không thể sửa, ví dụ mình muốn thêm 1 phần tử trong path thì làm thế nào".)

## 3. Hiện trạng
- `SystemManagement/Views/ConfigEnv/CustomEnvService.cs`: `CoreProtectedKeys` (12 key, dòng 13); `UpdateValue`/`Delete` ném `InvalidOperationException` khi `IsProtected`; ghi bằng `Environment.SetEnvironmentVariable(..., Machine)`.
- `UC_ConfigEnvCustom.cs`: nút Sửa chặn biến cốt lõi (`btn_Edit_Click`); `FrmCustomEnvEditor` là form nhập/sinh value dạng một ô text.
- `SystemManagement/app.manifest`: `requireAdministrator` nên tiến trình luôn elevated; **không có** đăng nhập/tài khoản nội bộ.
- `Environment.SetEnvironmentVariable` ghi Machine có thể làm mất kiểu `REG_EXPAND_SZ` (đọc ra đã mở rộng `%...%` rồi ghi lại thành chuỗi thường).
- Chưa có test project (M1 xUnit chưa làm).

## 4. Luồng nghiệp vụ
1. Người dùng chọn một biến cốt lõi trong tab Tùy chỉnh, bấm **Sửa value**.
2. Hệ thống kiểm tra tiến trình đang elevated (`WindowsPrincipal.IsInRole(Administrator)`); không elevated thì chặn kèm thông báo.
3. Hộp xác nhận mạnh: nêu rõ tên key và hậu quả, bắt **gõ lại đúng tên key** mới cho tiếp.
4. Hệ thống **ghi file backup** giá trị cũ (key, giá trị thô chưa mở rộng, kiểu registry, thời gian) vào `Backups/Env/` trước khi ghi; backup lỗi thì không ghi.
5. Biến dạng danh sách `Path`/`PATHEXT`/`PSModulePath` mở **trình sửa từng mục** (mỗi mục `;` một dòng: thêm, sửa, xóa, lên/xuống). Biến cốt lõi còn lại sửa bằng ô text thường.
6. Lưu: ghi vào registry Machine giữ nguyên kiểu giá trị gốc (`REG_EXPAND_SZ`/`REG_SZ`), rồi tải lại danh sách.
7. **Khôi phục:** chọn một biến cốt lõi, bấm **Khôi phục** → danh sách các backup của biến đó (mới nhất trước, hiện thời gian) → chọn một bản → qua cùng bước kiểm tra elevated + gõ lại tên key → hệ thống backup giá trị **hiện tại** rồi ghi lại giá trị của bản đã chọn (đúng kiểu registry đã lưu).

## 5. Quy tắc nghiệp vụ
- R1: Chỉ **sửa value** cho 12 biến cốt lõi; **xóa vẫn bị chặn** (biến thường vẫn xóa theo N1).
- R2: Mở khóa = kiểm tra elevated + gõ lại tên key. Không làm đăng nhập/tài khoản riêng.
- R3: Trình sửa từng mục áp dụng cho `Path`, `PATHEXT`, `PSModulePath`; tách theo `;`, bỏ qua mục rỗng khi lưu, cảnh báo mục trùng và mục là đường dẫn không tồn tại (chỉ cảnh báo, không chặn; mục có `%...%` không kiểm tồn tại).
- R4: Ghi biến cốt lõi phải giữ nguyên kiểu registry gốc (đọc giá trị thô bằng `RegistryValueOptions.DoNotExpandEnvironmentNames`), không làm phẳng `%...%`.
- R5: Bắt buộc có backup thành công trước khi ghi một biến cốt lõi.
- R6: Sau khi ghi, thông báo cho hệ thống biết biến đã đổi (broadcast `WM_SETTINGCHANGE`) để tiến trình mới nhận được; thông báo nêu rõ cần mở lại terminal/ứng dụng.
- R7: Không đổi 6 key mặc định, không ghi `system.config`, không thêm package.
- R8: Thư mục backup là `Backups/Env/` (cạnh file exe), dùng chung cho các module khác nên mỗi module một thư mục con của `Backups/`; file tên `<key>_<yyyyMMdd_HHmmss>.txt`. Chỉ tạo `Backups/Env/`, không tạo thư mục module khác.
- R9: Có nút **Khôi phục** cho biến cốt lõi: chỉ liệt kê backup của đúng biến đó; khôi phục cũng là một lần ghi nên phải qua R2 và R5 (backup giá trị hiện tại trước khi ghi đè).
- R10: Backup lưu giá trị thô, không mã hóa, giữ nguyên kiểu registry; file không bị app tự xóa.

## 6. Trường hợp biên và lỗi
| Tình huống | Hành vi mong đợi |
|---|---|
| Tiến trình không elevated | Chặn sửa biến cốt lõi, thông báo cần chạy bằng quyền Administrator |
| Gõ sai tên key khi xác nhận | Không cho tiếp, giữ nguyên giá trị |
| Không ghi được file backup | Hủy lưu, không đổi biến, báo lỗi |
| Biến bị xóa/đổi bởi tiến trình khác giữa chừng | Báo biến đã đổi, tải lại danh sách, không ghi |
| Mục Path trùng/không tồn tại | Cảnh báo, cho lưu nếu người dùng xác nhận |
| Path sau khi sửa rỗng hoặc dài quá giới hạn | Chặn lưu, báo lỗi |
| Giá trị gốc là `REG_EXPAND_SZ` | Ghi lại đúng `REG_EXPAND_SZ`, mục `%SystemRoot%...` giữ nguyên |
| Ghi registry bị từ chối | Báo thiếu quyền, giá trị cũ còn nguyên |
| Biến chưa có backup nào khi bấm Khôi phục | Thông báo chưa có bản sao lưu, không làm gì |
| File backup hỏng/sai định dạng/key trong file khác key đang chọn | Bỏ qua bản đó và báo, không ghi |
| `Backups/Env/` chưa tồn tại | Tự tạo khi backup lần đầu |

## 7. Dữ liệu và bảo mật
- Không có DB, không dữ liệu y tế/cá nhân. Giá trị `Path` có thể lộ tên thư mục người dùng: backup lưu cục bộ, không gửi đi, không log giá trị ra console/log.
- Quyền = quyền Administrator của tiến trình; không có quyền nội bộ khác.
- Thay đổi `Path`/biến hệ thống ảnh hưởng toàn máy: cần đường lùi bằng file backup (và thao tác khôi phục thủ công từ backup nếu cần).

## 8. Phụ thuộc
- Phụ thuộc **N1** (tab Tùy chỉnh, `CustomEnvService`). Nên chạy tay xong checklist N1 trước.
- Không cần 🔑 hay hạ tầng ngoài.

## 9. Câu hỏi còn mở
Đã chốt trong vòng hỏi 3 (2026-11-15), không còn câu hỏi mở:
- [x] Vị trí backup: `Backups/Env/` (cạnh file exe), vì các module khác cũng sẽ cần backup; thư mục `Backups/` dùng chung.
- [x] Có nút **Khôi phục** trong app.
- [x] Trình sửa từng mục chỉ áp dụng cho 3 key cố định (`Path`, `PATHEXT`, `PSModulePath`).
