# API Response & Error Handling

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ [`CLAUDE.md`](../../CLAUDE.md).

## 1. Response chuẩn (bắt buộc mọi API)

Luôn trả `HTTP 200 OK` bọc trong `Response<T>`:

```csharp
return Ok(new Response<UserOutputDTO>()
{
    Success = true,
    Code = System.Net.HttpStatusCode.OK,
    Message = "Successfully",
    Data = result
});
```

Không tự chế schema response khác, không trả trực tiếp entity/DTO không bọc `Response<T>`.

## 2. Error Handling

- Toàn bộ exception nghiệp vụ ném bằng `AppException` (không dùng `Exception` trần).
- Mã lỗi bắt buộc theo format: `[{Service}_Repo_{Method}-{Number}]`
  - Ví dụ: `[User_Repo_CreateUser-01] Input is null.`
- Không tự bắt và nuốt exception im lặng — để `GlobalExceptionMiddleware` xử lý ở tầng global trừ khi có lý do nghiệp vụ rõ ràng.
