using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class ModuleInputDTO : BaseInputDTO
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string SiteId { get; set; }
    }

    public class ModuleOutputDTO : BaseOutputDTO
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string SiteId { get; set; }
    }

    public class ModuleFilterDTO : BaseFilterDTO
    {
        public string? ModuleName { get; set; }
        public string? SiteId { get; set; }
    }
}
