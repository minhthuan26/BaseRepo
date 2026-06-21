using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Logging
{
    public sealed class LoggingOptions
    {
        public string Provider { get; set; } = "Serilog";
        public bool LogRequestBody { get; set; } = true;
        public bool LogResponseBody { get; set; } = true;
        public bool LogSuccessResponse { get; set; } = false;
        public bool LogFailedResponse { get; set; } = true;
        public int MaxBodyLength { get; set; } = 5000;
    }
}
