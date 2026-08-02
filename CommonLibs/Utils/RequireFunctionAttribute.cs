namespace CommonLibs.Utils
{
    /// <summary>
    /// Gắn lên 1 controller action để yêu cầu user phải được gán Function tương ứng (qua Role -> Role_Function)
    /// mới được gọi action đó. Đọc bởi <see cref="Middlewares.AuthorizationMiddleware"/> qua endpoint metadata,
    /// so khớp với danh sách Function đã nạp vào context.Items["allowedFunctions"]. User có Role "Admin" (toàn
    /// hệ thống) luôn được bỏ qua kiểm tra này.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireFunctionAttribute : Attribute
    {
        public string FunctionId { get; }

        public RequireFunctionAttribute(string functionId)
        {
            FunctionId = functionId;
        }
    }
}
