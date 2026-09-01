# Tái cấu trúc thư mục repo theo nhóm domain — Thiết kế

> Spec này KHÔNG đổi kiến trúc 5-layer nội bộ từng service (Api/Application/Contract/Domain/Infrastructure) —
> chỉ thêm 1 cấp thư mục nhóm bên trên để phân biệt service nào thuộc "nền tảng" (Base) và service nào thuộc
> dự án domain nào (SportsBooking, BloodDonation...). Không đổi tên project/namespace C# nào.

## 1. Bối cảnh & Mục tiêu

BaseRepo hiện là monorepo phẳng: `Gateway/`, `Site/`, `AdministrativeUnit/`, `Booking/` đều nằm ngang hàng ở gốc
repo, cùng dùng chung `CommonLibs/`. Vấn đề: khi số lượng dự án domain tăng lên (Booking cho "Quản lý thuê sân",
sắp tới thêm dự án "Đăng ký hiến máu"), nhìn vào cây thư mục không phân biệt được service nào là hạ tầng nền
tảng dùng chung (auth, RBAC, danh mục hành chính) và service nào thuộc riêng dự án nghiệp vụ nào.

Đã brainstorm và cân nhắc 3 hướng tổ chức repo (tách repo riêng / đổi hẳn repo hiện tại / giữ monorepo) — chốt
giữ monorepo (lý do: `CommonLibs` vẫn đang thay đổi thường xuyên, tách package lúc này tạo ma sát lớn cho mỗi
thay đổi nhỏ; xem thảo luận trong phiên làm việc, không lặp lại ở đây). Spec này chỉ xử lý phần còn lại: tổ chức
thư mục monorepo cho rõ ràng.

**Mục tiêu**: thêm 1 cấp thư mục nhóm bên trên các service hiện có, sao cho nhìn cây thư mục là biết ngay service
nào phục vụ domain nào — không đổi bất kỳ hành vi runtime, namespace, hay convention 5-layer nội bộ nào.

## 2. Quyết định đã chốt (qua trao đổi trực tiếp)

| Câu hỏi | Quyết định |
|---|---|
| `CommonLibs` đặt ở đâu? | **Giữ nguyên tại gốc repo** — dùng chung mọi domain, không thuộc riêng nhóm nào. |
| Domain chỉ có 1 service thì lồng thêm 1 cấp hay gộp tên? | **Luôn lồng thêm 1 cấp** (`SportsBooking/Booking/...`) — nhất quán dù domain có 1 hay nhiều service sau này, tránh phải tái cấu trúc lần nữa khi domain phình thêm service thứ 2. |
| Style đặt tên thư mục nhóm? | **Tiếng Anh** (`Base`, `SportsBooking`, `BloodDonation`) — khớp convention tên project/namespace C# hiện có (toàn bộ đều tiếng Anh). |
| Thời điểm thực hiện? | **Ngay bây giờ** — Booking mới có 3 entity (FieldType/Field/PricingRule), di chuyển ít việc hơn là để phình thêm Customer/Booking/RecurringGroup rồi mới move. |

## 3. Cấu trúc thư mục mục tiêu

```
BaseRepo/
├── BaseRepo.slnx
├── CommonLibs/                       # giữ nguyên tại gốc — không đổi
│
├── Base/                             # MỚI — nhóm nền tảng platform (auth, RBAC, danh mục hành chính)
│   ├── Gateway/
│   │   ├── Gateway.Api/
│   │   ├── Gateway.Application/
│   │   ├── Gateway.Contract/
│   │   ├── Gateway.Domain/
│   │   └── Gateway.Infrastructure/
│   ├── Site/
│   │   └── Site.{Api,Application,Contract,Domain,Infrastructure}/
│   └── AdministrativeUnit/
│       └── AdministrativeUnit.{Api,Application,Contract,Domain,Infrastructure}/
│
├── SportsBooking/                    # MỚI — domain "Quản lý thuê sân"
│   └── Booking/
│       └── Booking.{Api,Application,Contract,Domain,Infrastructure}/
│
├── BloodDonation/                    # MỚI — domain "Đăng ký hiến máu" (thư mục rỗng, tạo trước, service tạo sau khi có plan)
│
├── Elastic/                          # giữ nguyên — không thuộc kiến trúc 5-layer
├── SystemManagement/                 # giữ nguyên — không thuộc kiến trúc 5-layer
└── docs/                             # giữ nguyên
```

**Nguyên tắc bất biến**: tên project (`Gateway.Api.csproj`...), namespace C# (`Gateway.Api.Controllers`...),
tên file, nội dung code — **giữ nguyên 100%**. Đây thuần túy là di chuyển vị trí vật lý + cập nhật path tham
chiếu, không phải đổi tên hay refactor.

## 4. Cơ chế di chuyển (đã khảo sát thực tế repo, số liệu chính xác)

### 4.1 Di chuyển giữ lịch sử git

```
git mv Gateway Base/Gateway
git mv Site Base/Site
git mv AdministrativeUnit Base/AdministrativeUnit
mkdir SportsBooking && git mv Booking SportsBooking/Booking
mkdir BloodDonation && touch BloodDonation/.gitkeep   # thư mục rỗng, giữ chỗ
```

Đúng kỹ thuật đã dùng khi tách Gateway thành 5-layer trước đó (`git mv`, không tạo-rồi-xoá).

### 4.2 Path cần cập nhật (đã xác nhận qua khảo sát, không phải suy đoán)

Mỗi service bị lồng thêm 1 cấp thư mục → mọi `ProjectReference` trỏ tới `CommonLibs` phải thêm 1 cấp `..\`.
Đã kiểm tra thực tế `Gateway/Gateway.Api/Gateway.Api.csproj`:
```xml
<ProjectReference Include="..\..\CommonLibs\CommonLibs.csproj" />
```
→ sau khi move thành `Base/Gateway/Gateway.Api/...`, cần đổi thành `..\..\..\CommonLibs\CommonLibs.csproj`
(3 cấp: Api → Gateway → Base → gốc). Áp dụng tương tự cho **mọi** `ProjectReference` tới `CommonLibs` trong cả
4 service (Gateway, Site, AdministrativeUnit, Booking) — kể cả từ layer Application/Infrastructure, không chỉ
`.Api`. ProjectReference giữa các layer cùng 1 service (VD `Gateway.Api` → `Gateway.Application`) **không đổi**
vì di chuyển cùng nhau, độ sâu tương đối giữ nguyên.

Các nơi khác cần cập nhật path:
- **`BaseRepo.slnx`** — path của toàn bộ 20 project thuộc 4 service bị dời (5 project × 4 service).
- **`.vscode/launch.json`** — đã đọc thực tế, hiện có 3 cấu hình debug: Gateway, Site, AdministrativeUnit (path
  dạng `${workspaceFolder}/Gateway/Gateway.Api/bin/Debug/net10.0/Gateway.Api.dll` và `cwd` tương ứng) — cả 3 cần
  đổi path theo vị trí mới trong `Base/`. **Lưu ý phát hiện thêm (ngoài phạm vi spec này, chỉ ghi nhận)**:
  `Booking` hiện **chưa có** cấu hình debug nào trong file này — đây là gap có sẵn từ trước, không phải do reorg
  gây ra, không xử lý trong spec này trừ khi được yêu cầu riêng.

### 4.3 Đã xác minh KHÔNG cần đổi (tránh làm thừa)

Đã grep toàn bộ `.claude/skills/create-*/SKILL.md` — các skill này chỉ tham chiếu `CommonLibs.*` dưới dạng
namespace C# trong khối code mẫu (`using CommonLibs.DTOs;`...), **không** hard-code path tương đối kiểu
`..\..\CommonLibs` ở đâu cả. Namespace không đổi khi di chuyển vị trí vật lý → **các skill này không cần sửa**.

## 5. Tài liệu cần cập nhật theo

- **`CLAUDE.md`**: cây thư mục tổng quan (mục "📁 Cấu trúc thư mục tổng quan") + mục "Cấu trúc thư mục vật lý" —
  mô tả lại quy ước 2 cấp (`{DomainGroup}/{ServiceName}/{ServiceName}.{Layer}/`) thay vì 1 cấp
  (`{ServiceName}/{ServiceName}.{Layer}/`) như hiện tại. Cập nhật Progress Log ghi lại việc reorg.
- **`.claude/rules/01-architecture.md`**: dòng 44-45 hiện viết "cả 5 gom vào 1 folder cha `{Service}/`" — sửa
  thành 2 cấp, kèm quy ước: tạo service mới phải xác định trước thuộc domain nào (đặt vào `Base/` nếu là hạ
  tầng dùng chung, hoặc domain group tương ứng nếu là nghiệp vụ riêng; tạo domain group mới nếu chưa có).

## 6. Ngoài phạm vi spec này

- Không đổi tên project/namespace của bất kỳ service nào.
- Không đổi nội dung `appsettings.json`/`log4net.config`/`Keys/` bên trong từng service (các file này di chuyển
  cùng thư mục service, không cần sửa nội dung — chỉ path bên ngoài trỏ vào chúng, như `.vscode/launch.json`,
  mới cần sửa).
- Không tạo service thật cho `BloodDonation/` — thư mục này chỉ tạo trước làm chỗ chứa, service cụ thể sẽ có
  spec/plan riêng khi dự án "Đăng ký hiến máu" được brainstorm.
- Không xử lý gap "Booking chưa có trong `.vscode/launch.json`" (mục 4.2) — ghi nhận, để xử lý riêng nếu được
  yêu cầu.

## 7. Phạm vi test sau khi hoàn thành

- `dotnet restore` + `dotnet build BaseRepo.slnx` toàn bộ solution — phải 0 Error, không phát sinh warning mới
  liên quan tới path.
- `dotnet run` thử tối thiểu 1 trong 4 service đã di chuyển (khuyến nghị Gateway, vì có phụ thuộc
  `Keys/`/JWT nhạy với path) — xác nhận boot bình thường, `/swagger` truy cập được.
- Xác nhận `.vscode/launch.json` mở debug được ít nhất 1 trong 3 cấu hình đã sửa path.
