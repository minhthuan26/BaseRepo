namespace CommonLibs.Logging
{
    // Dùng chung cho mọi nơi log HTTP headers (SerilogExtension, Log4netExtension, GlobalExceptionMiddleware)
    // để đảm bảo Authorization/Cookie/... luôn được mask nhất quán, tránh trường hợp thêm 1 nơi log headers
    // mới mà quên mask.
    public static class SensitiveHeaderMasker
    {
        private static readonly string[] SensitiveHeaders =
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "Password",
            "RefreshToken",
            "AccessToken"
        };

        public static string Mask(string key, string value)
        {
            if (SensitiveHeaders.Any(x => key.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                return "***MASKED***";
            }

            return value;
        }
    }
}
