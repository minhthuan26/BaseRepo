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
    public class SiteInputDTO : BaseInputDTO
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public byte[]? SiteLogo { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Hotline { get; set; }
        public string? TaxCode { get; set; }
        public string? Representative { get; set; }
        public string? RepresentativeIdNumber { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["SiteId"] = nameof(SiteId),
            ["SiteName"] = nameof(SiteName),
            ["SiteLogo"] = nameof(SiteLogo),
            ["Address"] = nameof(Address),
            ["Email"] = nameof(Email),
            ["Hotline"] = nameof(Hotline),
            ["TaxCode"] = nameof(TaxCode),
            ["Representative"] = nameof(Representative),
            ["RepresentativeIdNumber"] = nameof(RepresentativeIdNumber),
            ["Status"] = nameof(Status)
        };
    }

    public class ImportSitesInputDTO : BaseInputDTO
    {
        public IFormFile File { get; set; }
    }

    public class DeleteSiteInputDTO : BaseInputDTO
    {
        public string? SiteId { get; set; }
        public bool IsSoftDelete { get; set; } = true;
    }
    #endregion

    #region Output DTOs
    public class SiteOutputDTO : BaseOutputDTO
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public byte[]? SiteLogo { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Hotline { get; set; }
        public string? TaxCode { get; set; }
        public string? Representative { get; set; }
        public string? RepresentativeIdNumber { get; set; }
    }
    #endregion

    #region Filter DTOs
    public class SiteFilterDTO : BaseFilterDTO
    {
        public string? SiteName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Hotline { get; set; }
        public string? TaxCode { get; set; }
        public string? Representative { get; set; }
        public string? RepresentativeIdNumber { get; set; }
    }
    #endregion
}
