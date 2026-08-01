# Data Access & Dependency Injection

> Một phần của bộ rules trong [`.claude/rules/`](README.md), đúc kết từ [`CLAUDE.md`](../../CLAUDE.md).

## 1. Data Access

- Ưu tiên **Dapper** (`_dbContext.QueryAsync<T>()`, `_dbContext.ExecuteAsync()`) hoặc **Stored Procedure** qua `ExecuteProcAsync()` từ `BaseRepository`.
- Transaction dùng `_dbContext.ExecuteWithTransactionAsync()`.
- Bulk insert dùng `_dbContext.ExecuteBulkAsync()`.
- Không dùng EF Core LINQ query trực tiếp thay cho Dapper/SP nếu convention hiện tại của repo là Dapper — kiểm tra repository cùng entity/feature trước khi viết.
- Repository class luôn chia 3 region: `#region Stored Procedure`, `#region Repository Methods`, `#region Helpers`.

## 2. Dependency Injection

Mỗi service (`{Service}.Infrastructure/DependencyInjection.cs`) đăng ký theo đúng thứ tự:

```csharp
public static IServiceCollection Add{Service}Infrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddEnsureServiceRunning();
    services.AddCommonServices<{Service}DbContext>(configuration, "{Service}.Application");
    services.AddScoped<I{Name}Repository, {Name}Repository>();
    return services;
}
```

Gọi trong `Program.cs` bằng `builder.Services.Add{Service}Infrastructure(builder.Configuration)`.
