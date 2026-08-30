namespace CommonLibs.Utils
{
    /// <summary>
    /// Gắn lên 1 controller action để yêu cầu user phải được gán Function tương ứng (qua Role -> Role_Function)
    /// mới được gọi action đó. Đọc bởi <see cref="Middlewares.AuthorizationMiddleware"/> qua endpoint metadata,
    /// so khớp với danh sách Function đã nạp vào context.Items["allowedFunctions"]. KHÔNG có bypass nào cho Role
    /// tên "Admin" - mỗi Site tự có Role "Admin" riêng (multi-tenant) nên so khớp literal tên Role không còn
    /// đáng tin; Role "Admin" của 1 Site phải được cấp đầy đủ Function qua tbl_Role_Function như mọi Role khác.
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
