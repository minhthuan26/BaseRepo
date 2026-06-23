using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class RoleFunctionInputDTO : BaseInputDTO
    {
        public string RoleId { get; set; }
        public string FunctionId { get; set; }
    }

    public class RoleFunctionOutputDTO : BaseOutputDTO
    {
        public string RoleId { get; set; }
        public string FunctionId { get; set; }
    }

    public class RoleFunctionFilterDTO : BaseFilterDTO
    {
        public string? RoleId { get; set; }
        public string? FunctionId { get; set; }
    }
}
