# N3-1 — ConfigSQL: khung màn hình, quản lý nhiều kết nối (nguồn/đích), test kết nối, chế độ chỉ đọc/ghi theo từng kết nối · Coding Spec

- **Trạng thái duyệt:** ✅ đã duyệt bởi Minh Thuận ngày 2026-11-15 (đã viết lại 2026-11-15 sau khi người dùng bổ sung yêu cầu nhiều kết nối đồng thời)
- **Yêu cầu gốc:** `../N3-configsql-quan-tri-sql-server/01-requirement.md` (đã chốt 2026-11-15, R1–R3, R19) · **Phụ thuộc:** không · **Task kế tiếp:** N3-2 trở đi đều dựa vào task này

## 1. Phạm vi
Dựng nền cho toàn bộ ConfigSQL; **chưa có tính năng thao tác DB nào** ngoài thêm/test kết nối và xem thông tin máy chủ.
- `UC_ConfigSQL` trở thành trang thật (`IBasePage`), mở được từ thẻ "Cấu hình SQL" ở trang chủ.
- **Danh sách kết nối** (tối đa **10** kết nối đồng thời), mỗi kết nối gồm: Tên gợi nhớ (alias), Vai trò (Nguồn / Đích / Khác, chỉ là nhãn để lọc và gợi ý), Host, Database, kiểu xác thực (SQL/Windows), Username, Password (ẩn), TrustServerCertificate, Timeout lệnh (mặc định 30 giây).
- Nút **Thêm** / **Sửa** / **Xóa** / **Test** / **Test tất cả**. Thêm/Sửa mở form `FrmSqlConnectionEditor` (nhập tay hoặc chọn mẫu từ `FrmMain.SysConfig.Connections` để điền sẵn).
- **Test kết nối** = mở thử, đọc thông tin máy chủ, đóng; kết nối **Sẵn sàng** khi lần test gần nhất **OK**. Mỗi kết nối có trạng thái riêng: *Chưa test / OK / Lỗi*. Test tất cả chạy lần lượt, một kết nối lỗi không làm dừng các kết nối khác.
- **Chế độ ghi theo từng kết nối:** mặc định **Chỉ đọc**; nút **Bật/Tắt ghi** cho kết nối đang chọn (bật cần xác nhận Yes/No; tự tắt khi Sửa, Xóa hoặc test lại lỗi). Kết nối Nguồn chỉ cần đọc nên không bắt buộc bật ghi.
- Tab **Thông tin** hiện thông tin của kết nối đang chọn: Host, DB, login, phiên bản, có phải sysadmin không, chế độ ghi. Phiên bản chính < 15 (trước SQL Server 2019) thì cảnh báo nhưng vẫn cho dùng.
- Điểm mở rộng cho task sau: `Manager` (toàn bộ danh sách), `SelectedSession` (kết nối đang chọn, cho tab dùng một kết nối) + sự kiện `SelectedSessionChanged`, `AddFeatureTab(string title, Control content)`. Có ít nhất một kết nối Sẵn sàng thì các tab mới bật.
- Toàn bộ cấu hình kết nối **chỉ nằm trong RAM**, xóa hết khi đóng trang.

## 2. Ngoài phạm vi
- Chạy file `.sql`, hộp xác nhận mạnh, nhật ký hành động (N3-2); truy vấn đối tượng, backup/copy/restore, tối ưu, chẩn đoán (N3-3 trở đi).
- Lưu/mã hóa chuỗi kết nối, ghi ngược `system.config`, thêm package (không sửa `.csproj`), distributed transaction.
- Sửa `SystemConfig.cs`/`system.config` (N3-2 làm).

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph; đã đọc `UC_Home.cs`, `UC_ConfigSQL.cs` (stub 20 dòng, chỉ `InitializeComponent()`), `SystemConfig.cs`, `system.config`. `UC_ConfigSQL` hiện **không ai tham chiếu**; thẻ "Cấu hình SQL" (`label6`, trong `panel7` của `UC_Home.Designer.cs`) chưa gắn Click.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| `UC_Home` (thêm Click mở `UC_ConfigSQL`) | Luồng điều hướng trang chủ → `UC_ConfigEnv` | Không đổi, chỉ thêm |
| `UC_ConfigSQL` (stub → trang thật) | Không có nơi gọi cũ | Không |
| `SysConfig.Connections` (chỉ đọc) | `FrmMain.ReadSystemConfig` | Không sửa |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `SystemManagement/Utils/SqlConnectionSession.cs` | Tạo | Một kết nối trong RAM: cấu hình, validate, dựng chuỗi, test, thông tin server, cờ ghi, mở connection |
| `SystemManagement/Utils/SqlConnectionManager.cs` | Tạo | Danh sách tối đa 10 kết nối, alias duy nhất, thêm/xóa/xóa hết, test tất cả, sự kiện thay đổi |
| `SystemManagement/Views/ConfigSQL/FrmSqlConnectionEditor.cs` | Tạo | Form thêm/sửa một kết nối (có chọn mẫu từ `Connections`) |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.cs` | Sửa | `IBasePage`, lưới kết nối, nút, bật/tắt ghi, tab Thông tin, `Manager`, `SelectedSession`, `AddFeatureTab` |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.Designer.cs` | Sửa | Bố cục lưới + thanh nút + `TabControl` |
| `SystemManagement/Views/ConfigSQL/UC_ConfigSQL.resx` | Sửa/Tạo | Chỉ nếu Designer sinh/đổi (ví dụ `ToolTip`) |
| `SystemManagement/Views/Home/UC_Home.cs` | Sửa | Handler mở `UC_ConfigSQL` |
| `SystemManagement/Views/Home/UC_Home.Designer.cs` | Sửa | Gắn `Click` cho thẻ "Cấu hình SQL" (`label6` và control hình cùng thẻ trong `panel7`) |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại. Nếu `SystemManagement/Utils/` chưa có thì tạo; namespace `SystemManagement.Utils`.

## 4. Thay đổi cơ sở dữ liệu
Không có (chỉ đọc thông tin server bằng truy vấn đọc).

## 5. API / giao diện
| Thành phần | Hành vi | Lỗi |
|---|---|---|
| Lưới **Kết nối** | Cột: Alias, Vai trò, Host, Database, Xác thực, Trạng thái (Chưa test/OK/Lỗi, tô màu), Ghi (Bật/Tắt). Chọn một dòng thì `SelectedSession` đổi | Danh sách trống: gợi ý bấm Thêm |
| **Thêm** / **Sửa** | Mở `FrmSqlConnectionEditor`; thêm vượt 10 kết nối bị chặn kèm thông báo; alias trùng (không phân biệt hoa thường) bị chặn. Sửa đưa trạng thái về *Chưa test* và tắt ghi | Validate lỗi hiện ngay trong form |
| **Xóa** | Xóa kết nối đang chọn (hộp Yes/No), xóa password khỏi bộ nhớ | — |
| **Test** / **Test tất cả** | `TestAsync` trong `RunWithLoadingAsync`; cập nhật trạng thái từng dòng; thông báo tổng "N OK, M lỗi" | Lỗi từng kết nối nằm ở cột trạng thái/tooltip, không lộ password |
| **Bật/Tắt ghi** | Chỉ bật được khi kết nối đang chọn **Sẵn sàng**; bật: hộp Yes/No nêu rủi ro | Chưa Sẵn sàng thì nút khóa kèm tooltip |
| `FrmSqlConnectionEditor` | Alias, Vai trò, Mẫu (`(Nhập tay)` + tên từng `Connection`), Host, Database, Xác thực, Username, Password (`UseSystemPasswordChar`), TrustServerCertificate, Timeout (5–600) | Mẫu trống hoặc `TrustCertificate` không phải `true/false/1/0` thì coi là `false`; chọn mẫu chỉ điền form, không ghi ngược `system.config` |

## 6. Tên theo convention
Theo `convention-backend.md` (PascalCase, field `_camelCase`, một class một file, comment giải thích "vì sao") và style N1 (`FrmMain.Throw*`, `RunWithLoadingAsync`, control `btn_`/`txt_`/`cbo_`/`lbl_`/`dgv_`).
- `enum SqlAuthMode { Sql, Windows }`; `enum SqlConnectionRole { Source, Target, Other }`; `enum SqlSessionStatus { NotTested, Ok, Failed }`; `record SqlServerInfo(string Version, int MajorVersion, string DatabaseName, string LoginName, bool IsSysAdmin)`.
- `SqlConnectionSession` (sealed): `Id`, `Alias`, `Role`, `TemplateName` (tên mẫu `Connection` nếu chọn từ mẫu, để N3-2 gợi ý đích), `Status`, `LastError`, `ServerInfo`, `Configure(alias, role, templateName, host, database, authMode, username, password, trustServerCertificate, commandTimeoutSeconds)`, `string? Validate()`, `string BuildConnectionString()`, `Task TestAsync(CancellationToken ct)`, `bool IsReady` (`Status == Ok`), `bool AllowWrite`, `void SetAllowWrite(bool)`, `void RequireWrite()` (ném `InvalidOperationException` nếu `AllowWrite == false`), `Task<SqlConnection> OpenAsync(CancellationToken ct)` (mở connection mới, **người gọi tự `Dispose`**), `Task<DataTable> QueryTableAsync(string sql, IReadOnlyDictionary<string, object?>? parameters, CancellationToken ct)` (**chỉ cho câu truy vấn catalog/DMV do code viết sẵn, không bao giờ nhận SQL người dùng gõ**), `bool IsSameDatabase(SqlConnectionSession other)` (cùng Host và Database, không phân biệt hoa thường), `void Clear()` (xóa password, đưa về *Chưa test*, tắt ghi), `string Host`, `string Database`, `int CommandTimeoutSeconds`.
- `SqlConnectionManager`: `const int MaxConnections = 10`, `IReadOnlyList<SqlConnectionSession> Sessions`, `SqlConnectionSession Add(...)` (ném `InvalidOperationException` khi đủ 10 hoặc alias trùng), `bool Remove(Guid id)`, `void Clear()`, `Task TestAllAsync(CancellationToken ct)`, `IEnumerable<SqlConnectionSession> GetReady(SqlConnectionRole? role = null)`, sự kiện `SessionsChanged`, `SessionStateChanged`.
- `FrmSqlConnectionEditor(SqlConnectionManager manager, SqlConnectionSession? editing)` → `DialogResult`.

## 7. Luồng xử lý dự kiến
1. Trang chủ: bấm thẻ "Cấu hình SQL" → `BaseUC.InternalNavigate(new UC_ConfigSQL())` (như `UC_ConfigEnv`). `UC_ConfigSQL` có `BaseUC` và `Title = "Cấu hình SQL"` (hai property có `[DesignerSerializationVisibility(Hidden)]` vì analyzer `WFO1000`); `Responsive()` để rỗng, không ném `NotImplementedException`.
2. Khi load: lưới trống, tab Thông tin hiện "Chưa có kết nối"; các tab feature (nếu có) bị vô hiệu.
3. **Thêm kết nối:** `FrmSqlConnectionEditor` → `Validate()` (Alias bắt buộc, tối đa 50 ký tự, không xuống dòng; Host không rỗng; SQL Auth phải có Username; Database rỗng thì dùng `master`; Timeout 5–600) → `manager.Add` → dòng mới ở trạng thái *Chưa test*.
4. **Test:** `TestAsync`: dựng chuỗi bằng `SqlConnectionStringBuilder` (`DataSource`, `InitialCatalog`, `IntegratedSecurity` hoặc `UserID`/`Password`, `TrustServerCertificate`, `ApplicationName = "BaseRepo.SystemManagement"`, `ConnectTimeout = 15`, `Pooling = false` để đóng connection thật sự giải phóng), mở, chạy `SELECT @@VERSION, CAST(SERVERPROPERTY('ProductMajorVersion') AS int), DB_NAME(), SUSER_SNAME(), IS_SRVROLEMEMBER('sysadmin')`, đóng. Thành công → `Status = Ok`, lưu `ServerInfo`; thất bại → `Status = Failed`, `LastError` chỉ chứa thông điệp đã lọc (không kèm chuỗi kết nối hay password).
5. **Test tất cả:** chạy lần lượt từng kết nối (không song song, để không quá tải máy chủ nhỏ); lỗi một kết nối không dừng các kết nối còn lại.
6. **Bật ghi:** `SetAllowWrite(true)` chỉ khi `IsReady`; mọi thao tác ghi của các task sau phải gọi `session.RequireWrite()` ở **từng kết nối đích**.
7. **Xóa/Đóng trang:** `manager.Clear()` xóa password của mọi kết nối; `Dispose` của `UC_ConfigSQL` cũng gọi `Clear()`.

## 8. Tiêu chí nghiệm thu
Không có SQL Server trên máy dev (🔑 cho nhóm kiểm thủ công). Logic thuần kiểm bằng harness console ngoài repo.
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `BuildConnectionString` SQL Auth chứa `Data Source`, `Initial Catalog`, `User ID`, `Password`, `Application Name=BaseRepo.SystemManagement`, `Connect Timeout=15`, `Pooling=False`, không chứa `Integrated Security=True` | Unit/harness (chờ M1) |
| AC2 | Windows Auth có `Integrated Security=True` và **không** chứa `User ID`/`Password` | Unit/harness (chờ M1) |
| AC3 | `Validate`: Alias rỗng/quá 50 ký tự/có xuống dòng → lỗi; Host rỗng → lỗi; SQL Auth thiếu Username → lỗi; Timeout ngoài 5–600 → lỗi; Database rỗng → hợp lệ và chuỗi dùng `master` | Unit/harness (chờ M1) |
| AC4 | `RequireWrite()` ném `InvalidOperationException` khi `AllowWrite == false`, không ném sau `SetAllowWrite(true)`; `Clear()` đưa `AllowWrite` về `false`, `Status` về `NotTested`, xóa password; bật ghi ở một kết nối **không** làm đổi kết nối khác | Unit/harness (chờ M1) |
| AC5 | `manager.Add` lần thứ 11 ném lỗi; alias trùng (khác hoa thường) ném lỗi; `Remove` giải phóng chỗ; `Clear` xóa hết và password của mọi session bị xóa | Unit/harness (chờ M1) |
| AC6 | `IsSameDatabase`: cùng Host+Database (khác hoa thường) → true; khác Database hoặc khác Host → false | Unit/harness (chờ M1) |
| AC7 | `TestAllAsync` với 3 kết nối trong đó 2 trỏ Host không tồn tại (timeout ngắn): trạng thái đúng từng cái (`Failed`), không ném ra ngoài, không dừng giữa chừng, `LastError` không chứa password | Unit/harness (chờ M1) |
| AC8 | Thêm **1 nguồn + 3 đích** (có thể 4 DB thử trên cùng một instance SQL Server 2019+, không production), **Test tất cả** → 4 dòng `OK`; tab Thông tin hiện đúng DB/login/phiên bản của từng dòng khi chọn | Thủ công (🔑 SQL Server) |
| AC9 | Một kết nối sai password: chỉ dòng đó `Lỗi`, các dòng khác vẫn `OK`, không hiện password ở bất kỳ đâu | Thủ công (🔑) |
| AC10 | Chọn mẫu từ `Connections` điền đủ form; sửa form rồi lưu không làm đổi `FrmMain.SysConfig` và không ghi `system.config` (so sánh file trước/sau) | Thủ công |
| AC11 | Windows Auth ẩn/khóa ô Username và Password | Thủ công |
| AC12 | Sửa kết nối → trạng thái về *Chưa test* và ghi về Chỉ đọc; Xóa → mất khỏi lưới; đóng trang (Back) xóa toàn bộ | Thủ công |
| AC13 | Bật/Tắt ghi độc lập từng kết nối, cần xác nhận Yes/No khi bật; chưa `OK` thì nút khóa kèm tooltip | Thủ công |
| AC14 | Server < 2019: hiện cảnh báo, vẫn dùng được (nếu có máy cũ; không có thì xác nhận bằng đọc code) | Thủ công/Review |
| AC15 | Thẻ "Cấu hình SQL" (cả chữ và hình) mở `UC_ConfigSQL`; nút Back quay về trang chủ; thẻ ConfigEnv vẫn mở bình thường | Thủ công |
| AC16 | Không có `Console.*`/`Log.*`/`Debug.*`/`Trace.*` ghi password hoặc chuỗi kết nối; ô Password dùng `UseSystemPasswordChar`; thông báo lỗi không chứa password | Review/grep |
| AC17 | `git diff` chỉ gồm các file ở mục 3; `dotnet build BaseRepo.slnx` 0 Error, không `WFO1000`; `dotnet build -c Release SystemManagement` (có Obfuscar) vẫn thành công | Build |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx` (`-m:1` nếu compiler hết bộ nhớ) và `dotnet build SystemManagement -c Release` để Obfuscar chạy. Chưa có test project nên tester dựng harness console tạm **ngoài repo** cho AC1–AC7.
- Rủi ro cần báo lại, không tự xử lý: `Microsoft.Data.SqlClient` có native SNI; kiểm tra app Release/PublishSingleFile + Obfuscar còn mở được màn hình (nếu Obfuscar làm hỏng thì báo, có thể cần sửa `obfuscar.xml`, hiện ngoài danh sách file).
- Kiểm thủ công chỉ dùng DB thử và tài khoản thử, không dùng dữ liệu hay server production.
