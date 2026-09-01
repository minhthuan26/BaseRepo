<div class="cover">
  <div class="doc-type">BUSINESS REQUIREMENTS DOCUMENT</div>
  <h1>Hệ thống Quản lý &amp; Đặt sân Thể thao</h1>
  <div class="subtitle">BaseRepo Sports Facility Booking System</div>
  <table>
    <tr><td>Mã tài liệu</td><td>BRD-BOOKING-001</td></tr>
    <tr><td>Phiên bản</td><td>1.0</td></tr>
    <tr><td>Ngày ban hành</td><td>2026-09-01</td></tr>
    <tr><td>Trạng thái</td><td>Draft — chờ phê duyệt</td></tr>
    <tr><td>Phạm vi hiện tại</td><td>Phase 1 đã triển khai; Phase 2–3 ở mức yêu cầu nghiệp vụ</td></tr>
    <tr><td>Đối tượng đọc</td><td>Chủ đầu tư, Ban vận hành, Đội phát triển</td></tr>
  </table>
</div>

## 1. Tóm tắt tổng quan (Executive Summary)

Hệ thống Quản lý &amp; Đặt sân Thể thao là nền tảng phần mềm phục vụ **1 cụm sân thể thao đa loại hình**
(bóng đá mini, cầu lông, pickleball, tennis...), cho phép:

- Khách hàng **đặt sân trực tuyến** (đặt lẻ theo giờ hoặc đặt định kỳ), không bắt buộc phải có tài khoản.
- Nhân viên (Admin, Lễ tân) **quản lý sân, giá, lịch đặt** và hỗ trợ khách tại quầy qua **web nội bộ**.
- Thu tiền qua **tiền mặt tại quầy** và **cổng thanh toán online** (VNPay/MoMo/ZaloPay).
- Bán thêm hàng hóa (nước uống, phụ kiện) qua **POS** tại quầy.
- Ra **báo cáo/thống kê** vận hành và doanh thu qua **Power BI**.

Hệ thống được xây dựng trên nền **BaseRepo** — kiến trúc microservices .NET dùng chung cho nhiều dự án của
tổ chức, giúp tái sử dụng hạ tầng đăng nhập, phân quyền và giảm chi phí phát triển.

Dự án được triển khai theo **3 giai đoạn** để đưa sản phẩm ra thị trường nhanh và giảm rủi ro:

<table>
  <tr><th>Giai đoạn</th><th>Nội dung chính</th><th>Trạng thái</th></tr>
  <tr>
    <td><span class="tag tag-phase1">PHASE 1</span></td>
    <td>Quản lý sân, giá theo khung giờ; đặt sân online; web nội bộ cơ bản cho Admin/Lễ tân</td>
    <td><b>Đã triển khai</b> (nền tảng quản lý sân)</td>
  </tr>
  <tr>
    <td><span class="tag tag-phase2">PHASE 2</span></td>
    <td>POS bán hàng hóa, tích hợp cổng thanh toán online</td>
    <td>Kế hoạch tiếp theo</td>
  </tr>
  <tr>
    <td><span class="tag tag-phase3">PHASE 3</span></td>
    <td>Thống kê, dashboard, tích hợp Power BI</td>
    <td>Kế hoạch sau cùng</td>
  </tr>
</table>

> **Lưu ý về trạng thái triển khai:** Tính đến thời điểm tài liệu này ban hành, đợt triển khai kỹ thuật đầu
> tiên (nền tảng dịch vụ `Booking`: quản lý loại sân, sân, giá) đã hoàn tất ở mức mã nguồn và build thành
> công. Luồng đặt sân của khách hàng (đối tượng `Customer`, `Booking`, đặt định kỳ) — phần lõi của Phase 1 —
> sẽ được triển khai ở đợt kế tiếp trên cùng nền tảng vừa dựng.

---

## 2. Mục tiêu kinh doanh (Business Objectives)

| # | Mục tiêu | Chỉ số đo lường liên quan |
|---|---|---|
| BO-1 | Số hóa toàn bộ quy trình đặt sân, giảm phụ thuộc vào đặt lịch thủ công qua điện thoại/sổ tay | % booking tạo qua hệ thống so với tổng booking |
| BO-2 | Tối đa hóa tỷ lệ lấp đầy khung giờ sân bằng cách công khai lịch trống theo thời gian thực | Tỷ lệ lấp đầy (occupancy rate) theo tuần |
| BO-3 | Giảm sai sót/trùng lịch đặt sân do thao tác thủ công | Số ca trùng lịch phát sinh / tháng |
| BO-4 | Tăng doanh thu phụ trợ qua bán hàng hóa tại quầy (Phase 2) | Doanh thu POS / doanh thu sân |
| BO-5 | Có dữ liệu để ra quyết định vận hành (giờ cao điểm, loại sân được ưa chuộng...) (Phase 3) | Có báo cáo Power BI cập nhật định kỳ |
| BO-6 | Xây dựng nền tảng có thể mở rộng thêm chi nhánh/dự án khác trong tương lai mà không phải redesign | Thời gian triển khai 1 Site/chi nhánh mới |

---

## 3. Phạm vi dự án (Scope)

### 3.1 Trong phạm vi (In-Scope)

- Quản lý danh mục loại sân, sân cụ thể, giá theo khung giờ/ngày trong tuần.
- Đặt sân online: đặt lẻ theo khung giờ và đặt định kỳ (lặp theo tuần), cho khách vãng lai (guest) lẫn khách
  đã đăng ký tài khoản.
- Web nội bộ cho Admin (toàn quyền quản lý) và Lễ tân (xử lý booking hằng ngày, tạo booking hộ khách tại
  quầy).
- Thanh toán: tiền mặt tại quầy (Phase 1) và cổng thanh toán online VNPay/MoMo/ZaloPay (Phase 2).
- POS bán đồ uống/phụ kiện kèm quản lý tồn kho cơ bản (Phase 2).
- Báo cáo/thống kê vận hành qua Power BI (Phase 3).
- Thông tin khách hàng phục vụ CRM cơ bản (liên hệ, nhân khẩu học, đồng ý nhận marketing, nguồn khách) để
  làm nền cho các chiến dịch chăm sóc khách hàng sau này.

### 3.2 Ngoài phạm vi (Out-of-Scope, giai đoạn hiện tại)

- Ứng dụng di động (mobile app) riêng — hệ thống hiện là web, responsive cho di động.
- Đa chi nhánh/đa đơn vị kinh doanh vận hành đồng thời — kiến trúc đã chuẩn bị sẵn cho việc này (khái niệm
  Site) nhưng nghiệp vụ hiện tại chỉ vận hành 1 cụm sân.
- Chương trình khách hàng thân thiết/tích điểm (loyalty) — dữ liệu nền (lịch sử booking) đã sẵn sàng nhưng
  cơ chế tích điểm chưa được yêu cầu triển khai.
- Xác thực khách hàng bằng OTP SMS — Phase 1 dùng email/số điện thoại + mật khẩu để tránh chi phí phát sinh
  từ nhà cung cấp SMS; có thể xem xét lại khi có nhu cầu.
- Chống trùng lịch giữa các chương trình đặt sân định kỳ chồng chéo phức tạp (hợp đồng thuê bao dài hạn) —
  Phase 1 chỉ hỗ trợ đặt định kỳ đơn giản (sinh nhiều lượt đặt lẻ theo lịch lặp).

---

## 4. Các bên liên quan (Stakeholders)

| Vai trò | Mô tả | Quan tâm chính |
|---|---|---|
| Chủ đầu tư / Chủ cụm sân | Người ra quyết định đầu tư, phê duyệt ngân sách | ROI, thời gian đưa sản phẩm ra thị trường |
| Quản lý vận hành (Admin) | Vận hành cụm sân hằng ngày, cấu hình giá/sân | Dễ sử dụng, kiểm soát được toàn bộ hoạt động |
| Lễ tân / Nhân viên trực sân | Xử lý booking, thu tiền, hỗ trợ khách tại quầy | Thao tác nhanh, ít bước, không nhầm lẫn |
| Khách hàng | Người đặt sân, có thể là khách vãng lai hoặc thành viên | Đặt sân nhanh, biết chắc còn slot trống, giá rõ ràng |
| Đội phát triển | Xây dựng và bảo trì hệ thống | Yêu cầu rõ ràng, kiến trúc nhất quán, dễ mở rộng |

---

## 5. Yêu cầu nghiệp vụ theo module

### 5.1 Quản lý sân <span class="tag tag-phase1">PHASE 1</span>

| Mã | Yêu cầu |
|---|---|
| BR-01 | Admin có thể tạo/sửa/xóa (mềm hoặc cứng) loại sân (VD: bóng đá mini, cầu lông) dùng chung cho toàn hệ thống. |
| BR-02 | Admin có thể tạo/sửa/xóa từng sân cụ thể, gắn với 1 loại sân và 1 cụm sân (Site); mỗi sân có trạng thái Hoạt động hoặc Đang bảo trì. |
| BR-03 | Admin có thể cấu hình giá theo khung giờ và theo ngày trong tuần cho từng sân (VD: giờ vàng buổi tối giá cao hơn giờ trưa). |
| BR-04 | Khách vãng lai và nhân viên đều có thể xem danh sách loại sân/sân/giá công khai, không cần đăng nhập. |

### 5.2 Đặt sân trực tuyến <span class="tag tag-phase1">PHASE 1</span> <i>(đợt triển khai kế tiếp)</i>

| Mã | Yêu cầu |
|---|---|
| BR-05 | Khách hàng có thể đặt sân theo khung giờ đơn lẻ, hoặc đặt định kỳ (chọn thứ trong tuần + khung giờ, lặp trong khoảng thời gian tự chọn). |
| BR-06 | Khách hàng có thể đặt sân mà không cần tạo tài khoản (đặt khách/guest, chỉ cần họ tên + số điện thoại); vẫn có tùy chọn đăng ký tài khoản để lưu lịch sử đặt sân. |
| BR-07 | Hệ thống phải đảm bảo không cho phép 2 lượt đặt trùng khung giờ trên cùng 1 sân — đây là yêu cầu bắt buộc, không có ngoại lệ. |
| BR-08 | Khi đặt định kỳ, nếu có buổi bị trùng lịch, hệ thống bỏ qua đúng buổi đó và báo cho khách biết buổi nào đặt thành công/thất bại, không hủy toàn bộ chuỗi đặt. |

### 5.3 Vận hành nội bộ (Web quản lý) <span class="tag tag-phase1">PHASE 1</span>

| Mã | Yêu cầu |
|---|---|
| BR-09 | Lễ tân xem được lịch đặt sân dạng lưới (sân × khung giờ) theo ngày để nắm tình trạng sân nhanh chóng. |
| BR-10 | Lễ tân có thể xác nhận, hủy booking, hoặc tạo booking hộ khách đến trực tiếp tại quầy (walk-in) mà không cần khách có tài khoản. |
| BR-11 | Lễ tân đánh dấu trạng thái thanh toán (đã thu tiền mặt) cho từng booking. |
| BR-12 | Admin có toàn quyền trên dữ liệu Lễ tân thao tác, cộng thêm quyền quản lý cấu hình sân/giá mà Lễ tân không có. |

### 5.4 Thanh toán <span class="tag tag-phase1">PHASE 1</span> <span class="tag tag-phase2">PHASE 2</span>

| Mã | Yêu cầu |
|---|---|
| BR-13 | Phase 1: hỗ trợ đánh dấu thanh toán tiền mặt thủ công bởi Lễ tân. |
| BR-14 | Phase 2: tích hợp cổng thanh toán online (VNPay/MoMo/ZaloPay) để khách thanh toán/đặt cọc ngay khi đặt sân online. |

### 5.5 Bán hàng tại quầy (POS) <span class="tag tag-phase2">PHASE 2</span>

| Mã | Yêu cầu |
|---|---|
| BR-15 | Lễ tân bán được đồ uống/phụ kiện thể thao kèm theo lượt đặt sân hoặc bán riêng lẻ. |
| BR-16 | Hệ thống quản lý tồn kho cơ bản cho hàng hóa bán tại quầy. |

### 5.6 Thống kê &amp; báo cáo <span class="tag tag-phase3">PHASE 3</span>

| Mã | Yêu cầu |
|---|---|
| BR-17 | Ban vận hành xem được báo cáo tỷ lệ lấp đầy sân, doanh thu theo loại sân/khung giờ qua Power BI. |
| BR-18 | Báo cáo tổng hợp hành vi khách hàng (tần suất đặt, chi tiêu) phục vụ ra quyết định marketing, dựa trên dữ liệu Booking đã có sẵn từ Phase 1 (không cần sửa lại mô hình dữ liệu gốc). |

---

## 6. Mô hình vận hành &amp; phân quyền

- **Admin**: toàn quyền cấu hình sân, loại sân, giá; quản lý toàn bộ booking và khách hàng.
- **Lễ tân**: xử lý booking hằng ngày (xác nhận, check-in, hủy, tạo hộ khách walk-in), không có quyền đổi
  cấu hình sân/giá.
- **Khách hàng (Customer)**: tài khoản độc lập với tài khoản nhân viên, có thể là khách vãng lai (không tài
  khoản) hoặc khách đã đăng ký (email/SĐT + mật khẩu).

Hệ thống kế thừa cơ chế **đăng nhập &amp; phân quyền theo Site** đã có sẵn trên nền BaseRepo — cụm sân hiện
tại được xem là 1 Site; nếu tổ chức mở thêm chi nhánh trong tương lai, chỉ cần khởi tạo thêm 1 Site mới mà
không phải thiết kế lại hệ thống.

---

## 7. Giả định &amp; ràng buộc (Assumptions &amp; Constraints)

- Hệ thống hiện chỉ phục vụ **1 cụm sân** — mô hình đa chi nhánh/đa khách hàng (SaaS) không nằm trong phạm vi
  giai đoạn này, dù kiến trúc đã hỗ trợ sẵn.
- Cổng thanh toán online cụ thể (VNPay/MoMo/ZaloPay — chọn 1 hay nhiều nhà cung cấp) sẽ được xác nhận lại
  trước khi triển khai Phase 2.
- Xác thực khách hàng dùng email/SĐT + mật khẩu (không dùng OTP SMS) để tránh phát sinh chi phí nhà cung cấp
  SMS ngoài phạm vi cần thiết ở giai đoạn này.
- Dự án được xây trên nền tảng dùng chung BaseRepo — mọi thay đổi ở tầng hạ tầng chung (đăng nhập, phân
  quyền) có thể ảnh hưởng tới các dự án khác đang chạy trên cùng nền tảng, cần phối hợp khi thay đổi.

---

## 8. Tiêu chí thành công (Success Criteria)

- Admin/Lễ tân có thể cấu hình và vận hành toàn bộ cụm sân qua web nội bộ mà không cần thao tác thủ công
  song song (sổ tay, Excel).
- Khách hàng đặt được sân trong vòng dưới 2 phút mà không cần tạo tài khoản.
- Không phát sinh booking trùng lịch do lỗi hệ thống.
- Tỷ lệ lấp đầy sân có thể đo lường được qua báo cáo (từ Phase 3).

---

## 9. Rủi ro chính (Key Risks)

| Rủi ro | Mức độ | Giải pháp giảm thiểu |
|---|---|---|
| Trùng lịch đặt sân do lỗi xử lý đồng thời (2 khách đặt cùng lúc) | Cao | Kiểm tra trùng lịch bắt buộc ở tầng xử lý nghiệp vụ trước khi ghi nhận booking |
| Nhân viên/khách hàng chưa quen giao diện mới, phát sinh thao tác sai | Trung bình | Đào tạo Lễ tân trước khi go-live, thiết kế luồng walk-in đơn giản |
| Phụ thuộc hạ tầng dùng chung (đăng nhập, phân quyền) với các dự án khác trên BaseRepo | Trung bình | Đội phát triển kiểm soát thay đổi ở tầng chung qua quy trình review riêng |
| Chưa seed dữ liệu phân quyền (Function) cho nhân viên sau khi triển khai | Trung bình | Có hướng dẫn vận hành cấp quyền cho Admin/Lễ tân ngay sau go-live |

---

## 10. Lộ trình triển khai (Roadmap)

<div class="diagram-wrap">
<svg viewBox="0 0 760 220" xmlns="http://www.w3.org/2000/svg">
  <line x1="60" y1="110" x2="720" y2="110" stroke="#0f4c81" stroke-width="3"/>
  <circle cx="140" cy="110" r="10" fill="#2f855a"/>
  <circle cx="400" cy="110" r="10" fill="#b7791f"/>
  <circle cx="660" cy="110" r="10" fill="#6b46c1"/>
  <rect x="60" y="30" width="160" height="55" rx="8" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="140" y="52" text-anchor="middle" font-size="12" font-weight="700" fill="#14532d">PHASE 1</text>
  <text x="140" y="68" text-anchor="middle" font-size="10" fill="#14532d">Quản lý sân + Booking</text>
  <text x="140" y="80" text-anchor="middle" font-size="10" fill="#14532d">+ Web nội bộ</text>
  <rect x="320" y="130" width="160" height="55" rx="8" fill="#fdf3e2" stroke="#b7791f" stroke-width="1.5"/>
  <text x="400" y="152" text-anchor="middle" font-size="12" font-weight="700" fill="#7a5000">PHASE 2</text>
  <text x="400" y="168" text-anchor="middle" font-size="10" fill="#7a5000">POS + Thanh toán</text>
  <text x="400" y="180" text-anchor="middle" font-size="10" fill="#7a5000">online</text>
  <rect x="580" y="30" width="160" height="55" rx="8" fill="#f1e9fb" stroke="#6b46c1" stroke-width="1.5"/>
  <text x="660" y="52" text-anchor="middle" font-size="12" font-weight="700" fill="#44337a">PHASE 3</text>
  <text x="660" y="68" text-anchor="middle" font-size="10" fill="#44337a">Thống kê +</text>
  <text x="660" y="80" text-anchor="middle" font-size="10" fill="#44337a">Power BI</text>
  <line x1="140" y1="85" x2="140" y2="100" stroke="#2f855a" stroke-width="1.5"/>
  <line x1="400" y1="120" x2="400" y2="130" stroke="#b7791f" stroke-width="1.5"/>
  <line x1="660" y1="85" x2="660" y2="100" stroke="#6b46c1" stroke-width="1.5"/>
</svg>
</div>
<div class="diagram-caption">Hình 1 — Lộ trình 3 giai đoạn triển khai dự án</div>

---

## 11. Phụ lục — Thuật ngữ (Glossary)

| Thuật ngữ | Giải thích |
|---|---|
| Site | Đơn vị/tenant trong kiến trúc BaseRepo — hiện tương ứng 1 cụm sân; có thể mở rộng thành nhiều chi nhánh sau này |
| FieldType | Loại hình thể thao (bóng đá, cầu lông...) |
| Field | Sân cụ thể, thuộc 1 Site và 1 FieldType |
| PricingRule | Quy tắc giá theo khung giờ + ngày trong tuần cho 1 sân |
| Booking | Một lượt đặt sân cụ thể (Phase 1, đợt kế tiếp) |
| RecurringGroup | Nhóm các lượt đặt sinh ra từ 1 lần đặt định kỳ |
| Walk-in | Khách đến trực tiếp quầy, được Lễ tân tạo booking hộ |
| RBAC | Role-Based Access Control — phân quyền theo vai trò (Admin/Lễ tân) |

<div class="footer-note">
BRD-BOOKING-001 · v1.0 · Tài liệu nội bộ — không phân phối ra ngoài tổ chức khi chưa được phê duyệt chính thức.
</div>
