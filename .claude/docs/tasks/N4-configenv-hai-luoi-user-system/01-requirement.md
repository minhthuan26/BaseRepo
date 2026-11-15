# N4 — ConfigEnv: hai lưới System / User, hiện cả biến BaseRepo, backup mọi thao tác · Phân tích yêu cầu

- **Người viết:** BA (skill `ba-requirements`) · **Ngày:** 2026-11-16 · **Module:** 6 (SystemManagement)
- **Trạng thái:** ✅ đã chốt bởi Minh Thuận ngày 2026-11-16.

## 1. Bối cảnh và mục tiêu
Tab **Tùy chỉnh** hiện chỉ có một lưới biến **Machine** và cố tình loại 6 key `BaseRepo__*` mặc định (chúng được quản lý riêng ở tab Mặc định). Người dùng muốn thấy đủ: cả biến cấp **User** lẫn **System**, và thấy luôn các biến `BaseRepo__*`. Mục tiêu: một chỗ để xem/sửa mọi biến môi trường quan trọng của Windows theo đúng hai phạm vi, an toàn nhờ backup trước mọi thay đổi.

## 2. Yêu cầu gốc
> "mở rộng thêm chức năng ConfigEnv, đọc cả những biến có BaseRepo vào và chia thành 2 lưới, user và system"

Các lựa chọn đã chốt qua hỏi đáp (2026-11-16): "User/System" = phạm vi Windows User (HKCU) và Machine (HKLM); hiện cả 6 key mặc định trong lưới; lưới User đầy đủ gồm backup/khôi phục; biến trùng tên ở cả hai phạm vi **để Windows tự xử lý**, không đối chiếu; 6 key mặc định **sửa và xóa như biến thường**; backup áp dụng cho **mọi sửa/xóa ở cả hai lưới**; biến cốt lõi ở User dùng chung quy tắc (gõ lại tên key, `Path` mở trình sửa từng mục); bố cục hai lưới trên/dưới, chung thanh công cụ.

## 3. Hiện trạng
- `UC_ConfigEnv.cs`: `TabControl` có tab **Mặc định** (6 ô nhập cố định, sinh AES/RSA, ghi Machine qua `EnvironmentHelper`) và tab **Tùy chỉnh**.
- `UC_ConfigEnvCustom.cs`: một lưới `dgv_Envs`, 3 cột Key/Value/Type, tìm kiếm, Mask/Unmask, Refresh, Thêm, Sửa, Xóa, Khôi phục. Nguồn `CustomEnvService.GetCustomEnvs(defaultKeys)` chỉ đọc `EnvironmentVariableTarget.Machine` và **loại** 6 default key.
- `CustomEnvService`: 12 biến cốt lõi (`CoreProtectedKeys`), 3 biến danh sách (`Path`, `PATHEXT`, `PSModulePath`); sửa biến cốt lõi qua `UpdateCoreValue` (elevated, backup, `EnvRegistryService` HKLM, giữ `RegistryValueKind`, broadcast). Biến thường dùng `Environment.Set/GetEnvironmentVariable(..., Machine)`.
- `EnvRegistryService`: chỉ `Registry.LocalMachine`. `EnvBackupService`: `Backups/Env/<key>_<yyyyMMdd_HHmmss>.txt`, chỉ cho biến cốt lõi, file **không có trường scope**; `RestoreCoreValue` từ chối biến không phải cốt lõi.
- Chưa có code nào đọc/ghi `HKCU\Environment`. Tên 6 key mặc định nằm ở `Config/system.config` (`FrmMain.SysConfig.EnvKeys`).

## 4. Luồng nghiệp vụ
1. Mở tab **Tùy chỉnh**: hai lưới xếp trên/dưới. **System** (Machine, HKLM) ở trên, **User** (HKCU) ở dưới, mỗi lưới có tiêu đề rõ.
2. Hai lưới hiện **mọi** biến của phạm vi tương ứng, kể cả 6 key `BaseRepo__*` mặc định (ở System nếu đang tồn tại ở Machine).
3. Thanh công cụ chung: ô tìm kiếm lọc cả hai lưới, Mask/Unmask áp cho cả hai, Làm mới tải lại cả hai, **Thêm/Sửa/Xóa tác động lưới đang được chọn** (lưới nhận focus gần nhất). **Thêm** vào lưới đang chọn tạo biến ở phạm vi đó. Nút **Khôi phục** dùng chung.
4. **Sửa/Xóa** một biến (ở bất kỳ lưới): tự **backup** giá trị hiện tại (kèm phạm vi và kiểu registry) rồi mới ghi/xóa. Backup lỗi thì **không** ghi/xóa.
5. **Khôi phục**: chọn một bản backup (hiện phạm vi, key, thời điểm, lý do) → xác nhận → ghi lại đúng giá trị, kiểu và phạm vi; nếu biến đang tồn tại thì backup `BeforeRestore` trước; nếu biến đã bị xóa thì tạo lại.
6. Biến cốt lõi (12 key) ở **cả hai** phạm vi: không xóa; sửa phải gõ lại đúng tên key; `Path`/`PATHEXT`/`PSModulePath` mở trình sửa từng mục (`FrmEnvListEditor`). Sửa phạm vi Machine vẫn cần elevated; phạm vi User **không** cần.

## 5. Quy tắc nghiệp vụ
- R1: Hai lưới độc lập: System = `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`, User = `HKCU\Environment`. Không đọc biến Process.
- R2: **Không** đối chiếu, cảnh báo hay chặn khi cùng tên ở cả hai phạm vi; Windows tự ghép (User ghi đè Machine, riêng `Path` nối).
- R3: Lưới System **không còn loại** 6 key mặc định; chúng hiện, **sửa được và xóa được như biến thường** (không có nhãn cấm). Tab **Mặc định** giữ nguyên (sinh AES/RSA, chọn file, Lưu).
- R4: Đọc/ghi mọi biến (cả biến thường) qua registry thô, **giữ nguyên `RegistryValueKind`** (String/ExpandString), không mở rộng `%...%`; sau ghi/xóa broadcast `WM_SETTINGCHANGE`.
- R5: **Backup bắt buộc** trước mỗi sửa/xóa ở cả hai lưới, cho mọi biến (cốt lõi hay thường), lý do `Edit`/`Delete`/`BeforeRestore`. Thêm mới không cần backup.
- R6: File backup thêm trường `Scope=Machine|User`. File cũ không có `Scope` coi là `Machine`. Vẫn nằm ở `Backups/Env/` (thư mục dùng chung cho các module khác); tên file phân biệt phạm vi để không trùng.
- R7: **Khôi phục** hỗ trợ mọi key (không còn giới hạn biến cốt lõi) ở cả hai phạm vi; khôi phục biến đã bị xóa là tạo lại. Phạm vi Machine cần elevated; phạm vi User thì không. Mọi lần khôi phục cần gõ lại tên key.
- R8: Quy tắc biến cốt lõi (mục 4.6) áp dụng như nhau ở hai phạm vi; biến thường không cần gõ lại tên key.
- R9: Thêm biến mới: kiểm tra trùng **trong cùng phạm vi** (không so với phạm vi kia). Key trùng 6 default key ở phạm vi System vẫn bị chặn như hiện nay (tạo chúng qua tab Mặc định).
- R10: Giá trị nhạy cảm bị che mặc định (Mask) ở cả hai lưới; thông báo lỗi chỉ nêu key, không nêu value.

## 6. Trường hợp biên và lỗi
| Tình huống | Hành vi mong đợi |
|---|---|
| Không có quyền ghi HKLM (không elevated) | Báo cần Administrator; lưới System chỉ xem; không ghi, không backup |
| Lưới User trống | Lưới rỗng, vẫn Thêm được |
| Biến bị xóa/đổi ngoài ứng dụng khi đang mở | Báo biến không còn, tải lại lưới |
| Backup lỗi (ổ đầy, không ghi được) | Không ghi/xóa biến; báo lỗi rõ |
| Khôi phục biến có kiểu registry không hỗ trợ (`REG_DWORD`, `REG_MULTI_SZ`...) | Từ chối với thông báo rõ, không ghi |
| Backup cũ (N2-1, không có `Scope`) | Hiểu là Machine, vẫn khôi phục được |
| Hai lần backup cùng giây, cùng key, cùng phạm vi | Thêm hậu tố `_1`, `_2` như N2-1 |
| Ứng dụng chạy bằng tài khoản Administrator **khác** người đăng nhập (UAC dùng credential khác) | `HKCU` là của tài khoản admin đó, không phải người dùng đang đăng nhập; hiển thị tên tài khoản hiện tại trên tiêu đề lưới User để người dùng nhận ra |
| Xóa/sửa một trong 6 key `BaseRepo__*` | Cho phép (theo yêu cầu); Gateway/Site đọc biến Machine nên sẽ mất cấu hình tới khi khôi phục hoặc tạo lại; xác nhận Yes/No như xóa thông thường |
| `Path` User vượt 2047 ký tự hoặc có mục lỗi | Như `FrmEnvListEditor` hiện tại |

## 7. Dữ liệu và bảo mật
- **Dữ liệu nhạy cảm vào file backup:** 6 key mặc định gồm `BaseRepo__JWT__AccessKey`, `BaseRepo__JWT__RefreshKey` là **secret**. Backup mọi sửa/xóa nghĩa là giá trị secret được ghi **plaintext** vào `Backups/Env/` cạnh exe. **Quyết định (2026-11-16): cho phép backup đầy đủ, kể cả secret, và thêm `Backups/` vào `.gitignore`** để file backup không bị commit nhầm. Không che, không mã hóa.
- Không log giá trị biến; thông báo chỉ nêu key. Không thêm package. Không đọc/ghi file secret của repo.
- Ghi HKLM cần Administrator (manifest hiện `requireAdministrator`); HKCU chỉ ghi vào hive của tài khoản đang chạy ứng dụng.
- Backup/khôi phục chỉ chạm registry biến môi trường (`Session Manager\Environment`, `HKCU\Environment`), không chạm khóa khác.
- Kiểm thử tự động tuyệt đối không ghi biến thật; chỉ dùng key `N4_TEST_*` và dọn sạch.

## 8. Phụ thuộc
- **N2-1** (backup/khôi phục/registry service/`FrmEnvTypeKeyConfirm`), **N2-2** (`FrmEnvListEditor`), **N1** (tab Tùy chỉnh). N2-1/N2-2 mới 🟡 (chưa chạy tay); N4 nên làm sau khi chúng được chạy tay và ✅, hoặc chấp nhận rủi ro xây tiếp trên code chưa verify.
- Không cần thứ bên ngoài 🔑.

## 9. Câu hỏi còn mở
- [x] 1. Backup chứa secret JWT dạng plaintext: chấp nhận, thêm `Backups/` vào `.gitignore` (người dùng chốt 2026-11-16).
- [x] 2. Mục "Ngoài phạm vi": ổn (người dùng chốt 2026-11-16).

## 10. Ngoài phạm vi
- Biến Process; biến của người dùng khác (hive khác); đọc `HKU\<SID>`.
- Đổi tab Mặc định, hai nút "Tải lên tập trung"/"Đồng bộ về cục bộ" (chưa có handler).
- Mã hóa file backup, tự xóa backup cũ, nén.
- Đối chiếu/so sánh/gộp giữa User và Machine.
- Đổi 12 biến cốt lõi hay danh sách biến danh sách.
- Test project xUnit (M1).
