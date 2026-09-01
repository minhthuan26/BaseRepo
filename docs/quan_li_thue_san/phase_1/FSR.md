<div class="cover">
  <div class="doc-type">FUNCTIONAL SPECIFICATION / REQUIREMENTS</div>
  <h1>Hệ thống Quản lý &amp; Đặt sân Thể thao</h1>
  <div class="subtitle">Đặc tả chức năng chi tiết — FSR</div>
  <table>
    <tr><td>Mã tài liệu</td><td>FSR-BOOKING-001</td></tr>
    <tr><td>Phiên bản</td><td>1.0</td></tr>
    <tr><td>Ngày ban hành</td><td>2026-09-01</td></tr>
    <tr><td>Tài liệu gốc</td><td>BRD-BOOKING-001</td></tr>
    <tr><td>Đối tượng đọc</td><td>BA, QA, Đội phát triển</td></tr>
  </table>
</div>

## 1. Giới thiệu

Tài liệu này đặc tả **chi tiết từng chức năng** của hệ thống, triển khai các yêu cầu nghiệp vụ đã nêu trong
BRD-BOOKING-001. Mỗi chức năng được mô tả theo cấu trúc: Actor, Điều kiện tiên quyết, Luồng chính, Luồng phụ
(exception), Quy tắc nghiệp vụ, và API tương ứng.

Ký hiệu trạng thái: <span class="tag tag-phase1">PHASE 1</span> đã triển khai mã nguồn ·
<span class="tag tag-outscope">PHASE 1 — TIẾP THEO</span> đã đặc tả, chưa code ·
<span class="tag tag-phase2">PHASE 2</span> · <span class="tag tag-phase3">PHASE 3</span> kế hoạch sau.

---

## 2. Ma trận chức năng theo vai trò (Function-Role Matrix)

<table>
<tr><th>Chức năng</th><th>Khách (guest)</th><th>Khách (thành viên)</th><th>Lễ tân</th><th>Admin</th></tr>
<tr><td>Xem danh sách loại sân / sân / giá</td><td>✅</td><td>✅</td><td>✅</td><td>✅</td></tr>
<tr><td>Đặt sân (lẻ / định kỳ)</td><td>✅</td><td>✅</td><td>—</td><td>—</td></tr>
<tr><td>Xem lịch sử đặt sân của bản thân</td><td>❌ (không có tài khoản)</td><td>✅</td><td>—</td><td>—</td></tr>
<tr><td>Tạo booking hộ khách tại quầy (walk-in)</td><td>—</td><td>—</td><td>✅</td><td>✅</td></tr>
<tr><td>Xác nhận / hủy / check-in booking</td><td>—</td><td>—</td><td>✅</td><td>✅</td></tr>
<tr><td>Đánh dấu đã thu tiền mặt</td><td>—</td><td>—</td><td>✅</td><td>✅</td></tr>
<tr><td>Tạo / sửa / xóa Loại sân (FieldType)</td><td>—</td><td>—</td><td>❌</td><td>✅</td></tr>
<tr><td>Tạo / sửa / xóa Sân (Field)</td><td>—</td><td>—</td><td>❌</td><td>✅</td></tr>
<tr><td>Cấu hình giá theo khung giờ (PricingRule)</td><td>—</td><td>—</td><td>❌</td><td>✅</td></tr>
<tr><td>Bán hàng hóa tại quầy (POS)</td><td>—</td><td>—</td><td>✅</td><td>✅</td></tr>
<tr><td>Xem báo cáo/thống kê</td><td>—</td><td>—</td><td>❌</td><td>✅</td></tr>
</table>

---

## 3. Đặc tả chức năng — Module Quản lý sân <span class="tag tag-phase1">PHASE 1</span>

### 3.1 FSR-01 — Quản lý Loại sân (FieldType)

| | |
|---|---|
| **Actor** | Admin |
| **Điều kiện tiên quyết** | Đã đăng nhập, có Function `CreateFieldType`/`UpdateFieldType`/`DeleteFieldType` |
| **Mô tả** | Admin quản lý danh mục loại hình thể thao dùng chung toàn hệ thống (không thuộc riêng cụm sân nào) |
| **API** | `POST /api/field-type`, `PUT /api/field-type/{id}`, `GET /api/field-type/{id}`, `GET /api/field-type`, `DELETE /api/field-type/{id}` |

**Luồng chính — Tạo mới:**
1. Admin nhập `FieldTypeId` (mã định danh, VD `BONGDA`) và `FieldTypeName` (VD "Bóng đá mini").
2. Hệ thống kiểm tra `FieldTypeId` chưa tồn tại.
3. Ghi nhận bản ghi mới, trả về thông tin vừa tạo.

**Quy tắc nghiệp vụ:**
- `FieldTypeId` là duy nhất toàn hệ thống.
- Xóa có thể là xóa mềm (ẩn khỏi danh sách hoạt động) hoặc xóa cứng (loại bỏ khỏi CSDL).
- Danh sách loại sân là **thông tin công khai** — không yêu cầu đăng nhập để xem.

---

### 3.2 FSR-02 — Quản lý Sân (Field)

| | |
|---|---|
| **Actor** | Admin |
| **Điều kiện tiên quyết** | Đã đăng nhập, có Function tương ứng, sân được tạo luôn gắn với cụm sân (Site) của chính Admin đó |
| **Mô tả** | Quản lý từng sân cụ thể — thuộc 1 loại sân và 1 cụm sân |
| **API** | `POST /api/field`, `PUT /api/field/{id}`, `GET /api/field/{id}`, `GET /api/field`, `DELETE /api/field/{id}` |

**Luồng chính — Tạo mới:**
1. Admin nhập `FieldId`, chọn `FieldTypeId` (phải tồn tại trong danh mục), `FieldName`.
2. Hệ thống tự động gán sân này thuộc cụm sân (Site) của Admin đang đăng nhập — **không cho phép Admin tự
   chọn cụm sân khác** kể cả khi cố tình gửi lên trong request.
3. Ghi nhận sân với trạng thái vận hành mặc định "Đang hoạt động".

**Luồng phụ:**
- Nếu `FieldTypeId` không tồn tại → từ chối với lỗi rõ ràng.
- Nếu xóa cứng một sân đang có quy tắc giá (PricingRule) tham chiếu → hệ thống từ chối, yêu cầu xóa/di dời
  quy tắc giá trước.

**Quy tắc nghiệp vụ:**
- Mỗi sân có trạng thái vận hành riêng biệt: **Đang hoạt động** hoặc **Đang bảo trì** — tách biệt hoàn toàn
  với trạng thái "đã xóa mềm" của bản ghi.
- Danh sách sân là thông tin công khai; chỉnh sửa/xóa yêu cầu quyền Admin **của đúng cụm sân sở hữu sân đó**.

---

### 3.3 FSR-03 — Cấu hình Giá theo khung giờ (PricingRule)

| | |
|---|---|
| **Actor** | Admin |
| **Điều kiện tiên quyết** | Sân (Field) đã tồn tại và thuộc cụm sân của Admin |
| **Mô tả** | Cấu hình giá tiền theo từng khung giờ + ngày trong tuần cho 1 sân cụ thể |
| **API** | `POST /api/pricing-rule`, `PUT /api/pricing-rule/{autoId}`, `GET /api/pricing-rule/{autoId}`, `GET /api/pricing-rule`, `DELETE /api/pricing-rule/{autoId}` |

**Luồng chính — Tạo mới:**
1. Admin chọn sân, chọn ngày trong tuần, khung giờ bắt đầu/kết thúc, đơn giá/giờ.
2. Hệ thống xác thực: giờ kết thúc phải sau giờ bắt đầu; đơn giá phải lớn hơn 0; ngày trong tuần hợp lệ
   (Thứ Hai → Chủ Nhật).
3. Hệ thống xác nhận sân này thuộc đúng cụm sân của Admin trước khi ghi nhận.

**Quy tắc nghiệp vụ:**
- Cho phép nhiều quy tắc giá khác nhau trên cùng 1 sân, miễn là khung giờ không trùng lặp về mặt logic
  nghiệp vụ (VD: 6h–17h giá thường, 17h–22h giá giờ vàng).
- Không có mã định danh nghiệp vụ riêng — mỗi quy tắc giá được nhận diện bằng mã hệ thống tự sinh.

---

## 4. Đặc tả chức năng — Module Đặt sân <span class="tag tag-outscope">PHASE 1 — TIẾP THEO</span>

> Các chức năng dưới đây đã được đặc tả đầy đủ trong tài liệu kỹ thuật chi tiết
> (`2026-09-01-booking-phase1-design.md`) và nằm trong phạm vi Phase 1, sẽ triển khai mã nguồn ở đợt phát
> triển kế tiếp trên nền service `Booking` đã dựng xong.

### 4.1 FSR-04 — Đăng ký / Đăng nhập Khách hàng

| | |
|---|---|
| **Actor** | Khách hàng |
| **Mô tả** | Khách hàng đăng ký tài khoản (không bắt buộc) bằng Email/SĐT + mật khẩu để lưu lịch sử đặt sân |

**Quy tắc nghiệp vụ:**
- Tài khoản khách hàng **độc lập hoàn toàn** với tài khoản nhân viên (không dùng chung hệ đăng nhập nội bộ).
- Không bắt buộc đăng ký để đặt sân — khách vãng lai chỉ cần cung cấp họ tên + số điện thoại.

### 4.2 FSR-05 — Đặt sân theo khung giờ đơn lẻ

| | |
|---|---|
| **Actor** | Khách hàng (guest hoặc thành viên) |
| **Mô tả** | Chọn sân, ngày, khung giờ còn trống để đặt |

**Luồng chính:**
1. Khách xem lịch trống của sân theo ngày (công khai, không cần đăng nhập).
2. Khách chọn khung giờ, nhập thông tin liên hệ (nếu là guest) hoặc dùng tài khoản đã đăng nhập.
3. Hệ thống **kiểm tra trùng lịch** — nếu khung giờ đã có người đặt, từ chối ngay lập tức với thông báo rõ
   ràng.
4. Ghi nhận booking ở trạng thái "Chờ xác nhận".

**Quy tắc nghiệp vụ (bắt buộc, không có ngoại lệ):**
- **Không được phép tồn tại 2 booking trùng khung giờ trên cùng 1 sân.** Đây là ràng buộc quan trọng nhất hệ
  thống.

### 4.3 FSR-06 — Đặt sân định kỳ

| | |
|---|---|
| **Actor** | Khách hàng |
| **Mô tả** | Đặt lặp lại theo 1 thứ trong tuần + khung giờ cố định, trong 1 khoảng thời gian |

**Luồng chính:**
1. Khách chọn sân, thứ trong tuần, khung giờ, ngày bắt đầu và ngày kết thúc chuỗi đặt định kỳ.
2. Hệ thống sinh ra nhiều booking đơn lẻ tương ứng từng buổi trong khoảng thời gian đó.
3. Với mỗi buổi, hệ thống áp dụng đúng quy tắc chống trùng lịch như đặt lẻ (FSR-05).

**Quy tắc nghiệp vụ:**
- Nếu 1 buổi trong chuỗi bị trùng lịch, hệ thống **bỏ qua đúng buổi đó**, không hủy toàn bộ chuỗi — trả về
  danh sách buổi thành công và buổi thất bại để khách biết.

### 4.4 FSR-07 — Lễ tân xử lý booking

| | |
|---|---|
| **Actor** | Lễ tân |
| **Mô tả** | Xem lịch dạng lưới (sân × khung giờ), xác nhận/hủy/check-in, tạo booking hộ khách walk-in |

**Quy tắc nghiệp vụ:**
- Lễ tân tạo booking hộ khách walk-in theo đúng luồng chống trùng lịch giống khách tự đặt (FSR-05) — không
  có đường tắt bỏ qua kiểm tra này dù là nhân viên thao tác.
- Lễ tân đánh dấu trạng thái thanh toán (đã thu tiền mặt) thủ công cho từng booking ở Phase 1.

---

## 5. Đặc tả chức năng — POS &amp; Thanh toán online <span class="tag tag-phase2">PHASE 2</span>

| Mã | Chức năng | Actor | Ghi chú |
|---|---|---|---|
| FSR-08 | Bán hàng hóa (đồ uống, phụ kiện) tại quầy | Lễ tân | Có kèm quản lý tồn kho cơ bản |
| FSR-09 | Thanh toán qua cổng online (VNPay/MoMo/ZaloPay) | Khách hàng | Áp dụng khi đặt sân online cần đặt cọc/thanh toán ngay |

---

## 6. Đặc tả chức năng — Thống kê &amp; Power BI <span class="tag tag-phase3">PHASE 3</span>

| Mã | Chức năng | Actor | Ghi chú |
|---|---|---|---|
| FSR-10 | Báo cáo tỷ lệ lấp đầy sân theo khung giờ/loại sân | Admin | Tính từ dữ liệu Booking, không sửa mô hình dữ liệu gốc |
| FSR-11 | Báo cáo hành vi khách hàng (tần suất, chi tiêu) | Admin | Phục vụ ra quyết định marketing (CRM cơ bản) |

---

## 7. Xử lý ngoại lệ chung (Cross-cutting Error Handling)

<table>
<tr><th>Tình huống</th><th>Hành vi hệ thống</th></tr>
<tr><td>Thiếu/sai dữ liệu đầu vào bắt buộc</td><td>HTTP 400 — thông báo rõ trường bị lỗi</td></tr>
<tr><td>Không có quyền (chưa đăng nhập / thiếu Function)</td><td>HTTP 401 (chưa đăng nhập) / 403 (thiếu quyền)</td></tr>
<tr><td>Thao tác lên dữ liệu không thuộc cụm sân của mình</td><td>HTTP 403 — từ chối, không tiết lộ dữ liệu của cụm sân khác</td></tr>
<tr><td>Không tìm thấy bản ghi</td><td>HTTP 404</td></tr>
<tr><td>Trùng khóa nghiệp vụ (VD FieldTypeId đã tồn tại)</td><td>HTTP 409</td></tr>
<tr><td>Xóa cứng bản ghi đang bị tham chiếu bởi dữ liệu con</td><td>HTTP 409 — yêu cầu xử lý dữ liệu con trước</td></tr>
<tr><td>Lỗi hệ thống không lường trước</td><td>HTTP 500 — thông báo chung, không lộ chi tiết kỹ thuật cho người dùng cuối</td></tr>
</table>

<div class="footer-note">
FSR-BOOKING-001 · v1.0 · Tham chiếu chi tiết mức triển khai: xem SRS-BOOKING-001 và
<code>docs/superpowers/specs/2026-09-01-booking-phase1-design.md</code>.
</div>
