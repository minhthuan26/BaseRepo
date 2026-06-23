using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class RoleInputDTO : BaseInputDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string SiteId { get; set; }
    }

    public class RoleOutputDTO : BaseOutputDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string SiteId { get; set; }
    }

    public class RoleFilterDTO : BaseFilterDTO
    {

        public string? RoleName { get; set; }
        public string? SiteId { get; set; }
    }
}
