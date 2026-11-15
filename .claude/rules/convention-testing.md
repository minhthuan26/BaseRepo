---
description: "Convention Test (xUnit)"
paths:
  - '**/*.Tests/**'
  - '**/*Tests.cs'
---

# Convention Test (xUnit)

> Repo hiện **chưa có test project**. Task đầu tiên cần unit test sẽ tạo project theo quy ước dưới đây
> (thêm package xUnit cần hỏi người dùng trước). Test API thủ công: xem `06-testing.md`.

## Cấu trúc
- Mỗi service một project `{Service}.Tests` đặt cạnh 5 layer: `{DomainGroup}/{Service}/{Service}.Tests/`, thêm vào `BaseRepo.slnx`.
- Thư mục test mirror theo layer: `Application/Features/{Entity}/...`, `Infrastructure/Repositories/...`.
- Chạy: `dotnet test BaseRepo.slnx` (hoặc chỉ project liên quan khi đang sửa 1 service).

## Viết test
- Tên test: `<Phương thức>_<Tình huống>_<Kết quả mong đợi>`.
- Mỗi test kiểm tra một hành vi; dữ liệu giả, không phụ thuộc DB/Redis/RabbitMQ thật.
- Handler test bằng mock `I{Name}Repository`; logic phân quyền/Site-scoping cần test cả trường hợp sai `site_id`.
- Mỗi tiêu chí nghiệm thu trong coding-spec có ít nhất một test.
