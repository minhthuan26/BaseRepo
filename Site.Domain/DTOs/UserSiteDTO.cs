using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    public class UserSiteInputDTO : BaseInputDTO
    {
        public string SiteId { get; set; }
        public string UserId { get; set; }
    }

    public class UserSiteOutputDTO : BaseOutputDTO
    {
        public string SiteId { get; set; }
        public string UserId { get; set; }
    }

    public class UserSiteFilterDTO : BaseFilterDTO
    {
        public string? UserId { get; set; }
        public string? SiteId { get; set; }
    }
}
