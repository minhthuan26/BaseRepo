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
    public class UserSite : BaseEntity
    {
        // --- Properties ---
        public string SiteId { get; private set; } = null!;
        public string UserId { get; private set; } = null!;

        // --- Constructors ---
        protected UserSite()
        {
        }

        public UserSite(string siteId, string userId)
        {
            SetSiteId(siteId);
            SetUserId(userId);
        }

        public UserSite(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetSiteId(row.Field<string>("SiteId")
                ?? throw new AppException("SiteId is required"));

            SetUserId(row.Field<string>("UserId")
                ?? throw new AppException("UserId is required"));
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

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new AppException("UserId could not be empty");

            UserId = userId.Trim();
        }
    }
}
