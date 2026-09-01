<div class="cover">
  <div class="doc-type">SOFTWARE REQUIREMENTS SPECIFICATION</div>
  <h1>Hệ thống Quản lý &amp; Đặt sân Thể thao</h1>
  <div class="subtitle">SRS — theo cấu trúc tham chiếu IEEE 830</div>
  <table>
    <tr><td>Mã tài liệu</td><td>SRS-BOOKING-001</td></tr>
    <tr><td>Phiên bản</td><td>1.0</td></tr>
    <tr><td>Ngày ban hành</td><td>2026-09-01</td></tr>
    <tr><td>Tài liệu liên quan</td><td>BRD-BOOKING-001, FSR-BOOKING-001, TDD-BOOKING-001</td></tr>
    <tr><td>Đối tượng đọc</td><td>Kiến trúc sư, Đội phát triển, QA, DevOps</td></tr>
  </table>
</div>

## 1. Giới thiệu (Introduction)

### 1.1 Mục đích tài liệu
Đặc tả đầy đủ yêu cầu phần mềm — bao gồm yêu cầu chức năng, phi chức năng, giao diện ngoài và ràng buộc kỹ
thuật — làm cơ sở thiết kế, phát triển, kiểm thử và nghiệm thu Hệ thống Quản lý &amp; Đặt sân Thể thao.

### 1.2 Phạm vi sản phẩm
Sản phẩm là 1 microservice (`Booking`) bổ sung vào nền tảng dùng chung **BaseRepo**, cung cấp năng lực quản
lý sân/giá và đặt sân cho 1 cụm sân thể thao, tích hợp với hạ tầng đăng nhập/phân quyền (`Gateway`,
`Site`) đã có sẵn của nền tảng.

### 1.3 Định nghĩa, từ viết tắt

| Thuật ngữ | Giải thích |
|---|---|
| BRD | Business Requirements Document |
| FSR | Functional Specification / Requirements |
| SRS | Software Requirements Specification (tài liệu này) |
| RBAC | Role-Based Access Control |
| JWT | JSON Web Token |
| YARP | Yet Another Reverse Proxy — thư viện Gateway của .NET |
| CQRS | Command Query Responsibility Segregation |
| Site | Đơn vị/tenant trong kiến trúc BaseRepo |
| IDOR | Insecure Direct Object Reference — lỗ hổng truy cập chéo dữ liệu qua ID |

### 1.4 Tài liệu tham chiếu
- BRD-BOOKING-001 — Business Requirements Document
- FSR-BOOKING-001 — Functional Specification/Requirements
- TDD-BOOKING-001 — Technical Design &amp; Diagrams
- `docs/superpowers/specs/2026-09-01-booking-phase1-design.md` — đặc tả kỹ thuật chi tiết cấp triển khai
- `.claude/rules/` — bộ quy tắc kiến trúc/coding của nền tảng BaseRepo

### 1.5 Tổng quan tài liệu
Mục 2 mô tả tổng quan sản phẩm. Mục 3 đặc tả chi tiết yêu cầu (giao diện ngoài, chức năng, phi chức năng, dữ
liệu). Mục 4 là phụ lục.

---

## 2. Mô tả tổng quan (Overall Description)

### 2.1 Bối cảnh sản phẩm (Product Perspective)
Hệ thống **không phải là sản phẩm độc lập** — nó là 1 trong nhiều microservice chạy trên nền tảng
**BaseRepo**, dùng chung:

- **Gateway** (YARP reverse proxy) — điểm vào duy nhất, xác thực JWT, định tuyến tới các service.
- **Site service** — quản lý người dùng nội bộ, vai trò (Role), quyền hạn (Function), và khái niệm **Site**
  (đơn vị/tenant — cụm sân hiện tại là 1 Site).
- **CommonLibs** — thư viện dùng chung (truy cập dữ liệu qua Dapper, JWT, cache Redis, xử lý lỗi chuẩn).

Hệ thống **không** triển khai lại cơ chế đăng nhập/phân quyền cho nhân viên — tái sử dụng 100% hạ tầng đã có
của nền tảng, chỉ bổ sung service mới (`Booking`) cho nghiệp vụ đặt sân.

### 2.2 Chức năng sản phẩm (tóm tắt)
Xem chi tiết tại FSR-BOOKING-001. Tóm tắt: quản lý sân/giá, đặt sân (lẻ + định kỳ), vận hành nội bộ
(Admin/Lễ tân), thanh toán, POS, báo cáo.

### 2.3 Nhóm người dùng (User Classes)

| Nhóm | Tần suất sử dụng | Mức kỹ năng công nghệ giả định |
|---|---|---|
| Khách hàng (guest) | Không thường xuyên | Thấp — phải thao tác được không cần hướng dẫn |
| Khách hàng (thành viên) | Thường xuyên nếu hay đặt sân | Thấp–trung bình |
| Lễ tân | Hằng ngày, liên tục trong ca làm | Trung bình |
| Admin | Định kỳ (cấu hình), không liên tục | Trung bình–cao |

### 2.4 Môi trường vận hành (Operating Environment)

| Thành phần | Công nghệ |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API |
| Kiến trúc | Clean Architecture (5 layer) + CQRS/MediatR |
| Data access | Dapper trên SQL Server |
| Cache | Redis |
| Reverse Proxy | YARP (tại Gateway) |
| Trình duyệt hỗ trợ (Frontend, ngoài phạm vi service backend) | Chrome/Edge/Safari/Firefox bản hiện hành, có responsive cho di động |

### 2.5 Ràng buộc thiết kế (Design Constraints)
- Phải tuân thủ đúng khuôn kiến trúc 5-layer sẵn có của BaseRepo (`{Service}.Api/.Application/.Contract/
  .Domain/.Infrastructure`) — không được tạo cấu trúc khác biệt cho service mới.
- Mọi API phải trả về đúng schema chuẩn `Response&lt;T&gt;` (`Success`, `Code`, `Message`, `Data`) đã áp
  dụng toàn hệ thống.
- Không được thêm cơ chế xác thực JWT thứ hai chồng chéo lên `AuthorizationMiddleware` hiện có cho luồng
  nhân viên — chỉ bổ sung cơ chế riêng cho luồng Khách hàng, tách bạch hoàn toàn.
- `SiteId` của mọi tài nguyên gắn với 1 cụm sân **bắt buộc phải lấy từ JWT claim của người gọi**, không được
  tin theo giá trị client tự gửi lên — đây là yêu cầu bảo mật bắt buộc, không phải khuyến nghị.

### 2.6 Giả định &amp; phụ thuộc (Assumptions &amp; Dependencies)
- Giả định hạ tầng dùng chung (Gateway, Site, Redis, SQL Server) đã sẵn sàng và ổn định trước khi triển khai
  service `Booking`.
- Phụ thuộc vào cơ chế phân quyền theo Function (`RequireFunction`) đã có sẵn — mọi hành động ghi dữ liệu
  đều cần được cấp quyền tường minh qua `Role_Function`, không có cơ chế "Admin toàn hệ thống" bỏ qua kiểm
  tra này.

---

## 3. Yêu cầu cụ thể (Specific Requirements)

### 3.1 Yêu cầu giao diện ngoài (External Interface Requirements)

#### 3.1.1 Giao diện người dùng
- **Website Đặt sân** (khách hàng): responsive, hỗ trợ desktop và di động, không yêu cầu tài khoản để xem
  giá/lịch trống.
- **Website Quản lý nội bộ** (Admin/Lễ tân): giao diện dạng dashboard/bảng dữ liệu, có màn hình lịch dạng
  lưới (sân × khung giờ).

#### 3.1.2 Giao diện phần mềm (API)
- Chuẩn **REST/JSON** qua HTTP(S), toàn bộ đi qua Gateway (`http://&lt;gateway-host&gt;/api/...`).
- Tài liệu API tự sinh qua **Swagger/OpenAPI**, gộp chung tại Gateway (`/booking-swagger`, `/site-swagger`...).
- Header bắt buộc cho request nội bộ (staff): `Authorization: Bearer &lt;token&gt;`, `X-Device-ID`,
  `X-Device-Name`.

#### 3.1.3 Giao diện phần cứng
Không có yêu cầu phần cứng đặc thù — chạy trên máy chủ ứng dụng thông thường (Windows Server/Linux container
tương thích .NET 10).

#### 3.1.4 Giao diện truyền thông
- HTTPS cho môi trường production.
- Kết nối SQL Server qua TCP (`localhost,1433` môi trường dev — cấu hình theo từng môi trường triển khai).
- Kết nối Redis qua TCP (cache phân quyền người dùng, TTL 1 giờ).
- (Phase 2) Kết nối API cổng thanh toán bên thứ ba (VNPay/MoMo/ZaloPay) qua HTTPS.

### 3.2 Yêu cầu chức năng (Functional Requirements)

> Danh sách đầy đủ, chi tiết theo actor/luồng/quy tắc nghiệp vụ: xem **FSR-BOOKING-001**. Bảng dưới đây liệt
> kê tóm tắt để đối chiếu ngược với BRD.

<table>
<tr><th>Mã</th><th>Yêu cầu</th><th>Đối chiếu BRD</th><th>Trạng thái</th></tr>
<tr><td>FR-01</td><td>Quản lý danh mục loại sân dùng chung</td><td>BR-01</td><td><span class="tag tag-phase1">Đã build</span></td></tr>
<tr><td>FR-02</td><td>Quản lý sân cụ thể, gắn Site + loại sân</td><td>BR-02</td><td><span class="tag tag-phase1">Đã build</span></td></tr>
<tr><td>FR-03</td><td>Cấu hình giá theo khung giờ/ngày trong tuần</td><td>BR-03</td><td><span class="tag tag-phase1">Đã build</span></td></tr>
<tr><td>FR-04</td><td>Đặt sân lẻ/định kỳ, chống trùng lịch bắt buộc</td><td>BR-05, BR-07, BR-08</td><td><span class="tag tag-outscope">Đã đặc tả</span></td></tr>
<tr><td>FR-05</td><td>Đặt sân không cần tài khoản (guest)</td><td>BR-06</td><td><span class="tag tag-outscope">Đã đặc tả</span></td></tr>
<tr><td>FR-06</td><td>Lễ tân xử lý booking + tạo hộ walk-in</td><td>BR-09, BR-10, BR-11</td><td><span class="tag tag-outscope">Đã đặc tả</span></td></tr>
<tr><td>FR-07</td><td>POS bán hàng hóa + tồn kho</td><td>BR-15, BR-16</td><td><span class="tag tag-phase2">Kế hoạch</span></td></tr>
<tr><td>FR-08</td><td>Thanh toán online (VNPay/MoMo/ZaloPay)</td><td>BR-14</td><td><span class="tag tag-phase2">Kế hoạch</span></td></tr>
<tr><td>FR-09</td><td>Báo cáo/thống kê qua Power BI</td><td>BR-17, BR-18</td><td><span class="tag tag-phase3">Kế hoạch</span></td></tr>
</table>

### 3.3 Yêu cầu phi chức năng (Non-Functional Requirements)

#### 3.3.1 Bảo mật (Security) — NFR-SEC

| Mã | Yêu cầu |
|---|---|
| NFR-SEC-01 | Mọi API ghi dữ liệu phải xác thực JWT hợp lệ, kèm kiểm tra quyền theo Function cụ thể (`RequireFunction`) — không có cơ chế bỏ qua kiểm tra ("Admin toàn hệ thống") cho bất kỳ vai trò nào. |
| NFR-SEC-02 | `SiteId` của mọi tài nguyên phải luôn được suy ra từ JWT claim của người gọi ở tầng xử lý nghiệp vụ (repository), không chỉ ở tầng giao diện — chống giả mạo dữ liệu đầu vào. |
| NFR-SEC-03 | Không được để lộ khả năng 1 người dùng thuộc cụm sân này thao tác/đọc dữ liệu thuộc cụm sân khác qua bất kỳ endpoint nào, kể cả khi biết chính xác ID bản ghi (chống IDOR — tham chiếu sự cố đã phát hiện và khắc phục trong quá trình phát triển: lỗ hổng cho phép sửa dữ liệu giá của cụm sân khác qua ID, đã vá trước khi hợp nhất vào nhánh chính). |
| NFR-SEC-04 | Mật khẩu và dữ liệu nhạy cảm không được ghi ra log dưới bất kỳ hình thức nào. |
| NFR-SEC-05 | API đọc công khai (xem sân/giá) không được vô tình để lộ dữ liệu nội bộ nhạy cảm (thông tin nhân viên, cấu hình hệ thống). |

#### 3.3.2 Hiệu năng (Performance) — NFR-PERF

| Mã | Yêu cầu |
|---|---|
| NFR-PERF-01 | Truy vấn danh sách (sân, giá, booking) phải hỗ trợ phân trang, không trả toàn bộ dữ liệu trong 1 lần gọi. |
| NFR-PERF-02 | Thông tin phân quyền người dùng được cache (Redis, TTL 1 giờ) để giảm tải truy vấn CSDL lặp lại trên mỗi request. |
| NFR-PERF-03 | Kiểm tra trùng lịch khi đặt sân phải hoàn tất trong thời gian phản hồi chấp nhận được cho một thao tác tương tác trực tiếp của người dùng (không phải xử lý nền/bất đồng bộ). |

#### 3.3.3 Độ tin cậy &amp; Khả dụng (Reliability &amp; Availability) — NFR-REL

| Mã | Yêu cầu |
|---|---|
| NFR-REL-01 | Mỗi service có cơ sở dữ liệu riêng — sự cố ở 1 service (VD `Booking`) không được làm sập hoàn toàn service khác (`Site`, `AdministrativeUnit`). |
| NFR-REL-02 | Lỗi hệ thống không lường trước phải được xử lý tập trung (global exception handling), luôn trả về response đúng chuẩn thay vì để lộ stack trace/lỗi kỹ thuật thô cho client. |

#### 3.3.4 Khả năng bảo trì &amp; Mở rộng (Maintainability &amp; Scalability) — NFR-MAINT

| Mã | Yêu cầu |
|---|---|
| NFR-MAINT-01 | Toàn bộ code phải tuân thủ đúng convention đặt tên và cấu trúc layer đã chuẩn hóa trong nền tảng, để bất kỳ lập trình viên nào quen với 1 service cũng đọc hiểu được service mới ngay. |
| NFR-MAINT-02 | Kiến trúc phải cho phép mở rộng thêm 1 cụm sân (Site) mới trong tương lai mà không cần thiết kế lại dữ liệu hay code hiện có — chỉ cần khởi tạo thêm bản ghi Site. |
| NFR-MAINT-03 | Mọi thay đổi schema cơ sở dữ liệu phải đi kèm script SQL độc lập, có thể chạy lại được, tách biệt theo từng service. |

#### 3.3.5 Khả năng sử dụng (Usability) — NFR-USE

| Mã | Yêu cầu |
|---|---|
| NFR-USE-01 | Khách hàng phải đặt được sân mà không bắt buộc tạo tài khoản — tối thiểu hóa số bước từ lúc xem lịch trống tới lúc đặt thành công. |
| NFR-USE-02 | Giao diện web nội bộ cho Lễ tân phải hiển thị lịch dạng trực quan (lưới sân × khung giờ), không yêu cầu tra cứu qua nhiều màn hình. |

### 3.4 Yêu cầu dữ liệu (Data Requirements)

- Mô hình dữ liệu chi tiết: xem Mục 3 (Domain/ER Model) của **TDD-BOOKING-001**.
- Mọi entity đều có bộ trường audit chuẩn: `AutoId`, `CreatedDate`, `UpdatedDate`, `CreatedBy`, `UpdatedBy`,
  `Status` (soft-delete: Active/Inactive/Deleted), `DeviceName`, `DeviceId`.
- Trạng thái nghiệp vụ riêng của từng entity (VD `Field.OperationalStatus`: Active/Maintenance) phải là
  trường **tách biệt hoàn toàn** khỏi trường `Status` dùng cho soft-delete — không được dùng chung 1 trường
  cho 2 mục đích khác nhau.
- Dữ liệu khách hàng (Phase 1 — tiếp theo) cần thu thập tối thiểu: họ tên, số điện thoại, email; các trường
  mở rộng phục vụ CRM (ngày sinh, giới tính, địa chỉ, đồng ý nhận marketing, nguồn khách, ghi chú) là tùy
  chọn, không bắt buộc.
- **Không lưu trữ số liệu tổng hợp** (tổng số lần đặt, tổng chi tiêu) trực tiếp trên bản ghi khách hàng — các
  số liệu này phải được tính từ dữ liệu giao dịch gốc (Booking) khi cần, để tránh sai lệch dữ liệu khi giao
  dịch bị hủy/sửa.

---

## 4. Phụ lục

### 4.1 Ma trận truy vết yêu cầu (Traceability Matrix — tóm tắt)

BRD (mục tiêu kinh doanh) → FSR (đặc tả chức năng) → SRS/FR (yêu cầu phần mềm) → Mã nguồn/API tương ứng.
Chi tiết đối chiếu từng dòng: xem Mục 3.2.

### 4.2 Lịch sử thay đổi

| Phiên bản | Ngày | Nội dung thay đổi |
|---|---|---|
| 1.0 | 2026-09-01 | Phát hành lần đầu, đồng bộ với đợt triển khai nền tảng service Booking (FieldType/Field/PricingRule) |

<div class="footer-note">
SRS-BOOKING-001 · v1.0 · Tài liệu kỹ thuật chính thức — thay đổi phải được đồng bộ ngược lại BRD/FSR liên quan.
</div>
