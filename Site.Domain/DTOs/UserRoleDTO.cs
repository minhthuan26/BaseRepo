using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class UserRoleInputDTO : BaseInputDTO
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
    }

    public class UserRoleOutputDTO : BaseOutputDTO
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
    }

    public class UserRoleFilterDTO : BaseFilterDTO
    {
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
    }
}
