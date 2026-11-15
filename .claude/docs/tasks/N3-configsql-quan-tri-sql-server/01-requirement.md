# N3 — ConfigSQL: công cụ quản trị SQL Server trong SystemManagement · Phân tích yêu cầu

- **Người viết:** BA (skill `ba-requirements`) · **Ngày:** 2026-11-15 · **Module:** 6 (SystemManagement) · **Bao gồm:** M6 (chạy file `.sql` theo module)

## 1. Bối cảnh và mục tiêu
Người vận hành (dev/DevOps cài đặt hệ thống BaseRepo trên máy Windows) hiện phải mở SSMS hoặc gõ tay lệnh SQL để kết nối DB, xem đối tượng, backup/copy, tối ưu và xử lý treo. Mục tiêu: màn hình **ConfigSQL** trong SystemManagement cho phép làm các việc đó từ một nơi, an toàn (mặc định chỉ đọc, thao tác nguy hiểm cần xác nhận mạnh), không thêm package, không lưu chuỗi kết nối.
Thành công khi: nhập chuỗi kết nối và kết nối được; xem đối tượng DB và sinh script; backup/copy sang DB khác; xem gợi ý index; chẩn đoán treo, kill session, dọn cache; mọi thao tác ghi/nguy hiểm đều có chốt chặn rõ ràng.

## 2. Yêu cầu gốc
> module SystemManagement, view ConfigSQL mình muốn có các tính năng sau:
> 1. Nhập các chuỗi kết nối DB và thực hiện kết nối (không cần lưu)
> 2. Quản lý user, store procedured, các bảng trogn sql, key, views, triggers, các hàm hệ thống gọi nhanh
> 3. Backup, tạo script, copy schema, file, filegroup, tạo bảng, copy dữ liệu sang 1 DB khác
> 4. Chuẩn hóa lệnh sql thiếu index, tối ưu hiệu năng query
> 5. có các lệnh chuẩn đoán treo, job clear cache, kill session treo

## 3. Hiện trạng
- `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.cs` (+ `.Designer.cs`): **stub rỗng**, chưa `IBasePage`, chưa có UI. `UC_Home.Designer.cs` có label "Cấu hình SQL" (`label6`) chưa gắn Click; điều hướng bằng `BaseUC.InternalNavigate(new UC_X())` như `UC_ConfigEnv` (xem `UC_Home.cs`).
- `Models/SystemConfig.cs` + `Config/system.config`: `ConnectionItem` (Name, Host, Database, Username, Password plaintext, TrustCertificate) và `ModuleItem.FolderPath`; mới có 1 connection `CenterConnection` (trống) và module `Gateway` (trống). Chưa có `DefaultConnection`, chưa có module Site/AdministrativeUnit/Booking.
- Package: `SystemManagement.csproj` chưa khai báo SQL trực tiếp; `Microsoft.Data.SqlClient` 7.0.2 và Dapper đi theo `CommonLibs`. Không có SMO. Publish `PublishSingleFile` + `SelfContained` + Obfuscar; `app.manifest` `requireAdministrator`.
- File SQL cài đặt nằm trong `{Service}.Api/SQL/` (Gateway: `BaseSQL.sql`, `seed_admin_account.sql`; AdministrativeUnit; Booking). Spec cũ: `docs/superpowers/specs/2026-09-02-sql-file-relocation-and-system-management-sql-tool-design.md` (chạy file `.sql` theo module, tách batch `GO`, dừng khi lỗi).
- Helper UI có sẵn: `FrmMain.RunWithLoadingAsync`, `ThrowInfo/Warning/Error/Question`, mẫu DataGridView của `UC_ConfigEnvCustom`, mẫu hộp xác nhận gõ lại tên key (spec N2-1, chưa code).
- Không có test project cho SystemManagement (M1 chưa làm).

## 4. Luồng nghiệp vụ
1. Người dùng mở **Cấu hình SQL** từ trang chủ. Màn hình có thanh **Kết nối** ở trên và các tab chức năng bên dưới; chưa kết nối thì các tab bị vô hiệu.
2. **Kết nối (nhóm 1):** nhập tay Host, Database, kiểu xác thực (SQL Auth hoặc Windows Auth), Username/Password, TrustServerCertificate; hoặc chọn nhanh một mục trong `Connections` của `system.config` để điền sẵn. Bấm **Kết nối** → thử mở kết nối, hiển thị phiên bản SQL Server, tên DB, user hiện tại, có phải sysadmin không. Chuỗi nhập tay **chỉ giữ trong RAM**, không ghi file, không ghi log.
3. **Chế độ:** mặc định **Chỉ đọc**. Có công tắc **Cho phép ghi** (mỗi phiên, tắt khi ngắt kết nối). Thao tác ghi/nguy hiểm khi bật mới dùng được và vẫn qua xác nhận mạnh (hiện Host/Database, bắt gõ lại tên DB, nêu hậu quả).
4. **Tab Chạy file SQL (M6):** chọn Module → liệt kê `*.sql` trong `{FolderPath}\SQL\` → xem trước chỉ đọc → Chạy (tách batch theo `GO`, chạy tuần tự, dừng ngay khi lỗi và báo batch lỗi).
5. **Tab Đối tượng (nhóm 2):** cây/danh sách theo DB: user/login, stored procedure, function, bảng, khóa (PK/FK/UNIQUE), view, trigger. Chọn một đối tượng → xem định nghĩa (`sys.sql_modules`, cột, khóa, index) và **sinh script** CREATE/ALTER/DROP vào ô script để người dùng xem rồi tự bấm chạy. Có nhóm **Hàm hệ thống gọi nhanh** (danh sách định sẵn như `@@VERSION`, `DB_NAME()`, `sp_helpdb`, `sp_who2`, `sys.dm_*` đọc nhanh) chạy bằng một cú bấm.
6. **Tab Backup & Copy (nhóm 3):** Backup database (`BACKUP DATABASE`, đường dẫn trên máy SQL Server, tùy chọn COPY_ONLY/COMPRESSION/CHECKSUM) và **Restore database** (`RESTORE VERIFYONLY` trước, `RESTORE DATABASE` sau, chọn WITH REPLACE/MOVE, đưa DB về SINGLE_USER khi cần rồi trả lại); tạo script schema (bảng, khóa, index, view, SP, function, trigger, file/filegroup); copy schema và/hoặc dữ liệu sang **DB đích** (kết nối thứ hai nhập như bước 2): tạo bảng ở đích, copy dữ liệu bằng `SqlBulkCopy`, có tiến độ và nút hủy.
7. **Tab Tối ưu (nhóm 4):** từ DMV liệt kê missing index, index trùng/không dùng, query tốn tài nguyên (top theo CPU/reads/duration); sinh script `CREATE/DROP INDEX` đề xuất vào ô script, người dùng tự quyết định chạy.
8. **Tab Chẩn đoán (nhóm 5):** blocking chain, session đang chạy lâu/đang chờ (wait), transaction mở lâu, tài nguyên (CPU/memory); lệnh **clear cache** tức thời (`DBCC FREEPROCCACHE`, `DROPCLEANBUFFERS`, `FREESYSTEMCACHE`) và **kill session** (`KILL <spid>`); tạo **SQL Agent job** dọn cache định kỳ (`msdb`).

## 5. Quy tắc nghiệp vụ
- R1: Chuỗi kết nối/password **không lưu** (không `system.config`, không registry, không file), không log, không hiện lại sau khi kết nối (ô Password dạng ẩn); thông báo lỗi và log hành động không chứa password.
- R2: Danh sách `Connections` chỉ dùng để **điền sẵn** form; mọi sửa đổi trong form không ghi ngược lại `system.config`.
- R3: Hỗ trợ SQL Auth và Windows Auth; có tùy chọn `TrustServerCertificate` và `Encrypt` theo `SqlConnectionStringBuilder`.
- R4: Mặc định **chỉ đọc**: chỉ chạy được truy vấn metadata/DMV và hàm hệ thống đọc từ danh sách định sẵn; mọi thao tác ghi (chạy script, backup tới đường dẫn, copy sang DB đích, tạo index, clear cache, kill, tạo job) bị khóa cho đến khi bật **Cho phép ghi**.
- R5: Thao tác nguy hiểm (kill session, clear cache, tạo/xóa job, **restore database**, chạy script có `DROP`/`TRUNCATE`/`DELETE`, copy dữ liệu đè lên bảng có sẵn) cần **xác nhận mạnh**: hiện Host/Database đích, nêu hậu quả, bắt gõ lại tên DB (hoặc `SPID` khi kill).
- R6: **Không dùng SMO, không thêm package**; chỉ T-SQL + `Microsoft.Data.SqlClient` (đã có qua CommonLibs). Sinh script từ `sys.*`/`INFORMATION_SCHEMA`.
- R7: Mọi tên đối tượng đưa vào câu lệnh động phải qua `QUOTENAME`/tham số hóa; không nối chuỗi từ đầu vào người dùng; script người dùng tự gõ/sinh chỉ chạy khi đã bật ghi.
- R8: Kill session không cho kill chính session của app và session hệ thống (`session_id` < 51 hoặc `is_user_process = 0`).
- R9: Việc nặng chạy nền với `RunWithLoadingAsync`, có **timeout** cấu hình được cho truy vấn (mặc định 30 giây cho đọc, không giới hạn có xác nhận cho backup/copy) và nút **Hủy** cho backup/copy dữ liệu.
- R10: Ghi **nhật ký hành động** cho thao tác ghi/nguy hiểm: thời gian, Host, Database, loại thao tác, đối tượng, kết quả; **không** ghi password, nội dung dữ liệu, hay chuỗi kết nối đầy đủ. Nhật ký **theo từng view**: màn hình này ghi vào `Logs/ConfigSQL/` (cạnh exe), một file văn bản mỗi ngày `yyyyMMdd.log`; các view khác sau này dùng `Logs/<TênView>/` cùng quy ước.
- R13: **Transaction cho mọi thao tác ghi bằng T-SQL có thể chạy trong transaction:** mỗi thao tác mở `SqlTransaction`, thành công thì commit, lỗi hoặc người dùng hủy thì **rollback ngay** và báo rõ. Áp dụng cho: chạy script/file `.sql` (cả file trong một transaction, `GO` tách batch nhưng cùng transaction), tạo bảng/index ở DB đích, copy dữ liệu theo bảng, tạo SQL Agent job (`msdb`).
- R14: **Ngoại lệ bắt buộc, SQL Server không cho chạy trong transaction:** `BACKUP`, `RESTORE`, `CREATE/ALTER/DROP DATABASE`, `ALTER DATABASE`, `DBCC FREEPROCCACHE/DROPCLEANBUFFERS/FREESYSTEMCACHE`, `KILL`, `RECONFIGURE`. Các lệnh này chạy ngoài transaction và giao diện nêu rõ "thao tác này không rollback được"; trước đó phải qua xác nhận mạnh (R5). Với script người dùng có chứa các lệnh trên, hệ thống phát hiện (quét từ khóa theo từng batch) và hỏi chạy ngoài transaction hay hủy.
- R15: **Copy dữ liệu theo lô:** dùng `SqlBulkCopy` với `BatchSize` 5.000 dòng. Bảng nhỏ/vừa (dưới ngưỡng bảng lớn) copy trong **một transaction/bảng**: lỗi hoặc hủy thì rollback cả bảng. Bảng lớn (từ ngưỡng trở lên, xem câu hỏi mở) chia lô, **commit từng lô** để không làm đầy transaction log; lỗi hoặc hủy thì không rollback được phần đã commit, nên: nếu bảng đích do lần chạy này tạo thì **DROP bảng đó** để về trạng thái trước; nếu bảng đã có sẵn thì dừng và báo số dòng đã chèn để người dùng xử lý. Hiển thị trước cho người dùng chế độ nào sẽ áp dụng cho từng bảng.
- R16: **Restore** luôn chạy `RESTORE VERIFYONLY` và `RESTORE HEADERONLY`/`FILELISTONLY` trước; khi đè DB đang có (`WITH REPLACE`) bắt xác nhận mạnh gõ lại tên DB; trước khi đè mặc định đề nghị **backup DB hiện tại** (COPY_ONLY) và nêu rõ nếu người dùng bỏ qua; không restore vào DB hệ thống (`master`, `model`, `msdb`, `tempdb`); nếu restore thất bại giữa chừng thì báo DB đang ở trạng thái RESTORING/không dùng được và gợi ý bước tiếp theo.
- R11: Nhắm SQL Server 2019 trở lên, tài khoản có quyền sysadmin; thiếu quyền hoặc phiên bản không hỗ trợ một DMV/lệnh thì báo lỗi rõ ràng cho tính năng đó, không làm hỏng các tab khác.
- R12: Phương án mặc định cho "tạo SQL Agent job clear cache": sinh và tạo job qua `msdb.dbo.sp_add_job*` với tên tiền tố `BaseRepo_`, chỉ khi SQL Server Agent đang chạy; không động vào job khác.

## 6. Trường hợp biên và lỗi
| Tình huống | Hành vi mong đợi |
|---|---|
| Sai Host/user/password, SQL Server không phản hồi | Báo lỗi kết nối kèm lý do ngắn, không lộ password, giữ trạng thái chưa kết nối |
| Kết nối bị rớt giữa chừng | Báo mất kết nối, vô hiệu tab, cho kết nối lại bằng chuỗi vừa nhập (còn trong RAM) |
| Thiếu quyền (không sysadmin, không `VIEW SERVER STATE`) | Báo thiếu quyền cho tính năng đó, các tính năng đọc khác vẫn dùng được |
| Bật Chỉ đọc rồi bấm thao tác ghi | Nút bị khóa kèm tooltip giải thích |
| Gõ sai tên DB khi xác nhận | Không thực hiện |
| Script có batch lỗi giữa chừng | Dừng ngay, **rollback toàn bộ transaction** (R13), báo số batch/dòng lỗi và nói rõ "đã rollback, không có thay đổi nào được áp dụng" |
| Script chứa lệnh không chạy được trong transaction (R14) | Báo danh sách lệnh, hỏi chạy ngoài transaction (không rollback được) hoặc hủy |
| Restore: file backup không hợp lệ hoặc `VERIFYONLY` lỗi | Dừng trước khi đụng DB đích, báo lỗi |
| Restore: DB đích đang có kết nối khác | Đề nghị đưa SINGLE_USER (`ROLLBACK IMMEDIATE`) có xác nhận mạnh, xong trả lại MULTI_USER |
| Restore vào DB hệ thống | Chặn |
| File `.sql` không đọc được/rỗng/không có thư mục `SQL\` | Báo rõ, không chạy |
| Backup: đường dẫn không tồn tại/không có quyền trên máy SQL Server | Báo lỗi SQL Server trả về, không tạo file tạm ở máy client |
| Copy dữ liệu: DB đích có bảng trùng tên | Hỏi chọn bỏ qua / thay thế (DROP + tạo lại) / chỉ chèn thêm; mặc định bỏ qua |
| Copy dữ liệu: bảng rất lớn, người dùng hủy | Dừng, báo số dòng đã copy, bảng đích ở trạng thái dở dang được nêu rõ |
| Kill session đã kết thúc sẵn | Báo session không còn, tải lại danh sách |
| Clear cache trên máy production | Cảnh báo ảnh hưởng hiệu năng tạm thời, vẫn qua xác nhận mạnh |
| SQL Agent không chạy/không có quyền `msdb` | Báo không tạo được job, không ảnh hưởng tính năng khác |
| DMV trả rỗng (server vừa khởi động) | Hiện "chưa có dữ liệu gợi ý", không coi là lỗi |

## 7. Dữ liệu và bảo mật
- Dữ liệu nhạy cảm: password kết nối, dữ liệu bảng khi copy/preview. Password chỉ ở RAM, ô nhập ẩn ký tự, không log, xóa khỏi bộ nhớ khi ngắt kết nối nếu có thể; backup/script có thể chứa dữ liệu thật nên nêu cảnh báo khi lưu file; không dùng DB production thật khi kiểm thử (theo quy tắc dự án).
- Quyền: quyền của tài khoản SQL nhập vào; app chạy `requireAdministrator` nhưng không dùng quyền đó để vượt SQL.
- Script người dùng chạy có thể phá dữ liệu: chốt chặn bằng R4, R5, R10.
- Nhật ký hành động lưu cục bộ cạnh exe (thư mục `Logs/` hoặc tương đương, vị trí chốt ở spec), không chứa secret.

## 8. Phụ thuộc
- Không phụ thuộc task N1/N2 về mặt code; **dùng lại ý tưởng** hộp xác nhận gõ lại tên (N2-1). Nếu N2-1 làm trước thì tái sử dụng `FrmEnvTypeKeyConfirm`/mẫu tương tự, nếu không thì N3 tự tạo hộp xác nhận riêng.
- Cần SQL Server thật (2019+) để verify runtime 🔑 (cùng nhóm M13/M14); máy dev hiện tại chưa có.
- Thay thế/bao gồm **M6** (chạy file `.sql` theo module, spec 2026-09-02).

## 9. Chia giai đoạn (đề xuất task, chi tiết ở bước tách task)
| Giai đoạn | Nội dung | Phụ thuộc |
|---|---|---|
| N3-1 | Khung ConfigSQL + kết nối (nhóm 1) + chế độ Chỉ đọc/Cho phép ghi + hộp xác nhận mạnh + nhật ký hành động + **chạy file `.sql` theo module (M6)** | — |
| N3-2 | Tab Đối tượng + hàm hệ thống gọi nhanh (nhóm 2, xem + sinh script) | N3-1 |
| N3-3 | Backup, sinh script schema (kể cả file/filegroup), copy schema và dữ liệu sang DB đích theo lô + transaction (nhóm 3) | N3-1 (kết nối thứ hai) |
| N3-4 | Gợi ý index và tối ưu query từ DMV, sinh script (nhóm 4) | N3-1 |
| N3-5 | Chẩn đoán treo, clear cache, kill session, tạo SQL Agent job (nhóm 5) | N3-1 |
| N3-6 | **Restore database** (R16), tách riêng vì rủi ro cao nhất | N3-3 (dùng chung Backup) |

## 10. Ngoài phạm vi
- Sửa/xóa/tạo đối tượng trực tiếp bằng form (chỉ sinh script rồi người dùng tự chạy).
- SMO, mã hóa/lưu chuỗi kết nối, quản lý nhiều kết nối cùng lúc (trừ DB đích khi copy), SSMS-like editor có IntelliSense.
- Restore tới thời điểm (point-in-time, log backup chain), differential/log backup (chỉ full backup và restore full).
- Hỗ trợ DB không phải SQL Server (MySQL, PostgreSQL...), tracking lịch sử migration, tự động hóa theo lịch của chính app.

## 11. Câu hỏi còn mở
Đã chốt trong vòng hỏi 3 (2026-11-15):
- [x] "Copy file, filegroup" = sinh script/tái tạo cấu trúc file và filegroup sang DB đích, không copy file `.mdf/.ndf` vật lý.
- [x] Nhật ký theo từng view: `Logs/ConfigSQL/` cạnh exe, file theo ngày (R10).
- [x] Làm luôn **Restore** (task riêng N3-6, R16).
- [x] Copy dữ liệu theo lô 5.000 dòng bằng `SqlBulkCopy`, bảng quá lớn thì chia nhỏ (R15).
- [x] Mọi thao tác ghi dùng transaction và rollback khi lỗi/hủy, trừ các lệnh SQL Server không cho chạy trong transaction (R13, R14).

Còn mở:
- [ ] Ngưỡng "bảng lớn" để chuyển từ một transaction/bảng sang commit từng lô (đề xuất: **1.000.000 dòng**, chỉnh được trong form).
- [ ] Với script người dùng gõ có lệnh không chạy được trong transaction (R14): mặc định **hỏi từng lần** thay vì tự chạy ngoài transaction (đề xuất: hỏi).
