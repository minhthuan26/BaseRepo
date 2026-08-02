using CommonLibs.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    #region Input DTOs
    public class UserSiteInputDTO : BaseInputDTO
    {
        public string SiteId { get; set; }
        public string UserId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["UserId"] = nameof(UserId),
            ["SiteId"] = nameof(SiteId),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportUserSitesInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteUserSiteInputDTO : BaseInputDTO
    {
        public string? UserId { get; set; }
        public string? SiteId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class UserSiteOutputDTO : BaseOutputDTO
    {
        public string SiteId { get; set; }
        public string UserId { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class UserSiteFilterDTO : BaseFilterDTO
    {
        public string? UserId { get; set; }
        public string? SiteId { get; set; }
    }
    #endregion
}
