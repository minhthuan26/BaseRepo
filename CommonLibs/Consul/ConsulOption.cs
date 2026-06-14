using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Consul
{
    public class ConsulOption
    {
        public string Host { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceIP { get; set; } = "localhost";
        public int ServicePort { get; set; }
        public string HealthCheckUrl { get; set; } = string.Empty;
    }
}
