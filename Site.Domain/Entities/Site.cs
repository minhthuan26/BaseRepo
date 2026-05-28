using CommonLibs.Entities;
using CommonLibs.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.Entities
{
    public class Site : BaseEntity
    {
        // --- Properties ---
        public string SiteId { get; private set; } = null!;
        public string SiteName { get; private set; } = null!;
        public byte[]? SiteLogo { get; private set; }
        public string? Address { get; private set; }
        public string? Email { get; private set; }
        public string? Hotline { get; private set; }
        public string? TaxCode { get; private set; }
        public string? Representative { get; private set; }
        public string? RepresentativeIdNumber { get; private set; }

        // --- Constructors ---
        protected Site()
        {
        }

        public Site(string siteId, string siteName)
        {
            SetSiteId(siteId);
            SetSiteName(siteName);
        }

        public Site(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetSiteId(row.Field<string>("SiteId") ?? throw new AppException("SiteId is required"));
            SetSiteName(row.Field<string>("SiteName") ?? throw new AppException("SiteName is required"));
            SetSiteLogo(row.Field<byte[]?>("SiteLogo"));
            SetAddress(row.Field<string?>("Address"));
            SetEmail(row.Field<string?>("Email"));
            SetHotline(row.Field<string?>("Hotline"));
            SetTaxCode(row.Field<string?>("TaxCode"));
            SetRepresentative(row.Field<string?>("Representative"));
            SetRepresentativeIdNumber(row.Field<string?>("RepresentativeIdNumber"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetSiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("SiteId could not be empty");

            SiteId = siteId.Trim();
        }

        public void SetSiteName(string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteName))
                throw new AppException("SiteName could not be empty");

            SiteName = siteName.Trim();
        }

        public void SetSiteLogo(byte[]? siteLogo)
        {
            SiteLogo = siteLogo;
        }

        public void SetAddress(string? address)
        {
            Address = address;
        }

        public void SetEmail(string? email)
        {
            if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
            {
                throw new AppException("Invalid email format");
            }

            Email = email;
        }

        public void SetHotline(string? hotline)
        {
            Hotline = hotline;
        }

        public void SetTaxCode(string? taxCode)
        {
            TaxCode = taxCode;
        }

        public void SetRepresentative(string? representative)
        {
            Representative = representative;
        }

        public void SetRepresentativeIdNumber(string? representativeIdNumber)
        {
            RepresentativeIdNumber = representativeIdNumber;
        }
    }
}
