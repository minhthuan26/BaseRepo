<div class="cover">
  <div class="doc-type">TECHNICAL DESIGN &amp; DIAGRAMS</div>
  <h1>Hệ thống Quản lý &amp; Đặt sân Thể thao</h1>
  <div class="subtitle">Kiến trúc hệ thống — Domain Model — Luồng nghiệp vụ</div>
  <table>
    <tr><td>Mã tài liệu</td><td>TDD-BOOKING-001</td></tr>
    <tr><td>Phiên bản</td><td>1.0</td></tr>
    <tr><td>Ngày ban hành</td><td>2026-09-01</td></tr>
    <tr><td>Nền tảng</td><td>BaseRepo — .NET 10, Clean Architecture, CQRS/MediatR, YARP Gateway</td></tr>
    <tr><td>Đối tượng đọc</td><td>Đội phát triển, Kiến trúc sư, DevOps</td></tr>
  </table>
</div>

## 1. Sơ đồ ngữ cảnh hệ thống (System Context)

<div class="diagram-wrap">
<svg viewBox="0 0 780 360" xmlns="http://www.w3.org/2000/svg">
  <rect x="300" y="140" width="180" height="80" rx="10" fill="#0f4c81"/>
  <text x="390" y="175" text-anchor="middle" font-size="14" font-weight="700" fill="white">Hệ thống Booking</text>
  <text x="390" y="195" text-anchor="middle" font-size="10" fill="#dbeafe">(BaseRepo Platform)</text>
  <circle cx="80" cy="80" r="28" fill="#2f855a"/>
  <text x="80" y="85" text-anchor="middle" font-size="12" font-weight="700" fill="white">KH</text>
  <text x="80" y="122" text-anchor="middle" font-size="11" font-weight="600">Khách hàng</text>
  <text x="80" y="135" text-anchor="middle" font-size="9" fill="#52606d">(guest / thành viên)</text>
  <circle cx="80" cy="280" r="28" fill="#2f855a"/>
  <text x="80" y="285" text-anchor="middle" font-size="12" font-weight="700" fill="white">LT</text>
  <text x="80" y="322" text-anchor="middle" font-size="11" font-weight="600">Lễ tân</text>
  <circle cx="700" cy="80" r="28" fill="#2f855a"/>
  <text x="700" y="85" text-anchor="middle" font-size="12" font-weight="700" fill="white">AD</text>
  <text x="700" y="122" text-anchor="middle" font-size="11" font-weight="600">Admin</text>
  <circle cx="700" cy="280" r="28" fill="#b7791f"/>
  <text x="700" y="285" text-anchor="middle" font-size="14" font-weight="700" fill="white">$</text>
  <text x="700" y="322" text-anchor="middle" font-size="11" font-weight="600">Cổng thanh toán</text>
  <text x="700" y="334" text-anchor="middle" font-size="9" fill="#52606d">VNPay/MoMo/ZaloPay (Phase 2)</text>
  <line x1="105" y1="95" x2="300" y2="155" stroke="#334155" stroke-width="1.5" marker-end="url(#arrow)"/>
  <line x1="105" y1="265" x2="300" y2="205" stroke="#334155" stroke-width="1.5" marker-end="url(#arrow)"/>
  <line x1="480" y1="155" x2="675" y2="95" stroke="#334155" stroke-width="1.5" marker-end="url(#arrow)"/>
  <line x1="480" y1="205" x2="675" y2="265" stroke="#334155" stroke-width="1.5" stroke-dasharray="5,3" marker-end="url(#arrow)"/>
  <text x="190" y="105" font-size="9" fill="#334155">Web Booking</text>
  <text x="190" y="245" font-size="9" fill="#334155">Web nội bộ</text>
  <text x="565" y="105" font-size="9" fill="#334155">Web nội bộ</text>
  <text x="555" y="255" font-size="9" fill="#334155">API thanh toán</text>
  <defs>
    <marker id="arrow" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#334155"/>
    </marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 1 — Sơ đồ ngữ cảnh hệ thống (System Context Diagram)</div>

---

## 2. Sơ đồ kiến trúc hệ thống (Component Architecture) — trạng thái hiện tại

<div class="diagram-wrap">
<svg viewBox="0 0 800 480" xmlns="http://www.w3.org/2000/svg">
  <rect x="20" y="20" width="760" height="60" rx="8" fill="#0f4c81"/>
  <text x="400" y="45" text-anchor="middle" font-size="13" font-weight="700" fill="white">Gateway (YARP Reverse Proxy) — port 82</text>
  <text x="400" y="63" text-anchor="middle" font-size="9.5" fill="#dbeafe">JWT Auth · RBAC (UserOrAdminPolicy + RequireFunction) · Swagger aggregation</text>
  <rect x="40" y="120" width="220" height="150" rx="8" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="150" y="145" text-anchor="middle" font-size="12" font-weight="700" fill="#14532d">Site Service — :8001</text>
  <text x="150" y="163" text-anchor="middle" font-size="9" fill="#14532d">Site / Module / Role</text>
  <text x="150" y="177" text-anchor="middle" font-size="9" fill="#14532d">Function / User / UserRole</text>
  <text x="150" y="191" text-anchor="middle" font-size="9" fill="#14532d">RoleFunction / Permission</text>
  <rect x="55" y="205" width="190" height="50" rx="6" fill="#c9e5d3"/>
  <text x="150" y="225" text-anchor="middle" font-size="9" font-weight="600" fill="#14532d">5-layer Clean Architecture</text>
  <text x="150" y="240" text-anchor="middle" font-size="8.5" fill="#14532d">Api / App / Contract / Domain / Infra</text>
  <rect x="290" y="120" width="220" height="150" rx="8" fill="#eef4fa" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="400" y="145" text-anchor="middle" font-size="12" font-weight="700" fill="#0f4c81">Booking Service — :8003</text>
  <text x="400" y="163" text-anchor="middle" font-size="9" fill="#0f4c81">FieldType / Field</text>
  <text x="400" y="177" text-anchor="middle" font-size="9" fill="#0f4c81">PricingRule</text>
  <text x="400" y="191" text-anchor="middle" font-size="9" fill="#7a1f1f" font-style="italic">Customer / Booking — kế tiếp</text>
  <rect x="305" y="205" width="190" height="50" rx="6" fill="#cfe0f5"/>
  <text x="400" y="225" text-anchor="middle" font-size="9" font-weight="600" fill="#0f4c81">5-layer Clean Architecture</text>
  <text x="400" y="240" text-anchor="middle" font-size="8.5" fill="#0f4c81">CQRS/MediatR · Dapper</text>
  <rect x="540" y="120" width="220" height="150" rx="8" fill="#f1e9fb" stroke="#6b46c1" stroke-width="1.5"/>
  <text x="650" y="145" text-anchor="middle" font-size="12" font-weight="700" fill="#44337a">AdministrativeUnit — :8002</text>
  <text x="650" y="163" text-anchor="middle" font-size="9" fill="#44337a">Province / Ward</text>
  <text x="650" y="185" text-anchor="middle" font-size="8.5" fill="#44337a">(dữ liệu hành chính dùng chung,</text>
  <text x="650" y="198" text-anchor="middle" font-size="8.5" fill="#44337a">ngoài phạm vi dự án Booking)</text>
  <rect x="150" y="330" width="200" height="60" rx="8" fill="#fdf3e2" stroke="#b7791f" stroke-width="1.5"/>
  <text x="250" y="355" text-anchor="middle" font-size="11" font-weight="700" fill="#7a5000">CommonLibs</text>
  <text x="250" y="372" text-anchor="middle" font-size="8.5" fill="#7a5000">BaseRepository · JWT · Redis · RabbitMQ</text>
  <rect x="410" y="330" width="140" height="60" rx="8" fill="#fde8e8" stroke="#c53030" stroke-width="1.5"/>
  <text x="480" y="358" text-anchor="middle" font-size="11" font-weight="700" fill="#742a2a">SQL Server</text>
  <text x="480" y="375" text-anchor="middle" font-size="8.5" fill="#742a2a">1 DB / service</text>
  <rect x="590" y="330" width="140" height="60" rx="8" fill="#fde8e8" stroke="#c53030" stroke-width="1.5"/>
  <text x="660" y="358" text-anchor="middle" font-size="11" font-weight="700" fill="#742a2a">Redis — :8012</text>
  <text x="660" y="375" text-anchor="middle" font-size="8.5" fill="#742a2a">Cache quyền User</text>
  <line x1="150" y1="80" x2="150" y2="120" stroke="#334155" stroke-width="1.5" marker-end="url(#arrow2)"/>
  <line x1="400" y1="80" x2="400" y2="120" stroke="#334155" stroke-width="1.5" marker-end="url(#arrow2)"/>
  <line x1="650" y1="80" x2="650" y2="120" stroke="#334155" stroke-width="1.5" marker-end="url(#arrow2)"/>
  <line x1="150" y1="270" x2="230" y2="330" stroke="#94a3b8" stroke-width="1.2" stroke-dasharray="4,3"/>
  <line x1="400" y1="270" x2="270" y2="330" stroke="#94a3b8" stroke-width="1.2" stroke-dasharray="4,3"/>
  <line x1="400" y1="270" x2="470" y2="330" stroke="#334155" stroke-width="1.5"/>
  <line x1="150" y1="270" x2="470" y2="330" stroke="#334155" stroke-width="1.2" opacity="0.5"/>
  <line x1="400" y1="270" x2="660" y2="330" stroke="#334155" stroke-width="1.2" opacity="0.5"/>
  <defs>
    <marker id="arrow2" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#334155"/>
    </marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 2 — Kiến trúc thành phần: Gateway điều phối tới 3 service, mỗi service có DB riêng, dùng chung CommonLibs + Redis cho cache phân quyền</div>

<blockquote>
Mỗi service tự xác thực JWT + RBAC độc lập ("defense in depth") — Gateway chặn trước, service phía sau
kiểm tra lại, không tin tưởng tuyệt đối vào lớp trước.
</blockquote>

---

## 3. Data Flow Diagram — Mức 0 (Context Diagram)

Ký hiệu dùng xuyên suốt 3 mức DFD (chuẩn Gane–Sarson): <b>hình chữ nhật góc vuông</b> = Đối tượng ngoài
(External Entity) — <b>hình chữ nhật bo góc</b> = Tiến trình (Process, có mã số) — <b>hình hở phải</b> = Kho
dữ liệu (Data Store) — <b>mũi tên</b> = Luồng dữ liệu (nét liền = Phase 1, nét đứt = Phase 2/3).

<div class="diagram-wrap">
<svg viewBox="0 0 800 480" xmlns="http://www.w3.org/2000/svg">
  <rect x="40" y="40" width="150" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="115" y="72" text-anchor="middle" font-size="11" font-weight="700" fill="#1f2933">Khách hàng</text>
  <rect x="610" y="40" width="150" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="685" y="72" text-anchor="middle" font-size="11" font-weight="700" fill="#1f2933">Admin</text>
  <rect x="40" y="385" width="150" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="115" y="417" text-anchor="middle" font-size="11" font-weight="700" fill="#1f2933">Lễ tân</text>
  <rect x="610" y="385" width="150" height="55" fill="#ffffff" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="685" y="410" text-anchor="middle" font-size="10" font-weight="700" fill="#7a5000">Cổng thanh toán</text>
  <text x="685" y="424" text-anchor="middle" font-size="8" fill="#7a5000">(Phase 2)</text>
  <rect x="280" y="185" width="240" height="100" rx="18" fill="#eef4fa" stroke="#0f4c81" stroke-width="2"/>
  <text x="400" y="222" text-anchor="middle" font-size="13" font-weight="700" fill="#0f4c81">0.0</text>
  <text x="400" y="242" text-anchor="middle" font-size="11" font-weight="700" fill="#0f4c81">Hệ thống Quản lý &amp;</text>
  <text x="400" y="258" text-anchor="middle" font-size="11" font-weight="700" fill="#0f4c81">Đặt sân Thể thao</text>
  <line x1="190" y1="70" x2="278" y2="205" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)"/>
  <text x="185" y="145" font-size="8.5" fill="#334155">Yêu cầu đặt sân,</text>
  <text x="185" y="156" font-size="8.5" fill="#334155">thông tin liên hệ</text>
  <line x1="278" y1="260" x2="192" y2="80" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)" opacity="0"/>
  <line x1="280" y1="255" x2="196" y2="90" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)"/>
  <text x="205" y="230" font-size="8.5" fill="#334155">Xác nhận booking,</text>
  <text x="205" y="241" font-size="8.5" fill="#334155">lịch sân trống</text>
  <line x1="610" y1="70" x2="522" y2="205" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)"/>
  <text x="500" y="145" font-size="8.5" fill="#334155">Cấu hình sân /</text>
  <text x="500" y="156" font-size="8.5" fill="#334155">loại sân / giá</text>
  <line x1="520" y1="255" x2="614" y2="90" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)"/>
  <text x="545" y="230" font-size="8.5" fill="#334155">Dữ liệu sân/giá,</text>
  <text x="545" y="241" font-size="8.5" fill="#334155">báo cáo</text>
  <line x1="190" y1="410" x2="285" y2="277" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)"/>
  <text x="188" y="345" font-size="8.5" fill="#334155">Yêu cầu xử lý</text>
  <text x="188" y="356" font-size="8.5" fill="#334155">booking, walk-in</text>
  <line x1="283" y1="283" x2="196" y2="400" stroke="#2f855a" stroke-width="1.5" marker-end="url(#dfa1)"/>
  <text x="205" y="380" font-size="8.5" fill="#334155">Danh sách booking,</text>
  <text x="205" y="391" font-size="8.5" fill="#334155">trạng thái sân</text>
  <line x1="610" y1="400" x2="520" y2="278" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3" marker-end="url(#dfa2)"/>
  <text x="500" y="345" font-size="8.5" fill="#7a5000">Kết quả</text>
  <text x="500" y="356" font-size="8.5" fill="#7a5000">thanh toán</text>
  <line x1="516" y1="283" x2="614" y2="393" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3" marker-end="url(#dfa2)"/>
  <text x="530" y="385" font-size="8.5" fill="#7a5000">Yêu cầu</text>
  <text x="530" y="396" font-size="8.5" fill="#7a5000">thanh toán</text>
  <defs>
    <marker id="dfa1" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#2f855a"/>
    </marker>
    <marker id="dfa2" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#b7791f"/>
    </marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 3 — DFD Mức 0: toàn hệ thống nhìn như 1 tiến trình duy nhất, trao đổi dữ liệu với 4 đối tượng ngoài</div>

---

## 4. Data Flow Diagram — Mức 1

Phân rã tiến trình 0.0 thành 6 tiến trình con, làm rõ luồng dữ liệu qua các kho dữ liệu chính.

<div class="diagram-wrap">
<svg viewBox="0 0 1000 800" xmlns="http://www.w3.org/2000/svg">
  <rect x="10" y="140" width="120" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="70" y="172" text-anchor="middle" font-size="10" font-weight="700">Khách hàng</text>
  <rect x="10" y="480" width="120" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="70" y="512" text-anchor="middle" font-size="10" font-weight="700">Lễ tân</text>
  <rect x="10" y="20" width="120" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="70" y="52" text-anchor="middle" font-size="10" font-weight="700">Admin</text>
  <rect x="860" y="480" width="130" height="55" fill="#ffffff" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="925" y="503" text-anchor="middle" font-size="9.5" font-weight="700" fill="#7a5000">Cổng thanh toán</text>
  <text x="925" y="517" text-anchor="middle" font-size="8" fill="#7a5000">(Phase 2)</text>
  <rect x="860" y="700" width="130" height="55" fill="#ffffff" stroke="#6b46c1" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="925" y="723" text-anchor="middle" font-size="9.5" font-weight="700" fill="#44337a">Power BI</text>
  <text x="925" y="737" text-anchor="middle" font-size="8" fill="#44337a">(Phase 3)</text>
  <rect x="220" y="20" width="180" height="55" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="310" y="42" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">1.0 Quản lý</text>
  <text x="310" y="58" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">Sân &amp; Giá</text>
  <rect x="220" y="150" width="180" height="65" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="2"/>
  <text x="310" y="175" text-anchor="middle" font-size="11" font-weight="700" fill="#14532d">2.0 Quản lý</text>
  <text x="310" y="192" text-anchor="middle" font-size="11" font-weight="700" fill="#14532d">Đặt sân</text>
  <text x="310" y="206" text-anchor="middle" font-size="8" fill="#14532d" font-style="italic">(xem chi tiết Hình 5)</text>
  <rect x="220" y="290" width="180" height="55" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="310" y="312" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">3.0 Quản lý</text>
  <text x="310" y="328" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">Khách hàng</text>
  <rect x="220" y="450" width="180" height="55" rx="14" fill="#fdf3e2" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="310" y="470" text-anchor="middle" font-size="10" font-weight="700" fill="#7a5000">4.0 Xử lý</text>
  <text x="310" y="486" text-anchor="middle" font-size="10" font-weight="700" fill="#7a5000">Thanh toán</text>
  <text x="310" y="498" text-anchor="middle" font-size="7.5" fill="#7a5000">(Phase 2)</text>
  <rect x="220" y="580" width="180" height="55" rx="14" fill="#fdf3e2" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="310" y="600" text-anchor="middle" font-size="10" font-weight="700" fill="#7a5000">5.0 Bán hàng</text>
  <text x="310" y="616" text-anchor="middle" font-size="10" font-weight="700" fill="#7a5000">POS</text>
  <text x="310" y="628" text-anchor="middle" font-size="7.5" fill="#7a5000">(Phase 2)</text>
  <rect x="220" y="700" width="180" height="55" rx="14" fill="#f1e9fb" stroke="#6b46c1" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="310" y="720" text-anchor="middle" font-size="10" font-weight="700" fill="#44337a">6.0 Báo cáo</text>
  <text x="310" y="736" text-anchor="middle" font-size="10" font-weight="700" fill="#44337a">Thống kê</text>
  <text x="310" y="748" text-anchor="middle" font-size="7.5" fill="#44337a">(Phase 3)</text>
  <path d="M540,20 h140 v55 h-140 M540,35 h140" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="600" y="32" font-size="9" font-weight="700" fill="#0f4c81">D1</text>
  <text x="655" y="32" font-size="9" font-weight="700" fill="#0f4c81">FieldType</text>
  <path d="M540,90 h140 v55 h-140 M540,105 h140" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="600" y="102" font-size="9" font-weight="700" fill="#0f4c81">D2</text>
  <text x="655" y="102" font-size="9" font-weight="700" fill="#0f4c81">Field</text>
  <path d="M540,160 h140 v55 h-140 M540,175 h140" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="600" y="172" font-size="9" font-weight="700" fill="#0f4c81">D3</text>
  <text x="655" y="172" font-size="9" font-weight="700" fill="#0f4c81">PricingRule</text>
  <path d="M540,290 h140 v55 h-140 M540,305 h140" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="600" y="302" font-size="9" font-weight="700" fill="#0f4c81">D4</text>
  <text x="655" y="302" font-size="9" font-weight="700" fill="#0f4c81">Customer</text>
  <path d="M540,360 h140 v55 h-140 M540,375 h140" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="600" y="372" font-size="9" font-weight="700" fill="#0f4c81">D5</text>
  <text x="655" y="372" font-size="9" font-weight="700" fill="#0f4c81">Booking</text>
  <path d="M540,430 h140 v55 h-140 M540,445 h140" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="600" y="442" font-size="9" font-weight="700" fill="#0f4c81">D6</text>
  <text x="655" y="442" font-size="9" font-weight="700" fill="#0f4c81">RecurringGroup</text>
  <path d="M540,570 h140 v55 h-140 M540,585 h140" fill="none" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="600" y="582" font-size="9" font-weight="700" fill="#7a5000">D7</text>
  <text x="655" y="582" font-size="9" font-weight="700" fill="#7a5000">Payment</text>
  <path d="M540,640 h140 v55 h-140 M540,655 h140" fill="none" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="600" y="652" font-size="9" font-weight="700" fill="#7a5000">D8</text>
  <text x="655" y="652" font-size="9" font-weight="700" fill="#7a5000">Product</text>
  <line x1="130" y1="45" x2="220" y2="45" stroke="#2f855a" stroke-width="1.4" marker-end="url(#dfb)"/>
  <line x1="130" y1="167" x2="220" y2="180" stroke="#2f855a" stroke-width="1.4" marker-end="url(#dfb)"/>
  <line x1="130" y1="500" x2="220" y2="470" stroke="#2f855a" stroke-width="1.4" marker-end="url(#dfb)"/>
  <line x1="220" y1="330" x2="130" y2="200" stroke="#2f855a" stroke-width="1.4" marker-end="url(#dfb)"/>
  <text x="128" y="240" font-size="8" fill="#334155">Đăng ký / đăng nhập</text>
  <line x1="400" y1="45" x2="536" y2="45" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="60" x2="536" y2="115" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="45" x2="536" y2="180" stroke="#0f4c81" stroke-width="1.1" opacity="0.6" marker-end="url(#dfc)"/>
  <line x1="400" y1="182" x2="536" y2="115" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="190" x2="536" y2="185" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="198" x2="536" y2="315" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="205" x2="536" y2="385" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="210" x2="536" y2="455" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="400" y1="315" x2="536" y2="315" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dfc)"/>
  <line x1="310" y1="215" x2="310" y2="450" stroke="#b7791f" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dfe)"/>
  <text x="315" y="440" font-size="8" fill="#7a5000">Yêu cầu thanh toán</text>
  <line x1="400" y1="475" x2="856" y2="500" stroke="#b7791f" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dfe)"/>
  <line x1="856" y1="520" x2="400" y2="490" stroke="#b7791f" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dfe)"/>
  <line x1="400" y1="470" x2="536" y2="590" stroke="#b7791f" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dfe)"/>
  <line x1="400" y1="605" x2="536" y2="660" stroke="#b7791f" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dfe)"/>
  <line x1="220" y1="610" x2="130" y2="525" stroke="#b7791f" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dfe)"/>
  <line x1="540" y1="400" x2="400" y2="720" stroke="#6b46c1" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#dff)"/>
  <line x1="540" y1="320" x2="400" y2="730" stroke="#6b46c1" stroke-width="1.1" stroke-dasharray="4,3" opacity="0.7" marker-end="url(#dff)"/>
  <line x1="540" y1="600" x2="400" y2="710" stroke="#6b46c1" stroke-width="1.1" stroke-dasharray="4,3" opacity="0.7" marker-end="url(#dff)"/>
  <line x1="400" y1="725" x2="860" y2="725" stroke="#6b46c1" stroke-width="1.3" stroke-dasharray="4,3" marker-end="url(#dff)"/>
  <defs>
    <marker id="dfb" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#2f855a"/></marker>
    <marker id="dfc" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#0f4c81"/></marker>
    <marker id="dfe" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#b7791f"/></marker>
    <marker id="dff" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#6b46c1"/></marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 4 — DFD Mức 1: 6 tiến trình chính và 8 kho dữ liệu. Tiến trình 2.0 (Quản lý Đặt sân) là trọng tâm, được phân rã tiếp ở Hình 5</div>

<blockquote>
Đường nét liền màu xanh lá/xanh dương = luồng dữ liệu đã/sẽ triển khai ở Phase 1. Đường nét đứt màu vàng =
Phase 2. Đường nét đứt màu tím = Phase 3. Không phải mọi luồng đều được gắn nhãn để tránh rối hình — nhãn chỉ
gắn cho các luồng quan trọng nhất; xem Mục 5.2 (FSR) để biết đầy đủ dữ liệu trao đổi cho từng tiến trình.
</blockquote>

---

## 5. Data Flow Diagram — Mức 2 (chi tiết tiến trình 2.0 Quản lý Đặt sân)

Phân rã tiếp tiến trình <b>2.0</b> — nơi chứa ràng buộc nghiệp vụ quan trọng nhất hệ thống
(<b>chống trùng lịch</b>).

<div class="diagram-wrap">
<svg viewBox="0 0 900 620" xmlns="http://www.w3.org/2000/svg">
  <rect x="10" y="30" width="120" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="70" y="62" text-anchor="middle" font-size="10" font-weight="700">Khách hàng</text>
  <rect x="10" y="480" width="120" height="55" fill="#ffffff" stroke="#334155" stroke-width="1.5"/>
  <text x="70" y="512" text-anchor="middle" font-size="10" font-weight="700">Lễ tân</text>
  <rect x="330" y="20" width="170" height="55" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="415" y="42" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">2.1 Kiểm tra</text>
  <text x="415" y="58" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">lịch trống</text>
  <rect x="330" y="260" width="170" height="65" rx="14" fill="#fde8e8" stroke="#c53030" stroke-width="2"/>
  <text x="415" y="284" text-anchor="middle" font-size="10.5" font-weight="700" fill="#742a2a">2.2 Kiểm tra</text>
  <text x="415" y="300" text-anchor="middle" font-size="10.5" font-weight="700" fill="#742a2a">TRÙNG LỊCH</text>
  <text x="415" y="314" text-anchor="middle" font-size="7.5" fill="#742a2a">(ràng buộc bắt buộc)</text>
  <rect x="130" y="150" width="170" height="55" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="215" y="172" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">2.3 Tạo Booking</text>
  <text x="215" y="188" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">đơn lẻ</text>
  <rect x="330" y="400" width="170" height="55" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="415" y="422" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">2.4 Sinh chuỗi</text>
  <text x="415" y="438" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">Booking định kỳ</text>
  <rect x="560" y="150" width="170" height="65" rx="14" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="645" y="172" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">2.5 Xử lý booking</text>
  <text x="645" y="188" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">nội bộ (xác nhận/</text>
  <text x="645" y="202" text-anchor="middle" font-size="10.5" font-weight="700" fill="#14532d">hủy/walk-in)</text>
  <path d="M690,20 h150 v55 h-150 M690,35 h150" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="700" y="32" font-size="9" font-weight="700" fill="#0f4c81">D2</text>
  <text x="700" y="60" font-size="8.5" fill="#0f4c81">Field / D3 PricingRule</text>
  <path d="M50,260 h150 v55 h-150 M50,275 h150" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="60" y="272" font-size="9" font-weight="700" fill="#0f4c81">D5</text>
  <text x="115" y="272" font-size="9" font-weight="700" fill="#0f4c81">Booking</text>
  <path d="M700,400 h180 v55 h-180 M700,415 h180" fill="none" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="710" y="412" font-size="9" font-weight="700" fill="#0f4c81">D6</text>
  <text x="765" y="412" font-size="9" font-weight="700" fill="#0f4c81">RecurringGroup</text>
  <line x1="130" y1="55" x2="330" y2="45" stroke="#2f855a" stroke-width="1.4" marker-end="url(#dg1)"/>
  <text x="150" y="42" font-size="8" fill="#334155">Xem lịch trống</text>
  <line x1="500" y1="45" x2="690" y2="45" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dg2)"/>
  <line x1="415" y1="75" x2="415" y2="260" stroke="#2f855a" stroke-width="1.3" marker-end="url(#dg1)"/>
  <text x="420" y="160" font-size="8" fill="#334155">Yêu cầu đặt (lẻ)</text>
  <line x1="10" y1="60" x2="10" y2="177" stroke="#94a3b8" stroke-width="0" />
  <line x1="20" y1="70" x2="200" y2="150" stroke="#2f855a" stroke-width="1.3" marker-end="url(#dg1)"/>
  <line x1="330" y1="290" x2="200" y2="205" stroke="#c53030" stroke-width="1.3" marker-end="url(#dg3)"/>
  <text x="230" y="255" font-size="8" fill="#742a2a">Kết quả kiểm tra</text>
  <line x1="330" y1="300" x2="415" y2="400" stroke="#c53030" stroke-width="1.3" marker-end="url(#dg3)"/>
  <line x1="215" y1="205" x2="130" y2="270" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dg2)"/>
  <text x="120" y="240" font-size="8" fill="#334155">Ghi Booking</text>
  <line x1="415" y1="400" x2="700" y2="425" stroke="#0f4c81" stroke-width="1.3" marker-end="url(#dg2)"/>
  <line x1="415" y1="400" x2="330" y2="295" stroke="#c53030" stroke-width="1.2" stroke-dasharray="3,2" marker-end="url(#dg3)"/>
  <text x="270" y="365" font-size="8" fill="#742a2a">Kiểm tra từng buổi</text>
  <line x1="70" y1="480" x2="415" y2="325" stroke="#2f855a" stroke-width="1.3" marker-end="url(#dg1)"/>
  <text x="90" y="440" font-size="8" fill="#334155">Xác nhận / hủy / walk-in</text>
  <line x1="560" y1="180" x2="500" y2="288" stroke="#c53030" stroke-width="1.3" marker-end="url(#dg3)"/>
  <text x="490" y="230" font-size="8" fill="#742a2a">Kiểm tra trước khi tạo hộ</text>
  <line x1="560" y1="200" x2="200" y2="290" stroke="#0f4c81" stroke-width="1.1" opacity="0.8" marker-end="url(#dg2)"/>
  <line x1="130" y1="530" x2="415" y2="465" stroke="#2f855a" stroke-width="1.3" marker-end="url(#dg1)"/>
  <text x="150" y="520" font-size="8" fill="#334155">Yêu cầu đặt định kỳ</text>
  <defs>
    <marker id="dg1" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#2f855a"/></marker>
    <marker id="dg2" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#0f4c81"/></marker>
    <marker id="dg3" markerWidth="9" markerHeight="9" refX="7" refY="3" orient="auto" markerUnits="strokeWidth"><path d="M0,0 L0,6 L8,3 z" fill="#c53030"/></marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 5 — DFD Mức 2: phân rã tiến trình Đặt sân. Tiến trình 2.2 (đỏ) là điểm nghẽn bắt buộc mọi luồng ghi Booking (đặt lẻ, đặt định kỳ, walk-in) đều phải đi qua trước khi ghi vào D5</div>

<blockquote>
Vì sao 2.2 được tách thành 1 tiến trình riêng thay vì gộp vào 2.3/2.4/2.5: đây là <b>ràng buộc nghiệp vụ duy
nhất không có ngoại lệ</b> trong toàn hệ thống — kể cả Lễ tân tạo booking hộ khách (walk-in) cũng phải đi qua
đúng bước kiểm tra này, không có đường tắt dành riêng cho nhân viên.
</blockquote>

---

## 6. Mô hình dữ liệu (Domain / ER Model) — Phase 1

<div class="diagram-wrap">
<svg viewBox="0 0 780 380" xmlns="http://www.w3.org/2000/svg">
  <rect x="20" y="30" width="170" height="90" rx="8" fill="#fdf3e2" stroke="#b7791f" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="105" y="52" text-anchor="middle" font-size="11" font-weight="700" fill="#7a5000">Site</text>
  <text x="105" y="68" text-anchor="middle" font-size="8.5" fill="#7a5000">(Site service — ngoài)</text>
  <text x="105" y="82" text-anchor="middle" font-size="8.5" fill="#7a5000">SiteId (PK)</text>
  <text x="105" y="95" text-anchor="middle" font-size="8.5" fill="#7a5000">SiteName</text>
  <text x="105" y="108" text-anchor="middle" font-size="8" fill="#a06a1e" font-style="italic">tham chiếu qua Id, không FK vật lý</text>
  <rect x="320" y="30" width="200" height="130" rx="8" fill="#eef4fa" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="420" y="52" text-anchor="middle" font-size="12" font-weight="700" fill="#0f4c81">FieldType</text>
  <line x1="330" y1="60" x2="510" y2="60" stroke="#0f4c81"/>
  <text x="335" y="76" font-size="9" fill="#0f4c81">FieldTypeId (PK)</text>
  <text x="335" y="90" font-size="9" fill="#0f4c81">FieldTypeName</text>
  <text x="335" y="104" font-size="9" fill="#0f4c81">Status</text>
  <text x="335" y="120" font-size="8" fill="#52606d" font-style="italic">Danh mục dùng chung</text>
  <text x="335" y="132" font-size="8" fill="#52606d" font-style="italic">toàn hệ thống</text>
  <text x="335" y="146" font-size="8" fill="#52606d" font-style="italic">(không thuộc Site nào)</text>
  <rect x="290" y="220" width="220" height="140" rx="8" fill="#eef4fa" stroke="#0f4c81" stroke-width="1.5"/>
  <text x="400" y="242" text-anchor="middle" font-size="12" font-weight="700" fill="#0f4c81">Field</text>
  <line x1="300" y1="250" x2="500" y2="250" stroke="#0f4c81"/>
  <text x="305" y="266" font-size="9" fill="#0f4c81">FieldId (PK)</text>
  <text x="305" y="280" font-size="9" fill="#0f4c81">SiteId (ref → Site)</text>
  <text x="305" y="294" font-size="9" fill="#0f4c81">FieldTypeId (FK)</text>
  <text x="305" y="308" font-size="9" fill="#0f4c81">FieldName</text>
  <text x="305" y="322" font-size="9" fill="#0f4c81">OperationalStatus</text>
  <text x="305" y="336" font-size="8" fill="#52606d" font-style="italic">(Active / Maintenance)</text>
  <text x="305" y="350" font-size="9" fill="#0f4c81">Status <tspan font-size="7.5" fill="#8896a6">(soft-delete)</tspan></text>
  <rect x="580" y="220" width="190" height="120" rx="8" fill="#e6f4ea" stroke="#2f855a" stroke-width="1.5"/>
  <text x="675" y="242" text-anchor="middle" font-size="12" font-weight="700" fill="#14532d">PricingRule</text>
  <line x1="590" y1="250" x2="760" y2="250" stroke="#2f855a"/>
  <text x="595" y="266" font-size="9" fill="#14532d">AutoId (PK)</text>
  <text x="595" y="280" font-size="9" fill="#14532d">FieldId (FK)</text>
  <text x="595" y="294" font-size="9" fill="#14532d">DayOfWeek</text>
  <text x="595" y="308" font-size="9" fill="#14532d">StartTime / EndTime</text>
  <text x="595" y="322" font-size="9" fill="#14532d">PricePerHour</text>
  <rect x="20" y="220" width="200" height="80" rx="8" fill="#f4f4f5" stroke="#a1a1aa" stroke-width="1.5" stroke-dasharray="5,3"/>
  <text x="120" y="242" text-anchor="middle" font-size="11" font-weight="700" fill="#52525b">Customer</text>
  <text x="120" y="258" text-anchor="middle" font-size="8.5" fill="#52525b">(FullName, Phone, Email...)</text>
  <text x="120" y="272" text-anchor="middle" font-size="8" fill="#71717a" font-style="italic">Kế tiếp — chưa triển khai</text>
  <text x="120" y="286" text-anchor="middle" font-size="8" fill="#71717a" font-style="italic">ở đợt build này</text>
  <line x1="105" y1="120" x2="330" y2="270" stroke="#94a3b8" stroke-width="1.3" stroke-dasharray="4,3"/>
  <text x="150" y="185" font-size="8.5" fill="#52606d">1 Site — N Field</text>
  <line x1="420" y1="160" x2="400" y2="220" stroke="#0f4c81" stroke-width="1.5" marker-end="url(#arrow3)"/>
  <text x="428" y="192" font-size="8.5" fill="#334155">1 FieldType — N Field</text>
  <line x1="510" y1="290" x2="580" y2="290" stroke="#2f855a" stroke-width="1.5" marker-end="url(#arrow3)"/>
  <text x="512" y="282" font-size="8.5" fill="#334155">1 Field — N PricingRule</text>
  <defs>
    <marker id="arrow3" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#334155"/>
    </marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 6 — Mô hình dữ liệu Phase 1: FieldType (danh mục chung) → Field (thuộc 1 Site) → PricingRule (giá theo khung giờ); Customer là bước kế tiếp trên cùng service</div>

---

## 7. Luồng nghiệp vụ: Admin tạo mới 1 sân (đã triển khai)

<div class="diagram-wrap">
<svg viewBox="0 0 780 380" xmlns="http://www.w3.org/2000/svg">
  <text x="90"  y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">Admin</text>
  <text x="270" y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">Gateway</text>
  <text x="470" y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">Booking.Api</text>
  <text x="670" y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">SQL Server</text>
  <line x1="90"  y1="34" x2="90"  y2="360" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="270" y1="34" x2="270" y2="360" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="470" y1="34" x2="470" y2="360" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="670" y1="34" x2="670" y2="360" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="90" y1="60" x2="266" y2="60" stroke="#0f4c81" stroke-width="1.5" marker-end="url(#arrow4)"/>
  <text x="95" y="54" font-size="8.5" fill="#334155">1. POST /api/field + Bearer token</text>
  <rect x="240" y="70" width="60" height="34" fill="#eef4fa" stroke="#0f4c81"/>
  <text x="270" y="90" text-anchor="middle" font-size="7.5" fill="#0f4c81">Xác thực</text>
  <text x="270" y="100" text-anchor="middle" font-size="7.5" fill="#0f4c81">JWT + RBAC</text>
  <line x1="270" y1="120" x2="466" y2="120" stroke="#0f4c81" stroke-width="1.5" marker-end="url(#arrow4)"/>
  <text x="278" y="114" font-size="8.5" fill="#334155">2. Forward (đã qua UserOrAdminPolicy)</text>
  <rect x="440" y="130" width="60" height="34" fill="#eef4fa" stroke="#0f4c81"/>
  <text x="470" y="150" text-anchor="middle" font-size="7.5" fill="#0f4c81">RequireFunction</text>
  <text x="470" y="160" text-anchor="middle" font-size="7.5" fill="#0f4c81">CreateField</text>
  <line x1="470" y1="180" x2="470" y2="200" stroke="#94a3b8" stroke-width="1.3"/>
  <text x="478" y="195" font-size="8" fill="#52606d" font-style="italic">3. SiteId lấy từ claim site_id</text>
  <text x="478" y="206" font-size="8" fill="#52606d" font-style="italic">(KHÔNG tin theo body client gửi)</text>
  <line x1="470" y1="220" x2="666" y2="220" stroke="#0f4c81" stroke-width="1.5" marker-end="url(#arrow4)"/>
  <text x="475" y="214" font-size="8.5" fill="#334155">4. Kiểm tra FieldTypeId tồn tại + INSERT Field</text>
  <line x1="670" y1="250" x2="474" y2="250" stroke="#2f855a" stroke-width="1.5" stroke-dasharray="4,3" marker-end="url(#arrow4)"/>
  <text x="500" y="244" font-size="8.5" fill="#334155">5. OK</text>
  <line x1="470" y1="280" x2="274" y2="280" stroke="#2f855a" stroke-width="1.5" stroke-dasharray="4,3" marker-end="url(#arrow4)"/>
  <text x="290" y="274" font-size="8.5" fill="#334155">6. Response&lt;FieldOutputDTO&gt;</text>
  <line x1="270" y1="310" x2="94" y2="310" stroke="#2f855a" stroke-width="1.5" stroke-dasharray="4,3" marker-end="url(#arrow4)"/>
  <text x="100" y="304" font-size="8.5" fill="#334155">7. 200 OK</text>
  <defs>
    <marker id="arrow4" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#334155"/>
    </marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 7 — Sequence diagram: Admin tạo sân mới, minh họa cơ chế SiteId luôn lấy từ JWT claim, không tin theo dữ liệu client gửi</div>

---

## 8. Luồng nghiệp vụ: Khách hàng đặt sân <span class="tag tag-phase1">Phần kế tiếp trên nền đã dựng</span>

<div class="diagram-wrap">
<svg viewBox="0 0 780 340" xmlns="http://www.w3.org/2000/svg">
  <text x="90"  y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">Khách hàng</text>
  <text x="270" y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">Gateway</text>
  <text x="470" y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">Booking.Api</text>
  <text x="670" y="24" text-anchor="middle" font-size="10" font-weight="700" fill="#334155">SQL Server</text>
  <line x1="90"  y1="34" x2="90"  y2="320" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="270" y1="34" x2="270" y2="320" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="470" y1="34" x2="470" y2="320" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="670" y1="34" x2="670" y2="320" stroke="#cbd5e1" stroke-width="1.5"/>
  <line x1="90" y1="60" x2="266" y2="60" stroke="#b7791f" stroke-width="1.5" marker-end="url(#arrow5)"/>
  <text x="95" y="54" font-size="8.5" fill="#334155">1. GET /api/field/availability (không cần đăng nhập)</text>
  <line x1="270" y1="90" x2="466" y2="90" stroke="#b7791f" stroke-width="1.5" marker-end="url(#arrow5)"/>
  <text x="278" y="84" font-size="8.5" fill="#334155">2. Route công khai — không qua kiểm tra JWT</text>
  <line x1="90" y1="140" x2="266" y2="140" stroke="#b7791f" stroke-width="1.5" marker-end="url(#arrow5)"/>
  <text x="95" y="134" font-size="8.5" fill="#334155">3. POST /api/booking (guest: họ tên + SĐT, hoặc kèm token Customer)</text>
  <rect x="440" y="150" width="60" height="34" fill="#fdf3e2" stroke="#b7791f"/>
  <text x="470" y="168" text-anchor="middle" font-size="7.5" fill="#7a5000">Kiểm tra</text>
  <text x="470" y="178" text-anchor="middle" font-size="7.5" fill="#7a5000">trùng lịch</text>
  <line x1="470" y1="200" x2="666" y2="200" stroke="#b7791f" stroke-width="1.5" marker-end="url(#arrow5)"/>
  <text x="475" y="194" font-size="8.5" fill="#334155">4. Còn trống → INSERT Booking</text>
  <line x1="670" y1="230" x2="474" y2="230" stroke="#2f855a" stroke-width="1.5" stroke-dasharray="4,3" marker-end="url(#arrow5)"/>
  <text x="500" y="224" font-size="8.5" fill="#334155">5. OK</text>
  <line x1="470" y1="260" x2="94" y2="260" stroke="#2f855a" stroke-width="1.5" stroke-dasharray="4,3" marker-end="url(#arrow5)"/>
  <text x="120" y="254" font-size="8.5" fill="#334155">6. Xác nhận đặt sân thành công</text>
  <defs>
    <marker id="arrow5" markerWidth="10" markerHeight="10" refX="8" refY="3" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L0,6 L9,3 z" fill="#334155"/>
    </marker>
  </defs>
</svg>
</div>
<div class="diagram-caption">Hình 8 — Sequence diagram (mục tiêu thiết kế): luồng khách đặt sân, bước "Kiểm tra trùng lịch" là ràng buộc nghiệp vụ quan trọng nhất hệ thống</div>

<blockquote>
Hình 8 mô tả <b>mục tiêu thiết kế</b> đã thống nhất trong tài liệu đặc tả kỹ thuật (spec) — thực thể
<code>Customer</code>/<code>Booking</code> và logic chống trùng lịch (đã thấy ở mức tiến trình tại Hình 5) sẽ
được hiện thực ở đợt phát triển kế tiếp trên nền service <code>Booking</code> vừa dựng xong ở Hình 2, 6–7.
</blockquote>

---

## 9. Tham chiếu

| Tài liệu | Vị trí |
|---|---|
| Đặc tả kỹ thuật chi tiết (FSR) | `docs/superpowers/specs/2026-09-01-booking-phase1-design.md` |
| Kế hoạch triển khai (Implementation Plan) | `docs/superpowers/plans/2026-09-01-booking-phase1-foundation-plan.md` |
| Schema cơ sở dữ liệu | `CommonLibs/BaseSQL/booking/BaseBooking.sql` |
| Quy tắc kiến trúc chung | `.claude/rules/01-architecture.md` |

<div class="footer-note">
TDD-BOOKING-001 · v1.0 · Tài liệu kỹ thuật nội bộ dành cho đội phát triển.
</div>
