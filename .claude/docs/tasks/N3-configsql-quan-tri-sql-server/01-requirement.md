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
2. **Kết nối (nhóm 1), nhiều kết nối đồng thời (R19):** người dùng thêm một hoặc nhiều kết nối vào **danh sách kết nối** (tối đa 10), mỗi kết nối có tên gợi nhớ (alias), vai trò (Nguồn / Đích / Khác), Host, Database, kiểu xác thực (SQL Auth hoặc Windows Auth), Username/Password, TrustServerCertificate; hoặc chọn nhanh một mục trong `Connections` của `system.config` để điền sẵn. Bấm **Test** (hoặc **Test tất cả**) → thử mở kết nối, hiển thị phiên bản SQL Server, tên DB, user hiện tại, có phải sysadmin không; kết nối **Sẵn sàng** khi test OK. Chuỗi nhập tay **chỉ giữ trong RAM**, không ghi file, không ghi log.
3. **Chế độ, theo từng kết nối:** mỗi kết nối mặc định **Chỉ đọc**. Có công tắc **Cho phép ghi** riêng cho từng kết nối (mỗi phiên, tắt khi sửa/xóa/đóng trang). Thao tác ghi/nguy hiểm chỉ dùng được trên kết nối **Sẵn sàng và đã bật ghi**, và vẫn qua xác nhận mạnh (hiện Host/Database, bắt gõ lại tên DB, nêu hậu quả). Kết nối **Nguồn** chỉ để đọc nên không cần bật ghi.
4. **Tab Chạy file SQL (M6):** chọn Module → liệt kê `*.sql` trong `{FolderPath}\SQL\` → xem trước chỉ đọc → chọn **một hoặc nhiều kết nối đích** → Chạy (tách batch theo `GO`, chạy tuần tự, dừng ngay khi lỗi và báo batch lỗi). Mỗi đích một transaction độc lập (R20).
5. **Tab Đối tượng (nhóm 2):** cây/danh sách theo DB: user/login, stored procedure, function, bảng, khóa (PK/FK/UNIQUE), view, trigger. Chọn một đối tượng → xem định nghĩa (`sys.sql_modules`, cột, khóa, index) và **sinh script** CREATE/ALTER/DROP vào ô script để người dùng xem rồi tự bấm chạy. Có nhóm **Hàm hệ thống gọi nhanh** (danh sách định sẵn như `@@VERSION`, `DB_NAME()`, `sp_helpdb`, `sp_who2`, `sys.dm_*` đọc nhanh) chạy bằng một cú bấm.
6. **Tab Backup & Copy (nhóm 3):** Backup database (`BACKUP DATABASE`, đường dẫn trên máy SQL Server, tùy chọn COPY_ONLY/COMPRESSION/CHECKSUM) và **Restore database** (`RESTORE VERIFYONLY` trước, `RESTORE DATABASE` sau, chọn WITH REPLACE/MOVE, đưa DB về SINGLE_USER khi cần rồi trả lại); tạo script schema (bảng, khóa, index, view, SP, function, trigger, file/filegroup); copy sang **DB đích** (một **kết nối nguồn** sang **một hoặc nhiều kết nối đích** chọn từ danh sách kết nối, R19–R20; ví dụ 1 nguồn và 3 đích) theo **chế độ chọn được: Chỉ schema / Chỉ dữ liệu / Cả hai** (R17): tạo bảng ở đích, copy dữ liệu bằng `SqlBulkCopy`, có tiến độ và nút hủy.
7. **Tab Tối ưu (nhóm 4):** từ DMV liệt kê missing index, index trùng/không dùng, query tốn tài nguyên (top theo CPU/reads/duration); sinh script `CREATE/DROP INDEX` đề xuất vào ô script, người dùng tự quyết định chạy.
8. **Tab Chẩn đoán (nhóm 5):** blocking chain, session đang chạy lâu/đang chờ (wait), transaction mở lâu, tài nguyên (CPU/memory); lệnh **clear cache** tức thời (`DBCC FREEPROCCACHE`, `DROPCLEANBUFFERS`, `FREESYSTEMCACHE`) và **kill session** (`KILL <spid>`); tạo **SQL Agent job** dọn cache định kỳ (`msdb`).
9. **Tab Thư viện script (chẩn đoán dựng sẵn, chỉ đọc):** danh mục script theo nhóm CPU, IO (kể cả độ trễ IO theo file), wait, memory, tempdb và dung lượng, index/thống kê, **log hệ thống** (SQL Server error log, SQL Agent log, deadlock từ `system_health`, lịch sử backup/job, trang nghi hỏng, `CHECKDB` gần nhất), phiên và kết nối, perf counter. Chọn script → nhập tham số (TOP N, khoảng thời gian, chuỗi lọc) → chạy → lưới kết quả và **Xuất CSV** (R18).
10. **Tab So sánh & Migration (nguồn → đích):** chọn **một kết nối nguồn** và **một hoặc nhiều kết nối đích** (R19). (a) **So sánh schema** (R21): liệt kê bảng thiếu ở đích, cột thiếu, cột thừa, và cột khác kiểu/độ dài/precision/NULL; tô màu mức rủi ro từng khác biệt. (b) **Sinh kế hoạch migration** theo bảng nguồn (thêm bảng/cột còn thiếu, sửa kiểu dữ liệu) thành script xem trước, kiểm tra tương thích dữ liệu trước khi sửa kiểu, **tự drop và tạo lại các đối tượng phụ thuộc** (R23), rồi **thực thi trực tiếp** trên các đích đã chọn (mỗi đích một transaction, R20). Không so sánh dữ liệu từng dòng (người dùng đã loại khỏi phạm vi, chỉ so sánh cấu trúc).

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

- R17: **Copy có 3 chế độ** do người dùng chọn: (a) **Chỉ schema** — chỉ tạo đối tượng ở DB đích (không copy dòng nào); (b) **Chỉ dữ liệu** — bảng đích **phải đã tồn tại**: kiểm tra trước, bảng thiếu hoặc cột không khớp (tên/kiểu/độ dài, bỏ qua cột identity/computed) thì **dừng và liệt kê**, không tự tạo; chọn *chèn thêm* hoặc *xóa dữ liệu cũ trước* (xóa phải qua xác nhận mạnh, nằm trong transaction của bảng đó; bảng có FK trỏ tới thì báo thay vì `TRUNCATE`); (c) **Cả hai** — tạo schema rồi copy dữ liệu theo R15. Mọi chế độ đều tuân R13 và R15.
- R18: **Thư viện script chẩn đoán chỉ đọc:** script nằm cố định trong code (không sửa, không ô nhập SQL tự do), chạy được cả khi đang Chỉ đọc; tham số truyền bằng `SqlParameter` (TOP N giới hạn 1–500, chuỗi lọc không nối vào câu lệnh); script **nặng** (`dm_db_index_physical_stats`, `dm_os_buffer_descriptors`...) cần xác nhận Yes/No và có timeout dài hơn; thiếu quyền hoặc phiên bản không hỗ trợ chỉ báo lỗi cho script đó (R11); Xuất CSV không ghi dữ liệu ra nhật ký.

- R19: **Nhiều kết nối đồng thời:** tối đa **10** kết nối trong RAM; mỗi kết nối độc lập (có trạng thái *Chưa test / OK / Lỗi*, chế độ ghi riêng); alias duy nhất (không phân biệt hoa thường); mọi kết nối bị xóa khi đóng trang. Tab dùng **một** kết nối (đối tượng, tối ưu, chẩn đoán, thư viện script, restore) làm việc trên **kết nối đang chọn**; tab **nhiều đích** (chạy file `.sql`, copy dữ liệu) cho chọn một nguồn (nếu cần) và một hoặc nhiều đích. **Không cho đích trùng nguồn** (cùng Host + Database, không phân biệt hoa thường).
- R20: **Thực thi đa đích:** mỗi đích chạy **tuần tự**, **một transaction riêng, độc lập**, không dùng distributed transaction/MSDTC; lỗi ở một đích chỉ rollback đích đó, các đích đã commit trước đó **không** được hoàn tác (giao diện nói rõ). Tùy chọn **"Dừng khi một đích lỗi"** (mặc định bật: đích chưa chạy đánh dấu *Bỏ qua*; tắt: chạy tiếp). **Xác nhận mạnh gộp một hộp** liệt kê mọi đích (Alias — Host/Database): 1 đích thì gõ lại tên DB, nhiều đích thì gõ lại **số lượng đích**. Kết quả báo theo từng đích (OK / ROLLBACK / BỎ QUA / HỦY) và ghi nhật ký một dòng mỗi đích (R10).

- R21: **So sánh schema và migration theo bảng nguồn (luôn nguồn → đích):** so khớp bảng theo `schema.tên` và cột theo tên (không phân biệt hoa thường). Khác biệt được phân loại: *bảng thiếu*, *cột thiếu*, *cột thừa ở đích*, *khác kiểu/độ dài/precision/scale/NULL*, *khác default/collation/identity/computed* (nhóm cuối **chỉ báo cáo**, không tự sửa). Migration tự sinh script cho: tạo bảng thiếu, **thêm cột thiếu**, **sửa kiểu dữ liệu/độ dài/precision/NULL theo nguồn**. **Không bao giờ tự xóa cột hoặc bảng thừa ở đích** (chỉ báo cáo). Mỗi thay đổi có mức rủi ro: *An toàn* (ví dụ mở rộng `varchar(50)` → `varchar(100)`, `int` → `bigint`), *Cần kiểm tra dữ liệu* (thu hẹp, đổi kiểu không tương thích, đổi sang NOT NULL), *Cần tự xử lý phụ thuộc* (cột nằm trong default/CHECK/index/PK/unique/FK/thống kê: phần mềm **tự drop đối tượng phụ thuộc, sửa cột, rồi tạo lại đúng như cũ trong cùng transaction**, xem R23), *Không hỗ trợ tự động* (một số trường hợp SQL Server không cho phép hoặc quá rủi ro: báo rõ lý do, loại khỏi script, xem R23). Thêm cột NOT NULL không có default vào bảng đã có dữ liệu thì sinh cột `NULL` kèm cảnh báo. Trước khi sửa kiểu, chạy **truy vấn kiểm tra tương thích** trên đích (ví dụ `MAX(LEN())` vượt độ dài mới, số dòng `TRY_CONVERT` lỗi, số dòng NULL khi chuyển NOT NULL); có vi phạm thì **chặn** thay đổi đó và liệt kê. Thực thi qua R13, R20 (script xem trước được, chỉ chạy khi đích bật Cho phép ghi, xác nhận mạnh, đề nghị backup `COPY_ONLY` trước).

- R23: **Tự xử lý phụ thuộc khi sửa cột (mục đích phần mềm là không phải xử lý thủ công):** với mỗi `ALTER COLUMN`, kế hoạch tự sinh theo thứ tự: (1) drop FK tham chiếu tới hoặc xuất phát từ cột, (2) drop default constraint và CHECK constraint chứa cột, (3) drop index/PK/UNIQUE constraint chứa cột (kể cả cột INCLUDE) và thống kê do người dùng tạo, (4) `ALTER COLUMN`, (5) tạo lại **đúng định nghĩa cũ** theo thứ tự ngược: index/PK/UNIQUE (tên, cột key và include, ASC/DESC, UNIQUE, clustered/nonclustered, filter, fill factor, `IGNORE_DUP_KEY`), CHECK, default, FK (kể cả `ON DELETE/UPDATE`, trạng thái disabled/not trusted). Toàn bộ nằm trong **một transaction của từng đích** (R13, R20): bước nào lỗi thì rollback nguyên vẹn, không để lại đối tượng bị drop dở. Các cột nối với nhau qua FK được xử lý **theo nhóm**: drop mọi FK của nhóm, sửa mọi cột trong nhóm, tạo lại FK; nếu sau khi sửa hai đầu FK vẫn khác kiểu thì chặn kèm lý do. Hành động nặng (drop/tạo lại clustered index hoặc PK trên bảng lớn, tạo lại FK `WITH CHECK`) phải hiện ước lượng số dòng, cảnh báo khóa bảng và tăng log, và đi qua xác nhận mạnh. **Không hỗ trợ tự động** (báo rõ lý do cho từng trường hợp, loại khỏi script, không bỏ qua im lặng): view/function `SCHEMABINDING` tham chiếu cột, cột computed hoặc cột mà computed column phụ thuộc, full-text index, bảng memory-optimized, bảng temporal (system-versioned), bảng bật CDC/replication, cột identity nếu SQL Server không cho đổi, cột Always Encrypted/dynamic data masking/ledger, kiểu CLR hoặc xml schema collection. Danh sách này sẽ được chốt chính xác khi viết spec chi tiết N3-9 và kiểm chứng trên SQL Server thật.

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
| Copy "Chỉ dữ liệu": bảng đích không tồn tại hoặc cột lệch | Dừng trước khi chép, liệt kê bảng/cột lệch, không tự tạo hay sửa |
| Copy "Chỉ dữ liệu" + xóa dữ liệu cũ, bảng đích có FK trỏ tới | Báo không `TRUNCATE` được, đề nghị bỏ qua bảng đó hoặc xử lý thủ công; không tự tắt FK |
| Copy "Chỉ schema" khi bảng đích đã có | Hỏi *Bỏ qua / Thay thế (DROP + tạo lại)*, mặc định *Bỏ qua* |
| Script thư viện thiếu quyền (ví dụ error log cần quyền cao hơn) | Báo riêng cho script đó, các script khác vẫn dùng được |
| Script nặng bị hủy giữa chừng | Dừng truy vấn, không để lại phiên treo, báo đã hủy |
| Thêm kết nối thứ 11 hoặc alias trùng | Chặn kèm thông báo, không thêm |
| Test tất cả, một số kết nối lỗi | Mỗi kết nối có trạng thái riêng; lỗi một cái không làm dừng cái khác |
| Chọn đích trùng nguồn (cùng Host + Database) | Chặn, báo rõ lý do |
| Một trong nhiều đích mất kết nối hoặc lỗi giữa lượt | Đích đó rollback; đích đã xong giữ nguyên; các đích còn lại theo tùy chọn Dừng/Tiếp tục; báo rõ từng đích |
| Người dùng hủy khi đang chạy nhiều đích | Đích hiện tại rollback, đích chưa chạy *Bỏ qua*, đích đã commit giữ nguyên |
| Sửa hoặc xóa một kết nối khi một tab đang dùng nó | Tab cập nhật hoặc vô hiệu hóa lựa chọn đó, không chạy bằng cấu hình cũ |
| Migration: sửa kiểu sang kiểu hẹp hơn mà đích có dữ liệu vượt giới hạn | Truy vấn kiểm tra tìm ra vi phạm, **chặn** thay đổi đó, liệt kê số dòng vi phạm, không chạy |
| Migration: cột cần sửa nằm trong PK/index/FK/CHECK/default/thống kê | Tự drop phụ thuộc, sửa cột, tạo lại đúng như cũ trong cùng transaction (R23); lỗi bất kỳ bước nào thì rollback cả đích |
| Migration: cột thuộc trường hợp không hỗ trợ tự động (view `SCHEMABINDING`, computed, full-text, temporal...) | Báo rõ lý do cho từng cột, loại khỏi script, các thay đổi còn lại vẫn chạy được |
| Migration: sau khi sửa, hai đầu FK vẫn khác kiểu hoặc tạo lại FK `WITH CHECK` gặp dữ liệu vi phạm | Rollback toàn bộ đích đó, báo FK và dữ liệu vi phạm |
| Migration: drop/tạo lại PK hoặc clustered index trên bảng rất lớn | Hiện ước lượng số dòng và cảnh báo khóa/log, cần xác nhận mạnh; đủ dung lượng log hay không được kiểm tra trước nếu có thể |
| Migration: thêm cột NOT NULL không default vào bảng đã có dữ liệu | Sinh cột `NULL` và cảnh báo; không tự điền giá trị |
| Đích có cột/bảng mà nguồn không có | Chỉ báo cáo "thừa ở đích", không xóa |
| Bảng cùng tên nhưng khác schema (`dbo` và `sales`) | Coi là hai bảng khác nhau, báo "thiếu" ở một phía |
| Migration đa đích, một đích lỗi giữa chừng | Theo R20: đích đó rollback, đích đã commit giữ nguyên, đích còn lại theo tùy chọn Dừng/Tiếp tục |

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
| N3-1 | Khung ConfigSQL + **quản lý nhiều kết nối đồng thời** (nguồn/đích, tối đa 10, test kết nối) + chế độ Chỉ đọc/Cho phép ghi theo từng kết nối (nhóm 1, R19) | — |
| N3-2 | **Chạy file `.sql` theo module (M6)** trên một hoặc nhiều đích, mỗi đích một transaction (R20) + hộp xác nhận mạnh + nhật ký hành động | N3-1 |
| N3-3 | Tab Đối tượng + hàm hệ thống gọi nhanh (nhóm 2, xem + sinh script) | N3-1 |
| N3-4 | Backup, sinh script schema (kể cả file/filegroup), copy sang DB đích theo lô + transaction, chọn chế độ **chỉ schema / chỉ dữ liệu / cả hai** (nhóm 3, R15, R17) | N3-1, N3-3 |
| N3-5 | Gợi ý index và tối ưu query từ DMV, sinh script (nhóm 4) | N3-1, N3-3 |
| N3-6 | Chẩn đoán treo, clear cache, kill session, tạo SQL Agent job (nhóm 5) | N3-1 |
| N3-7 | **Restore database** (R16), tách riêng vì rủi ro cao nhất | N3-4 (dùng chung Backup) |
| N3-8 | **Thư viện script chẩn đoán** chỉ đọc (R18): CPU, IO, wait, memory, tempdb/dung lượng, index, log hệ thống, phiên/kết nối, perf counter | N3-1 |
| N3-9 | **So sánh schema nguồn → đích và migration** (R21): thêm bảng/cột thiếu, sửa kiểu dữ liệu theo nguồn, kiểm tra tương thích, **tự drop và tạo lại phụ thuộc (R23)**, thực thi trên nhiều đích | N3-1, N3-2, N3-3 (N3-4 để đề nghị backup) |

> Đã tách từ 6 thành 7 task khi viết spec (2026-11-15): "khung + kết nối" và "chạy file `.sql`" của N3-1 cũ lớn khoảng 12 file nên tách riêng N3-1/N3-2. Thư mục spec: `tasks/N3-1-khung-ket-noi-sql`, `N3-2-chay-file-sql-theo-module`, `N3-3-doi-tuong-va-ham-nhanh`, `N3-4-backup-script-copy`, `N3-5-goi-y-index-toi-uu`, `N3-6-chan-doan-kill-cache`, `N3-7-restore-database`, `N3-8-thu-vien-script-chan-doan` (thêm sau khi người dùng đồng ý bổ sung R17, R18), `N3-9-so-sanh-schema-va-migration` (thêm 2026-11-15 theo yêu cầu so sánh và migration giữa 2 kết nối, R21, R23; yêu cầu so sánh dữ liệu từng dòng đã được người dùng loại bỏ). N3-1 và N3-2 có spec chi tiết; N3-3 đến N3-9 là bản phác thảo, BA hoàn thiện trước khi tới lượt.

## 10. Ngoài phạm vi
- Sửa/xóa/tạo đối tượng trực tiếp bằng form (chỉ sinh script rồi người dùng tự chạy).
- SMO, mã hóa/lưu chuỗi kết nối, distributed transaction/MSDTC (đảm bảo all-or-nothing giữa nhiều DB), chạy song song nhiều đích, SSMS-like editor có IntelliSense.
- Restore tới thời điểm (point-in-time, log backup chain), differential/log backup (chỉ full backup và restore full).
- Hỗ trợ DB không phải SQL Server (MySQL, PostgreSQL...), tracking lịch sử migration, tự động hóa theo lịch của chính app.

## 11. Câu hỏi còn mở
Đã chốt trong vòng hỏi 3 (2026-11-15):
- [x] "Copy file, filegroup" = sinh script/tái tạo cấu trúc file và filegroup sang DB đích, không copy file `.mdf/.ndf` vật lý.
- [x] Nhật ký theo từng view: `Logs/ConfigSQL/` cạnh exe, file theo ngày (R10).
- [x] Làm luôn **Restore** (task riêng N3-7, R16).
- [x] Copy dữ liệu theo lô 5.000 dòng bằng `SqlBulkCopy`, bảng quá lớn thì chia nhỏ (R15).
- [x] Mọi thao tác ghi dùng transaction và rollback khi lỗi/hủy, trừ các lệnh SQL Server không cho chạy trong transaction (R13, R14).

- [x] Ngưỡng "bảng lớn" để chuyển từ một transaction/bảng sang commit từng lô: **1.000.000 dòng**, chỉnh được trong form.
- [x] Script có lệnh không chạy được trong transaction (R14): **hỏi từng lần**, không tự chạy ngoài transaction.

**Yêu cầu đã được người dùng chốt ngày 2026-11-15** ("chốt N3"); không còn câu hỏi mở.
Bổ sung sau chốt (2026-11-15, người dùng trả lời "đồng ý"): R17 (copy chế độ chỉ schema / chỉ dữ liệu / cả hai) và R18 + N3-8 (thư viện script chẩn đoán kể cả log hệ thống, IO theo file).
Bổ sung sau chốt (2026-11-15, theo yêu cầu "mở cùng lúc nhiều db, test kết nối sau đó thực thi trực tiếp, ví dụ 1 nguồn và 3 đích"): R19 (nhiều kết nối đồng thời) và R20 (thực thi đa đích). N3-1, N3-2 và N3-4 đã được sửa theo.

Giả định BA cho R19–R20 (người dùng đã xác nhận 2026-11-15):
- [x] Tối đa **10** kết nối đồng thời.
- [x] Nhiều đích chạy **tuần tự**, không song song.
- [x] Mặc định **dừng khi một đích lỗi** (có thể tắt để chạy tiếp).
- [x] Xác nhận mạnh nhiều đích: **gõ lại số lượng đích**, 1 đích thì gõ tên DB.
- [x] **Không** dùng distributed transaction: đích đã commit không rollback khi đích sau lỗi.

Bổ sung sau chốt (2026-11-15, theo yêu cầu so sánh và migration giữa 2 kết nối): R21 (so sánh schema và migration), R23 (tự xử lý phụ thuộc), N3-9. **Người dùng đã xác nhận 2026-11-15:** chỉ so sánh **cấu trúc**, **không** so sánh dữ liệu từng dòng (đã loại R22 và N3-10); hướng nguồn → đích, không tự xóa cột/bảng thừa; phần mềm **tự xử lý phụ thuộc** thay vì thủ công; thêm cột NOT NULL không default thì sinh cột NULL kèm cảnh báo; các giả định R19–R20 (10 kết nối, tuần tự, dừng khi một đích lỗi, gõ lại số lượng đích, không distributed transaction) được chấp nhận.
- [x] Hướng migration **luôn nguồn → đích**; **không tự xóa** cột/bảng thừa ở đích.
- [x] Cột bị phụ thuộc (PK, index, FK, CHECK, default, thống kê) khi sửa kiểu: **phần mềm tự drop, sửa và tạo lại** trong cùng transaction (R23), vì mục đích phần mềm là không phải xử lý thủ công; chỉ các trường hợp SQL Server không cho phép được báo rõ lý do.
- [x] Thêm cột NOT NULL không default vào bảng có dữ liệu: sinh cột `NULL` kèm cảnh báo.
