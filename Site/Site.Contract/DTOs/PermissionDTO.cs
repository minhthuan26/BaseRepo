using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Contract.DTOs
{
    #region Tree DTOs
    public class PermissionTreeDTO
    {
        public string SiteId { get; set; }
        public string? RoleId { get; set; }
        public List<PermissionModuleDTO> Modules { get; set; } = new();
    }

    public class PermissionModuleDTO
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<PermissionScreenDTO> Screens { get; set; } = new();
    }

    public class PermissionScreenDTO
    {
        public string ScreenId { get; set; }
        public string ScreenName { get; set; }
        public List<PermissionFunctionDTO> Functions { get; set; } = new();
    }

    public class PermissionFunctionDTO
    {
        public string FunctionId { get; set; }
        public string FunctionName { get; set; }
        public bool IsAssigned { get; set; }
    }
    #endregion

    #region Input DTOs
    public class AssignRolePermissionsInputDTO
    {
        public List<string> FunctionIds { get; set; } = new();
    }
    #endregion
}
