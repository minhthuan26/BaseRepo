using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
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
    }

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
}
