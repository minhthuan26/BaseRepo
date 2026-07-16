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
        public string AccessKey { get; set; } = null!;
        public string RefreshKey { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string PrivateKey { get; set; } = null!;
        public string PublicKey { get; set; } = null!;

        public byte[] ActualAccessKeyByte { get; set; } = null!;
        public byte[] ActualRefreshKeyByte { get; set; } = null!;
        public string ActualAccessKey { get; set; } = null!;
        public string ActualRefreshKey { get; set; } = null!;
        public string ActualAudience { get; set; } = null!;
        public string ActualIssuer { get; set; } = null!;
        public string ActualPrivateKey { get; set; } = null!;
        public string ActualPublicKey { get; set; } = null!;

        public string RedisConnectionString { get; set; } = null!;
        public string RabbitMqConnectionString{ get; set; } = null!;

    }
}
