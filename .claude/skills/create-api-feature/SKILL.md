---
name: create-api-feature
description: >
  Tạo API mới theo Clean Architecture + CQRS pattern cho BaseRepo project. (Orchestrator Skill)
  Sử dụng khi cần thêm CRUD API cho một entity mới trong bất kỳ microservice nào (Gateway, Site, hoặc service mới).
  Skill này liệt kê quy trình và gọi tới các skill nhỏ hơn theo thứ tự các layer.
---

# Skill: Orchestrator - Tạo API Feature theo Clean Architecture

## Tổng quan

Để tạo một API feature hoàn chỉnh cho một entity trong BaseRepo, bạn cần đi qua các layer (từ trong ra ngoài) theo chuẩn Clean Architecture kết hợp CQRS (MediatR).

Quy trình chuẩn như sau:

1. **Domain Layer** (Entity)
2. **Contract Layer** (DTOs)
3. **Application Layer** (Use Cases, Interfaces)
4. **Infrastructure Layer** (Data Access)
5. **Presentation Layer** (API Controllers)

Bạn hãy sử dụng các skill chi tiết tương ứng cho từng layer dưới đây.

---

## 🚀 Các bước thực hiện

### 1. Tạo Domain Layer
- **Mục tiêu:** Tạo Entity.
- **Cách thực hiện:** Xem hướng dẫn tại **Skill `create-domain-layer`**.
- **File cần tạo:**
  - `{ServiceName}.Domain/Entities/{EntityName}.cs`

### 2. Tạo Contract Layer (DTOs)
- **Mục tiêu:** Tạo các class DTO (Input, Output, Filter).
- **Cách thực hiện:** Xem hướng dẫn tại **Skill `create-contract-layer`**.
- **File cần tạo:**
  - `{ServiceName}.Contract/DTOs/{EntityName}DTO.cs`

### 3. Tạo Application Layer
- **Mục tiêu:** Tạo Interface Repository, CQRS Commands/Queries, Handlers và thêm config AutoMapper.
- **Cách thực hiện:** Xem hướng dẫn tại **Skill `create-application-layer`**.
- **File cần tạo/cập nhật:**
  - `{ServiceName}.Application/Interfaces/I{EntityName}Repository.cs`
  - Các thư mục Features: `{ServiceName}.Application/Features/{EntityName}/...` (chứa Command/Query và Handler)
  - Cập nhật profile mapping trong `{ServiceName}.Application/Mappings/{ServiceName}Mapping.cs`

### 4. Tạo Infrastructure Layer
- **Mục tiêu:** Implement logic query Database bằng Dapper/SQL và đăng ký Dependency Injection.
- **Cách thực hiện:** Xem hướng dẫn tại **Skill `create-infrastructure-layer`**.
- **File cần tạo/cập nhật:**
  - `{ServiceName}.Infrastructure/Repositories/{EntityName}Repository.cs`
  - Cập nhật file `{ServiceName}.Infrastructure/DependencyInjection.cs`

### 5. Tạo Presentation Layer
- **Mục tiêu:** Tạo API Controller, gọi MediatR để xử lý Request và trả về định dạng `Response<T>`.
- **Cách thực hiện:** Xem hướng dẫn tại **Skill `create-presentation-layer`**.
- **File cần tạo:**
  - `{ServiceName}.Api/Controllers/{EntityName}Controller.cs`

---

## 🎯 Quy tắc Validation & Errors (Dùng chung)

1. **Tất cả các exception nghiệp vụ** ném bằng custom exception `AppException`.
2. **Quy tắc đánh mã lỗi:** `[{EntityName}_Repo_{Method}-{Number}]` hoặc `[{EntityName}_{Method}-{Number}]`.
   - *Ví dụ:* `[Product_Repo_Create-01] ProductId is required.`
3. **Response:** Tất cả API trả về `Http 200 OK` bọc trong schema JSON chuẩn:
   ```json
   {
      "success": true,
      "code": 200,
      "message": "Successfully",
      "data": { ... }
   }
   ```
