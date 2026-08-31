# Booking Phase 1 — Thiết kế (Quản lý sân + Booking online + Web nội bộ cơ bản)

> Spec này là **Phase 1** của dự án "Quản lý thuê sân thể thao" (BRD tổng ở mức cao hơn sẽ tham chiếu spec
> này). Phase 2 (POS + thanh toán online) và Phase 3 (Thống kê + Power BI) sẽ có spec riêng, viết sau khi
> Phase 1 triển khai xong.

## 1. Bối cảnh & Mục tiêu

- Dự án mở rộng trên nền **BaseRepo** hiện có (.NET 10, Clean Architecture 5-layer, CQRS/MediatR, Dapper +
  Stored Procedure, YARP Gateway, RBAC theo Function) — không tách repo riêng.
- Mô hình kinh doanh: hiện tại chỉ **1 cụm sân duy nhất**, phục vụ **nhiều loại hình thể thao** (bóng đá
  mini, cầu lông, pickleball, tennis...). Hệ thống vẫn tận dụng khái niệm **Site** (đơn vị/tenant) đã có sẵn
  trong BaseRepo — cụm sân hiện tại là 1 Site; nếu sau này mở thêm chi nhánh, chỉ cần tạo thêm Site mới,
  không phải redesign kiến trúc.
- Phase 1 (MVP) gồm 3 mảng: **Quản lý sân**, **Booking online** (website công khai cho khách), **Web nội bộ
  cơ bản** (Admin + Lễ tân xử lý booking, tạo booking hộ khách tại quầy).
- **Ngoài phạm vi Phase 1** (để lại cho Phase 2/3, không thiết kế chi tiết ở đây): POS bán hàng hóa
  (đồ uống/phụ kiện) + tồn kho, tích hợp cổng thanh toán online (VNPay/MoMo/ZaloPay), thống kê/Power BI.
  Phase 1 chỉ cần trường `PaymentStatus` để Lễ tân đánh dấu thu tiền mặt thủ công.

## 2. Kiến trúc tổng quan

- Tạo **service mới `Booking`**, theo đúng khuôn 5-layer chuẩn của repo:
  `Booking/Booking.Api`, `Booking/Booking.Application`, `Booking/Booking.Contract`, `Booking/Booking.Domain`,
  `Booking/Booking.Infrastructure` — gom vào 1 folder cha `Booking/` như Gateway/Site/AdministrativeUnit.
- Entity `Field` mang `SiteId` (tham chiếu Site bên `Site` service qua Id, không join DB trực tiếp — đúng
  pattern cross-service hiện có, giống cách `Module`/`Role` gắn `SiteId`).
- Nhân viên (Admin/Lễ tân) dùng nguyên **Gateway User + Role scoped theo Site** đã có (`UserRole`,
  `Role_Function`) — tạo Role "Admin"/"Lễ tân" gán cho đúng Site của cụm sân, dùng
  `[RequireFunction("...")]` như các service khác. Không sửa gì ở Gateway/Site cho phần này.
- `Customer` (khách đặt sân) là entity **mới, độc lập**, nằm trong `Booking.Domain` — **không** dùng chung
  Gateway User (staff và khách hàng công khai là 2 khái niệm khác nhau, không nên trộn vào 1 hệ RBAC nội bộ).
- Route Gateway: tách **route đọc** (public, anonymous — xem sân trống, xem giá) và **route ghi** (cần auth)
  thành 2 route YARP riêng theo Methods, đúng pattern đã áp dụng cho `AdministrativeUnit`
  (`province-read-route`/`province-write-route`) — vì `AuthorizationMiddleware` bỏ qua xác thực hoàn toàn
  khi route không có `IAuthorizeData` metadata.

## 3. Domain Model (`Booking.Domain`)

### 3.1 Entity chính

| Entity | Vai trò | Field quan trọng |
|---|---|---|
| `FieldType` | Danh mục loại sân (bóng đá, cầu lông, pickleball, tennis...) — master data dùng chung | `FieldTypeId`, `FieldTypeName` |
| `Field` | Sân cụ thể thuộc 1 Site | `FieldId`, `SiteId`, `FieldTypeId`, `FieldName`, `Status` (Active/Maintenance/Inactive) |
| `PricingRule` | Giá theo khung giờ + ngày trong tuần (giờ vàng/thường, cuối tuần khác giá) | `FieldId`, `DayOfWeek` (1 giá trị enum duy nhất/dòng — muốn áp giá cho nhiều ngày thì tạo nhiều dòng `PricingRule`, không dùng bitmask), `StartTime`, `EndTime`, `PricePerHour` |
| `Customer` | Khách đặt sân, entity riêng biệt khỏi Gateway User | Xem chi tiết mục 3.2 |
| `Booking` | 1 lượt đặt sân cụ thể | `BookingId`, `FieldId`, `CustomerId` (null nếu guest chưa đăng ký), `GuestName`/`GuestPhone` (khi guest), `BookingDate`, `StartTime`, `EndTime`, `BookingStatus` (Pending/Confirmed/CheckedIn/Completed/Cancelled/NoShow — enum nghiệp vụ riêng, KHÔNG phải `BaseEntity.Status`; `BaseEntity.Status` trên `Booking` vẫn giữ đúng ý nghĩa soft-delete Active/Inactive như quy ước chung của repo), `TotalPrice`, `PaymentStatus` (Unpaid/Deposited/Paid), `RecurringGroupId` (null nếu đặt lẻ), `CreatedBy` (Customer tự đặt / Staff tạo hộ walk-in) |
| `RecurringGroup` | Nhãn nhóm cho chuỗi booking định kỳ — chỉ để trace/hủy cả chuỗi, **không** quản lý thanh toán gộp | `RecurringGroupId`, `FieldId`, `CustomerId`, `DayOfWeek`, `StartTime`, `EndTime`, `RangeStartDate`, `RangeEndDate`, `RecurringStatus` (Active/Cancelled — enum nghiệp vụ riêng, cùng lý do tách khỏi `BaseEntity.Status` như trên) |

Tất cả entity kế thừa `BaseEntity` (CommonLibs) như quy ước chung của repo — mọi field `{Xxx}Status` nghiệp
vụ nêu trên (`BookingStatus`, `RecurringStatus`) là property riêng của entity, tách biệt khỏi
`BaseEntity.Status` (`EntityStatus`, chỉ dùng cho soft-delete) để tránh nhầm lẫn khi implement.

### 3.2 Customer — chi tiết (phục vụ CRM sau này)

| Field | Ghi chú |
|---|---|
| `FullName`, `Phone`, `Email` | Bắt buộc tối thiểu để liên hệ |
| `PasswordHash` | Null nếu khách chưa từng đăng ký (guest thuần) |
| `DateOfBirth` | Segment theo độ tuổi, ưu đãi sinh nhật (tùy chọn) |
| `Gender` | Tùy chọn, không bắt buộc nhập |
| `Address` | Tùy chọn, phân tích khu vực khách hàng |
| `MarketingConsent` (bool) | Khách đồng ý nhận khuyến mãi/marketing hay không, mặc định `false` |
| `Source` | Kênh khách biết đến (Facebook/Google/Giới thiệu/Walk-in...) |
| `Note` | Ghi chú tự do của nhân viên |

**Cố tình không thêm** các field tổng hợp (`TotalBookings`, `TotalSpent`, `LastBookingDate`,
`LoyaltyPoints`) vào entity `Customer` — đây là số liệu tính được từ dữ liệu `Booking`, để tránh lệch dữ
liệu khi Booking bị hủy/sửa. Các chỉ số này sẽ được tổng hợp ở Phase 3 (Power BI) mà không cần sửa lại
Domain model của Phase 1.

### 3.3 Business logic quan trọng (không phải CRUD thuần)

- **Chống trùng lịch**: khi tạo `Booking` (lẻ hoặc từng bản ghi sinh từ `RecurringGroup`), Application layer
  bắt buộc kiểm tra overlap thời gian trên cùng `FieldId` + `BookingDate` trước khi insert. Đây là rule quan
  trọng nhất hệ thống.
- **Sinh Booking từ RecurringGroup**: khi khách đặt định kỳ, Application layer tạo 1 `RecurringGroup` rồi
  lặp sinh N bản ghi `Booking` con theo `DayOfWeek` trong khoảng `RangeStartDate`–`RangeEndDate`. Nếu 1 buổi
  bị trùng lịch, **bỏ qua buổi đó** (không insert) và trả về danh sách buổi tạo thành công/thất bại cho
  client hiển thị — không fail toàn bộ chuỗi.

## 4. Auth model cho Customer

- Customer đăng ký/đăng nhập bằng **Email/SDT + mật khẩu** (không dùng OTP SMS ở Phase 1 — tránh thêm
  dependency nhà cung cấp SMS ngoài phạm vi cần thiết).
- Đăng ký/đăng nhập tại **chính Booking service** (`CustomerAuthController` trong `Booking.Api`), tái dùng
  `IJwtTokenService`/`AppOptions` sẵn có trong CommonLibs (không viết lại JWT logic), hash password bằng
  `EncryptionUtils` như pattern hiện có.
- JWT Customer phát hành theo **scheme xác thực riêng** (khác `Bearer` của staff) để **không đi qua**
  `AuthorizationMiddleware` (middleware này chỉ xử lý RBAC cho staff, tra `tbl_User_Role`/`tbl_Role_Function`
  — không áp dụng cho Customer).
- Policy riêng `CustomerPolicy` cho API cần đăng nhập của khách (VD "xem lịch sử đặt sân của tôi") — check
  claim `CustomerId`, không dùng `Role`.
- Guest (không đăng nhập) vẫn gọi được API tạo Booking công khai bình thường, chỉ cần `GuestName` +
  `GuestPhone`, không qua auth.

## 5. Web nội bộ — luồng nghiệp vụ & phân quyền

- **Admin**: CRUD `FieldType`/`Field`/`PricingRule`, xem toàn bộ `Booking` + `Customer`, quyền hủy/sửa bất
  kỳ booking nào.
- **Lễ tân**: xem lịch đặt sân dạng lưới (Field × khung giờ theo ngày), xác nhận `Pending → Confirmed`,
  check-in (`CheckedIn`), **tạo booking hộ khách tại quầy** (walk-in — nhập thẳng `GuestName`/`GuestPhone`,
  không cần Customer đăng nhập), hủy booking, đánh dấu `PaymentStatus` khi thu tiền mặt. Không có quyền sửa
  `Field`/`PricingRule`.
- Dùng `[RequireFunction("...")]` đã có sẵn trong CommonLibs — VD `CreateField`, `UpdatePricingRule` (chỉ
  gán cho Role Admin), `ConfirmBooking`, `CreateWalkInBooking`, `CancelBooking` (gán cho cả Admin và Lễ
  tân). Không cần cơ chế phân quyền mới ngoài hệ đã có.

## 6. API Response & Error Handling

- Theo đúng chuẩn repo: mọi API trả `Response<T>`, exception nghiệp vụ dùng `AppException` với mã lỗi
  `[Booking_Repo_{Method}-{Number}]`.
- `GlobalExceptionMiddleware` xử lý exception toàn cục như các service khác, không tự bắt/nuốt exception.

## 7. Phạm vi test

- Test tập trung vào 2 logic nghiệp vụ thật sự phức tạp: **chống trùng lịch** và **sinh Booking từ
  RecurringGroup** (bao gồm case bỏ qua buổi trùng).
- Phần CRUD chuẩn (`FieldType`, `Field`, `PricingRule`, `Customer`) theo đúng checklist test hiện có của
  repo (`.claude/rules/06-testing.md`), không cần thiết kế riêng.

## 8. Ngoài phạm vi Phase 1 (nhắc lại)

- POS bán hàng hóa + quản lý tồn kho.
- Tích hợp cổng thanh toán online (VNPay/MoMo/ZaloPay) — Phase 1 chỉ có `PaymentStatus` đánh dấu thủ công.
- Thống kê, dashboard, tích hợp Power BI.
- BRD/FSR trang trọng cho stakeholder — viết riêng sau khi spec kỹ thuật này được duyệt, dựa trên nội dung
  đã thống nhất ở đây.
