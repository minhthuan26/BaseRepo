using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class ScreenInputDTO : BaseInputDTO
    {
        public string ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string ModuleId { get; set; }
    }

    public class ScreenOutputDTO : BaseOutputDTO
    {
        public string ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string ModuleId { get; set; }
    }

    public class ScreenFilterDTO : BaseFilterDTO
    {

        public string? ScreenName { get; set; }
        public string? ModuleId { get; set; }
    }
}
