---
name: create-presentation-layer
description: >
  Tạo Controller trong tầng Presentation để expose các API endpoints theo Clean Architecture cho BaseRepo project.
  Sử dụng khi cần tạo RESTful API (sử dụng MediatR để gọi Application layer).
---

# Skill: Tạo Presentation Layer

## Tổng quan

Tầng **Presentation** là đầu vào của HTTP request, chứa các Web API Controllers. Tại đây, Controllers chỉ có nhiệm vụ nhận Request, gửi qua **MediatR** đến tầng Application, sau đó nhận kết quả và wrap vào object `Response<T>` chuẩn trả về cho Client.

### Thành phần cần tạo

| # | File | Mô tả |
|---|---|---|
| 1 | `{ServiceName}/Controllers/{EntityName}Controller.cs` | Controller xử lý route và gọi MediatR |

---

## Quy tắc

1. Tất cả Controllers phải kế thừa class `BaseController<{EntityName}Controller>` từ thư viện `CommonLibs.Comtrollers`.
2. Controller sử dụng các Attribute: `[ApiController]`, `[Route("api/[controller]")]` hoặc custom path như `[Route("api/{entityName}")]`.
3. Có thể sử dụng `[Authorize(Policy = "UserOrAdminPolicy")]` nếu API yêu cầu quyền.
4. Trả về kết quả qua `Ok(new Response<T> { ... })`.

---

## Template: Controller

**File:** `{ServiceName}/Controllers/{EntityName}Controller.cs`

```csharp
using CommonLibs.Comtrollers; // Lưu ý: typo Comtrollers đang tồn tại trong project
using CommonLibs.DTOs;
using CommonLibs.Utils;
using {ServiceName}.Application.Features.{EntityName}.Create{EntityName};
using {ServiceName}.Application.Features.{EntityName}.GetAll{EntityName}s;
using {ServiceName}.Application.Features.{EntityName}.Get{EntityName}ById;
using {ServiceName}.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace {ServiceName}.Controllers
{
    // Đổi tên route phù hợp, thường là số nhiều hoặc số ít chữ thường
    [Route("api/{entityName}")] 
    [ApiController]
    [Authorize(Policy = "UserOrAdminPolicy")] // Xóa nếu không cần quyền
    public class {EntityName}Controller : BaseController<{EntityName}Controller>
    {
        public {EntityName}Controller(
            ILogger<{EntityName}Controller> logger,
            IMediator mediator,
            IHttpContextAccessor httpContext)
            : base(logger, mediator, httpContext)
        {
        }

        /// <summary>
        /// Tạo mới {EntityName}
        /// </summary>
        [HttpPost("")]
        public async Task<IActionResult> Create{EntityName}([FromBody] {EntityName}InputDTO data)
        {
            Create{EntityName}Command command = new()
            {
                Data = data
            };
            
            var result = await _mediator.Send(command);
            
            return Ok(new Response<{EntityName}OutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully created",
                Data = result
            });
        }

        /// <summary>
        /// Lấy chi tiết {EntityName} theo ID
        /// </summary>
        [HttpGet("{{{entityName}Id}}")]
        public async Task<IActionResult> Get{EntityName}([FromRoute] string {entityName}Id)
        {
            Get{EntityName}ByIdQuery query = new()
            {
                Data = {entityName}Id
            };
            
            var result = await _mediator.Send(query);
            
            return Ok(new Response<{EntityName}OutputDTO>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully retrieved",
                Data = result
            });
        }

        /// <summary>
        /// Lấy danh sách {EntityName} có phân trang và filter
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> GetAll{EntityName}s([FromQuery] {EntityName}FilterDTO filter)
        {
            GetAll{EntityName}sQuery query = new()
            {
                Filter = filter
            };
            
            var result = await _mediator.Send(query);
            
            return Ok(new Response<BaseFilterOutput<{EntityName}OutputDTO>>()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Message = "Successfully retrieved list",
                Data = result
            });
        }
    }
}
```
