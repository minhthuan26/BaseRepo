using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Options
{
    public class ServiceDiscoveryInfo
    {
        public string ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string Version { get; set; }

        public string Url { get; set; }

        public string MachineName { get; set; }

        public int ProcessId { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime LastHeartbeat { get; set; }

        public string Status { get; set; }
    }
}
