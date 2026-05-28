using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Options
{
    public class ApplicationOptions
    {
        public string ApplicationId { get; set; } = null!; 
        public string ApplicationName { get; set; } = null!;
        public string Version { get; set; } = null!;
    }
}
