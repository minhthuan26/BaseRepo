namespace CommonLibs.Utils
{
    public class UserPermissionCache
    {
        public List<string> Roles { get; set; } = new();
        public List<string> Functions { get; set; } = new();

        /// <summary>SiteId của các Site mà user là Admin riêng (có Role tên "Admin" gán cho đúng Site đó).</summary>
        public List<string> AdminSiteIds { get; set; } = new();

        public static string GetCacheKey(string userId) => $"auth:user-permissions:{userId}";
    }
}
