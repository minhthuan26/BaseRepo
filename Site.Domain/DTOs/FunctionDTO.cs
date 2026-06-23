using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class FunctionInputDTO : BaseInputDTO
    {
        public string FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string ScreenId { get; set; }
    }

    public class FunctionOutputDTO : BaseOutputDTO
    {
        public string FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string ScreenId { get; set; }
    }

    public class FunctionFilterDTO : BaseFilterDTO
    {

        public string? FunctionName { get; set; }
        public string? ScreenId { get; set; }
    }
}
