---
name: create-application-layer
description: >
  Tạo Repository Interface, CQRS Commands/Queries, Handlers và AutoMapper mappings trong tầng Application theo Clean Architecture cho BaseRepo project.
  Sử dụng khi cần định nghĩa use cases và business flows cho một entity.
---

# Skill: Tạo Application Layer

## Tổng quan

Tầng **Application** chứa các use cases của hệ thống, định nghĩa interfaces (để Infrastructure implement) và xử lý luồng thông qua CQRS pattern (bằng **MediatR**). Tầng này phụ thuộc vào Domain (Entities), Contract (DTOs — xem `create-contract-layer`) và CommonLibs.

### Thành phần cần tạo/cập nhật

| # | File / Folder | Mô tả |
|---|---|---|
| 1 | `{ServiceName}.Application/Interfaces/I{EntityName}Repository.cs` | Interface repository |
| 2 | `{ServiceName}.Application/Features/{EntityName}/` | Folder chứa các tính năng CQRS |
| 3 | `{ServiceName}.Application/Mappings/{ServiceName}Mapping.cs` | Cập nhật file mapping AutoMapper |

---

## Quy tắc

1. Mỗi Feature (Create, Update, Delete, GetById, GetAll) đều có cặp class: `Command`/`Query` và `Handler`.
2. **Command** dùng để thay đổi state (Create, Update, Delete) -> `IRequest<T>`
3. **Query** dùng để lấy dữ liệu (GetById, GetAll) -> `IRequest<T>`
4. Cấu trúc thư mục Feature: `Features/{EntityName}/{Action}{EntityName}/` (Ví dụ: `Features/User/CreateUser/CreateUserCommand.cs`)
5. Handler được inject các interfaces repository thông qua Constructor.
6. Mapping AutoMapper giúp chuyển đổi từ Entity -> OutputDTO và InputDTO -> OutputDTO.

---

## Template: Repository Interface

**File:** `{ServiceName}.Application/Interfaces/I{EntityName}Repository.cs`

```csharp
using CommonLibs.DTOs;
using {ServiceName}.Contract.DTOs;

namespace {ServiceName}.Application.Interfaces
{
    public interface I{EntityName}Repository
    {
        Task<{EntityName}OutputDTO> Create{EntityName}({EntityName}InputDTO input, CancellationToken cancellationToken);
        Task<{EntityName}OutputDTO> Update{EntityName}({EntityName}InputDTO input, CancellationToken cancellationToken);
        Task<{EntityName}OutputDTO> Get{EntityName}ById(string {entityName}Id, CancellationToken cancellationToken);
        Task<BaseFilterOutput<{EntityName}OutputDTO>> GetAll{EntityName}s({EntityName}FilterDTO filter, CancellationToken cancellationToken);
        Task<string> Delete{EntityName}(string {entityName}Id, CancellationToken cancellationToken);
    }
}
```

---

## Template: CQRS Features

Tạo thư mục cho entity: `{ServiceName}.Application/Features/{EntityName}/`

### 1. Create Command & Handler

**Thư mục:** `Create{EntityName}/`

**File:** `Create{EntityName}Command.cs`
```csharp
using {ServiceName}.Contract.DTOs;
using MediatR;

namespace {ServiceName}.Application.Features.{EntityName}.Create{EntityName}
{
    public class Create{EntityName}Command : IRequest<{EntityName}OutputDTO>
    {
        public {EntityName}InputDTO Data { get; set; }
    }
}
```

**File:** `Create{EntityName}CommandHandler.cs`
```csharp
using {ServiceName}.Application.Interfaces;
using {ServiceName}.Contract.DTOs;
using MediatR;

namespace {ServiceName}.Application.Features.{EntityName}.Create{EntityName}
{
    public class Create{EntityName}CommandHandler : IRequestHandler<Create{EntityName}Command, {EntityName}OutputDTO>
    {
        private readonly I{EntityName}Repository _repository;

        public Create{EntityName}CommandHandler(I{EntityName}Repository repository)
        {
            _repository = repository;
        }

        public async Task<{EntityName}OutputDTO> Handle(Create{EntityName}Command request, CancellationToken cancellationToken)
        {
            var result = await _repository.Create{EntityName}(request.Data, cancellationToken);
            return result;
        }
    }
}
```

### 2. GetAll Query & Handler

**Thư mục:** `GetAll{EntityName}s/`

**File:** `GetAll{EntityName}sQuery.cs`
```csharp
using CommonLibs.DTOs;
using {ServiceName}.Contract.DTOs;
using MediatR;

namespace {ServiceName}.Application.Features.{EntityName}.GetAll{EntityName}s
{
    public class GetAll{EntityName}sQuery : IRequest<BaseFilterOutput<{EntityName}OutputDTO>>
    {
        public {EntityName}FilterDTO Filter { get; set; }
    }
}
```

**File:** `GetAll{EntityName}sQueryHandler.cs`
```csharp
using CommonLibs.DTOs;
using {ServiceName}.Application.Interfaces;
using {ServiceName}.Contract.DTOs;
using MediatR;

namespace {ServiceName}.Application.Features.{EntityName}.GetAll{EntityName}s
{
    public class GetAll{EntityName}sQueryHandler : IRequestHandler<GetAll{EntityName}sQuery, BaseFilterOutput<{EntityName}OutputDTO>>
    {
        private readonly I{EntityName}Repository _repository;

        public GetAll{EntityName}sQueryHandler(I{EntityName}Repository repository)
        {
            _repository = repository;
        }

        public async Task<BaseFilterOutput<{EntityName}OutputDTO>> Handle(GetAll{EntityName}sQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetAll{EntityName}s(request.Filter, cancellationToken);
            return result;
        }
    }
}
```

*(Tương tự cho các tính năng GetById, Update, Delete)*

---

## Cập nhật AutoMapper

**File:** `{ServiceName}.Application/Mappings/{ServiceName}Mapping.cs`

Thêm đoạn sau vào bên trong Constructor của Profile:

```csharp
// --- {EntityName} ---
CreateMap<{EntityName}InputDTO, {EntityName}OutputDTO>()
    .ForMember(x => x.Status,
        opt => opt.MapFrom(src => src.Status.ToString()));

CreateMap<Domain.Entities.{EntityName}, {EntityName}OutputDTO>()
    .ForMember(x => x.Status,
        opt => opt.MapFrom(src => src.Status.ToString()));
```
