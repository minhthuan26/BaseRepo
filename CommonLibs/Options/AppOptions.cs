using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Options
{
    public class AppOptions
    {
        public string ApplicationId { get; set; } = null!; 
        public string ApplicationName { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string PrivateKey { get; set; } = null!;
        public string PublicKey { get; set; } = null!;

        public byte[] ActualAccessTokenByte { get; set; } = null!;
        public byte[] ActualRefreshTokenByte { get; set; } = null!;
        public string ActualAccessToken { get; set; } = null!;
        public string ActualRefreshToken { get; set; } = null!;
        public string ActualAudience { get; set; } = null!;
        public string ActualIssuer { get; set; } = null!;
        public string ActualPrivateKey { get; set; } = null!;
        public string ActualPublicKey { get; set; } = null!;

        public string RedisConnectionString { get; set; } = null!;
        public string RabbitMqConnectionString{ get; set; } = null!;

    }
}
