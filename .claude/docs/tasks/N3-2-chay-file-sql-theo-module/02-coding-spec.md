# N3-2 — ConfigSQL: chạy file `.sql` theo module trên một hoặc nhiều đích, trong transaction (M6) · Coding Spec

- **Trạng thái duyệt:** ✅ đã duyệt bởi Minh Thuận ngày 2026-11-15 (đã viết lại 2026-11-15 sau khi người dùng bổ sung nhiều kết nối đồng thời)
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` mục 4 (bước 4), R4, R5, R10, R13, R14, R19, R20 · Thiết kế gốc: `docs/superpowers/specs/2026-09-02-sql-file-relocation-and-system-management-sql-tool-design.md` mục 5–6 · **Phụ thuộc: N3-1** · **Thực hiện luôn M6 trong roadmap**

## 1. Phạm vi
Thêm tab **Chạy file SQL** vào ConfigSQL: chọn Module (gộp mọi Site) → liệt kê `*.sql` trong `{FolderPath}\SQL\` (không đệ quy) → xem trước chỉ đọc → chọn **một hoặc nhiều kết nối đích** → **Chạy** bằng bộ thực thi tự tách batch theo dòng `GO`.
- **Danh sách đích:** các kết nối do N3-1 quản lý đang **Sẵn sàng** và **bật Cho phép ghi** (kết nối khác hiện mờ kèm lý do). Mỗi đích tick chọn riêng; tối thiểu 1 đích.
- **Mỗi đích một transaction độc lập:** cả file chạy trong **một** `SqlTransaction` của đích đó; batch lỗi hoặc hủy thì **rollback đích đó**. Không dùng distributed transaction/MSDTC, nên các đích đã commit trước đó **không** bị rollback khi đích sau lỗi (giao diện nói rõ điều này).
- **Chạy tuần tự** từng đích theo thứ tự trong danh sách. Tùy chọn **"Dừng khi một đích lỗi"** (mặc định bật): bật thì các đích chưa chạy được đánh dấu *Bỏ qua*; tắt thì chạy tiếp đích còn lại.
- Phát hiện lệnh **không chạy được trong transaction** (R14) → hỏi **một lần cho cả lượt**: chạy ngoài transaction (không rollback được) hoặc hủy. Phát hiện lệnh **phá hủy** (`DROP`/`TRUNCATE`/`DELETE`) → thêm cảnh báo vào hộp xác nhận.
- **Xác nhận mạnh gộp một hộp:** liệt kê mọi đích (Alias — Host/Database) và hậu quả; **1 đích** thì gõ lại tên DB, **nhiều đích** thì gõ lại **số lượng đích**.
- Chỉ chạy **một file mỗi lần** (trên nhiều đích). Ghi nhật ký `Logs/ConfigSQL/` (một dòng cho mỗi đích).
- Hạ tầng cấu hình: thêm `ModuleItem.DefaultConnection`, thêm 3 module (`Site`, `AdministrativeUnit`, `Booking`) cạnh `Gateway` trong `system.config`; chọn Module sẽ **tick sẵn** kết nối đích có `TemplateName == DefaultConnection` (nếu có); nếu chưa có kết nối nào từ mẫu đó thì gợi ý "hãy thêm kết nối từ mẫu X" (không tự tạo, không tự kết nối).
- Đưa vào task này (vì dùng lần đầu): `ViewActionLogger` và `FrmSqlDangerConfirm`.

## 2. Ngoài phạm vi
- Chạy song song nhiều đích, distributed transaction, mã hóa `ConnectionItem.Password`, theo dõi lịch sử file đã chạy (migration history), chọn nhiều file một lượt, sửa/lưu nội dung file, soạn SQL tự do (không có ô nhập lệnh).
- Tab khác của ConfigSQL (N3-3 trở đi). Điền `folderPath` thật cho các module (người dùng tự điền theo máy triển khai).

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph. `ModuleItem` được `FileHelper.ReadXML<SystemConfig>` đọc trong `FrmMain.ReadSystemConfig`; thêm thuộc tính XML mới có giá trị mặc định rỗng nên `system.config` cũ không có thẻ `defaultConnection` vẫn đọc được. `UC_ConfigEnv` dùng `SysConfig.EnvKeys`, không dùng `Sites`, nên không bị ảnh hưởng.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `ModuleItem` (thêm `DefaultConnection`) | Đọc/ghi XML `system.config` | Có `system.config` (cùng task); không có nơi ghi lại file |
| `UC_ConfigSQL.AddFeatureTab`, `Manager` (do N3-1 cung cấp) | Chỉ thêm tab mới | Có (gọi trong `UC_ConfigSQL`) |
| `system.config` (thêm module) | Nơi nào lặp `Sites`/`Modules` | Chưa có nơi khác dùng (Grep chưa thấy), coder xác nhận lại bằng Grep |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Utils/SqlScriptRunner.cs` | Tạo | Tách batch theo `GO`, phát hiện lệnh không-transaction/phá hủy, chạy một đích trong transaction, trả kết quả |
| `SystemManagement/Utils/ViewActionLogger.cs` | Tạo | Ghi nhật ký hành động theo view, an toàn luồng, không nhận secret |
| `SystemManagement/Views/ConfigSQL/FrmSqlDangerConfirm.cs` | Tạo | Hộp xác nhận mạnh: liệt kê đích, gõ lại chuỗi yêu cầu |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQLRunFile.cs` | Tạo | Tab: Module, danh sách file, preview, chọn đích, Chạy, bảng kết quả |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQLRunFile.Designer.cs` | Tạo | Bố cục tab |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.cs` | Sửa | Gọi `AddFeatureTab("Chạy file SQL", ...)` |
| `SystemManagement/Models/SystemConfig.cs` | Sửa | Thêm `ModuleItem.DefaultConnection` (`[XmlElement("defaultConnection")]`) |
| `SystemManagement/Config/system.config` | Sửa | Thêm `<defaultConnection>` cho `Gateway` và 3 module mới |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại. Nếu Designer sinh `.resx` mới cho tab thì được thêm file đó.

## 4. Thay đổi cơ sở dữ liệu
Không có script mới. Tính năng **chạy** các script sẵn có (`BaseSQL.sql`, `seed_admin_account.sql`, `BaseAdministrativeUnit.sql`, `seed_administrative_unit.sql`, `BaseBooking.sql`); **không được sửa** các file đó.

## 5. API / giao diện
| Thành phần | Hành vi | Lỗi |
|---|---|---|
| Combo **Module** | Gộp `Sites[*].Modules[*]` hiển thị `Site / Module`; chọn thì quét thư mục và tick sẵn đích theo `DefaultConnection` | `FolderPath` rỗng/không tồn tại, không có thư mục `SQL\` → thông báo rõ, danh sách trống |
| Danh sách file | `*.sql` trong `{FolderPath}\SQL\`, không đệ quy, sắp theo tên | Không có file → "Không có file .sql" |
| Preview | `RichTextBox` chỉ đọc, UTF-8 (nhận BOM); file lớn hơn 5 MB chỉ hiện 200 KB đầu và nói rõ | Không đọc được file → báo lỗi, không cho chạy |
| Danh sách **Đích** (`CheckedListBox`) | Liệt kê từ `Manager`; đích chưa Sẵn sàng hoặc chưa bật ghi bị mờ, không tick được, tooltip nêu lý do | Không có đích hợp lệ → nút Chạy khóa |
| Tùy chọn **Dừng khi một đích lỗi** | Mặc định bật | — |
| Nút **Chạy** | Khóa khi chưa chọn file hoặc đích; mở `FrmSqlDangerConfirm` | Gõ sai thì không chạy |
| Nút **Hủy** | Hiện khi đang chạy; dừng đích hiện tại (rollback đích đó), đích sau đánh dấu *Bỏ qua* | — |
| Bảng kết quả | Mỗi **đích** một dòng: Alias, Host/DB, Trạng thái (OK / ROLLBACK / BỎ QUA / HỦY), batch lỗi + dòng + thông điệp; có chi tiết từng batch khi chọn dòng | — |

## 6. Tên theo convention
- `SqlBatch(int Index, int StartLine, string Text, int RepeatCount)`; `record SqlRunProgress(int BatchIndex, int TotalBatches, string Status)`; `record SqlRunResult(bool Success, bool RolledBack, bool UsedTransaction, int ExecutedBatches, int FailedBatchIndex, int FailedLine, string? ErrorMessage)`; `record SqlScriptWarning(int BatchIndex, int Line, string Keyword)`; `enum SqlTargetOutcome { Ok, RolledBack, Skipped, Cancelled }`.
- `SqlScriptRunner` (static): `SplitBatches(string script)`, `FindNonTransactional(IEnumerable<SqlBatch>)`, `FindDestructive(IEnumerable<SqlBatch>)`, `Task<SqlRunResult> RunAsync(SqlConnectionSession target, IReadOnlyList<SqlBatch> batches, bool useTransaction, IProgress<SqlRunProgress>? progress, CancellationToken ct)` (một đích; vòng lặp nhiều đích nằm ở UI/tab).
- `ViewActionLogger` (static): `bool Write(string viewName, string action, string host, string database, string target, string result)`.
- `FrmSqlDangerConfirm(string title, string message, IReadOnlyList<string> targetLines, string expectedText)` → `DialogResult`.
- `UC_ConfigSQLRunFile` (UserControl); control `cbo_Module`, `lst_Files`, `rtb_Preview`, `clb_Targets`, `chk_StopOnError`, `btn_Run`, `btn_Cancel`, `dgv_Result`.
- `ModuleItem.DefaultConnection` (XML `defaultConnection`).

## 7. Luồng xử lý dự kiến
1. Chọn Module → `Directory.GetFiles(Path.Combine(folderPath, "SQL"), "*.sql", SearchOption.TopDirectoryOnly)` → danh sách; chọn một file → đọc, hiện preview.
2. Chọn đích trong `clb_Targets`. Bấm **Chạy**: với từng đích đã chọn gọi `RequireWrite()`; `SplitBatches` một lần; `FindDestructive` (nếu có, thêm cảnh báo); `FindNonTransactional` có kết quả → hỏi **một lần**: *Chạy ngoài transaction (không rollback được)* / *Hủy*; không có → `useTransaction = true`.
3. `FrmSqlDangerConfirm`: danh sách `targetLines` ("Alias — Host / Database"); `expectedText` = tên DB nếu 1 đích, = số lượng đích (ví dụ `3`) nếu nhiều đích.
4. **Vòng lặp đích (tuần tự):** `RunAsync(target, ...)`: `OpenAsync`, `BeginTransactionAsync` nếu `useTransaction`; mỗi batch một `SqlCommand` (CommandTimeout `0`, hủy bằng `CancellationToken`), lặp `RepeatCount` lần (`GO n`); `SqlException`/hủy → `RollbackAsync` (nếu có transaction), trả `SqlRunResult` kèm batch/dòng lỗi (dòng lỗi = `StartLine` + `SqlException.LineNumber - 1`); thành công → `CommitAsync`. Lỗi ở một đích: nếu **Dừng khi lỗi** thì các đích chưa chạy = *Bỏ qua*, ngược lại chạy tiếp.
5. UI ghi kết quả từng đích. Thông báo cuối nêu rõ: đích nào **đã commit** (và **không** được hoàn tác), đích nào **đã rollback**, đích nào bị **bỏ qua**; khi chạy ngoài transaction, nêu "không rollback được".
6. Nhật ký, một dòng mỗi đích: `ViewActionLogger.Write("ConfigSQL", "RunSqlFile", host, db, <tên file>, "OK|ROLLBACK|SKIP|CANCEL")`; **không** ghi nội dung script, password, chuỗi kết nối.
7. **Tách batch:** một dòng là `GO` khi khớp `^\s*GO(\s+\d+)?\s*(--.*)?$` (không phân biệt hoa thường) **và không nằm trong khối `/* ... */`**; `GOTO`, `-- GO`, `'GO'` trong chuỗi không tính; batch rỗng/chỉ chú thích bị bỏ; `StartLine` là số dòng (từ 1) của dòng đầu batch.
8. **Phát hiện từ khóa** (không phân biệt hoa thường, bỏ qua chú thích `--` và `/* */`): không-transaction = `BACKUP`, `RESTORE`, `CREATE DATABASE`, `ALTER DATABASE`, `DROP DATABASE`, `DBCC FREEPROCCACHE|DROPCLEANBUFFERS|FREESYSTEMCACHE`, `KILL`, `RECONFIGURE`; phá hủy = `DROP`, `TRUNCATE`, `DELETE`. Báo nhầm do từ khóa nằm trong chuỗi ký tự là chấp nhận được (chỉ dẫn tới hỏi thêm).
9. **ViewActionLogger.Write:** `viewName` chỉ chấp nhận `[A-Za-z0-9_]` (chống path traversal, không hợp lệ trả `false`); đường dẫn `Path.Combine(AppContext.BaseDirectory, "Logs", viewName, $"{DateTime.Now:yyyyMMdd}.log")`; tự tạo thư mục; mỗi dòng `yyyy-MM-dd HH:mm:ss | action | host | database | target | result`; thay `\r`, `\n`, `\t` bằng khoảng trắng, cắt mỗi trường ở 500 ký tự; ghi nối tiếp UTF-8 dưới `lock`; lỗi ghi file trả `false` (không ném) để UI cảnh báo.

**Mẫu một dòng nhật ký (giá trị giả):**
```
2026-11-15 17:42:10 | RunSqlFile | localhost,1433 | AppDb_Target1 | BaseBooking.sql | OK
```

## 8. Tiêu chí nghiệm thu
Phần logic thuần kiểm bằng harness ngoài repo; phần chạy thật cần 🔑 SQL Server 2019+ với **DB thử** (không production).
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `SplitBatches("A\nGO\nB")` → 2 batch, `StartLine` đúng; `go`/`Go`/`  GO  -- note` nhận là GO; `GOTO x`, `-- GO`, `SELECT 'GO'` không tách | Unit/harness (chờ M1) |
| AC2 | `GO` trong khối `/* ... */` nhiều dòng không tách; batch rỗng/chỉ chú thích bị bỏ | Unit/harness (chờ M1) |
| AC3 | `GO 3` cho batch `RepeatCount == 3`; `GO` không số cho `1` | Unit/harness (chờ M1) |
| AC4 | `FindNonTransactional` thấy `BACKUP DATABASE`, `restore database`, `CREATE DATABASE`, `ALTER DATABASE`, `DBCC FREEPROCCACHE`, `KILL 55`, `RECONFIGURE` kèm đúng batch/dòng; bỏ qua khi nằm trong chú thích | Unit/harness (chờ M1) |
| AC5 | `FindDestructive` thấy `DROP TABLE`, `TRUNCATE TABLE`, `DELETE FROM`; script chỉ `CREATE TABLE`/`INSERT` thì rỗng | Unit/harness (chờ M1) |
| AC6 | `ViewActionLogger.Write("ConfigSQL", ...)` tạo `Logs/ConfigSQL/<yyyyMMdd>.log`, dòng đúng định dạng, `\r\n\t` bị thay bằng khoảng trắng, trường > 500 ký tự bị cắt; `viewName` chứa `..`, `/`, `\` hoặc ký tự lạ trả `false` và không tạo file; hai thread ghi 200 dòng mỗi bên đủ 400 dòng | Unit/harness (chờ M1) |
| AC7 | **1 đích:** file 3 batch, batch 2 lỗi cú pháp → `RolledBack=true`, `FailedBatchIndex=2`, dòng lỗi đúng; đối tượng tạo ở batch 1 **không tồn tại** sau đó | Thủ công (🔑 SQL Server) |
| AC8 | **3 đích** (3 DB thử), file hợp lệ: cả 3 `OK`, đối tượng có ở cả 3; chạy lại lần 2 thì cả 3 lỗi "đã tồn tại" và rollback từng đích | Thủ công (🔑) |
| AC9 | **3 đích**, đích thứ 2 có sẵn bảng trùng nên lỗi, **Dừng khi lỗi bật**: đích 1 `OK` và **đã commit** (giao diện nói rõ không hoàn tác), đích 2 `ROLLBACK`, đích 3 `BỎ QUA` và chưa bị đụng tới | Thủ công (🔑) |
| AC10 | Như AC9 nhưng **tắt** Dừng khi lỗi: đích 3 vẫn chạy và `OK` | Thủ công (🔑) |
| AC11 | Script có `ALTER DATABASE`/`BACKUP`: hộp hỏi **đúng một lần** cho cả lượt; Hủy thì không đích nào chạy; chọn chạy ngoài transaction thì chạy và thông báo "không rollback được" | Thủ công (🔑) |
| AC12 | Đích chưa Sẵn sàng hoặc chưa bật ghi bị mờ kèm tooltip; không chọn đích nào thì Chạy khóa; chọn nhiều đích thì phải gõ đúng **số lượng đích**, 1 đích thì gõ đúng **tên DB**; gõ sai không chạy | Thủ công |
| AC13 | Bấm Hủy giữa chừng (file có `WAITFOR DELAY`, 3 đích): đích đang chạy rollback, đích sau `BỎ QUA`, đích đã xong trước đó giữ nguyên kết quả | Thủ công (🔑) |
| AC14 | Chọn Module `Gateway` tick sẵn đích có `TemplateName == "CenterConnection"` nếu có, nếu không có thì gợi ý thêm kết nối từ mẫu; không tự kết nối, không sửa `system.config` (so sánh trước/sau) | Thủ công |
| AC15 | `FolderPath` rỗng, thư mục không tồn tại, không có `SQL\`, thư mục rỗng: mỗi trường hợp có thông báo riêng, không crash; file > 5 MB: preview cắt 200 KB nhưng vẫn chạy được cả file | Thủ công |
| AC16 | Mỗi lần chạy có **một dòng nhật ký cho mỗi đích**, không chứa nội dung script, password hay chuỗi kết nối | Thủ công/Review |
| AC17 | `system.config` cũ không có `defaultConnection` vẫn đọc được (`DefaultConnection == ""`, không lỗi) | Unit/harness hoặc thủ công |
| AC18 | `git diff` chỉ gồm file ở mục 3; không sửa các file `.sql` nguồn; build 0 Error, không `WFO1000` | Build |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx` (`-m:1` nếu hết bộ nhớ). Tester dựng harness console tạm ngoài repo cho AC1–AC6 và AC17; AC7–AC13 cần các DB thử trên SQL Server thật (có thể 4 DB trên cùng một instance).
- Chạy thử bằng script thử `N3_TEST_*` do tester tự viết, **không** chạy `BaseSQL.sql` hay file thật lên DB dùng chung khi chưa được phép.
