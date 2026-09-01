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
    public class Role : BaseEntity
    {
        // --- Properties ---
        public string RoleId { get; private set; } = null!;
        public string RoleName { get; private set; } = null!;
        public string SiteId { get; private set; } = null!;

        // --- Constructors ---
        protected Role()
        {
        }

        public Role(string roleId, string roleName, string siteId)
        {
            SetRoleId(roleId);
            SetRoleName(roleName);
            SetSiteId(siteId);
        }

        public Role(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetRoleId(row.Field<string>("RoleId") ?? throw new AppException("RoleId is required"));
            SetRoleName(row.Field<string>("RoleName") ?? throw new AppException("RoleName is required"));
            SetSiteId(row.Field<string>("SiteId") ?? throw new AppException("SiteId is required"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetRoleId(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new AppException("RoleId could not be empty");

            RoleId = roleId.Trim();
        }

        public void SetRoleName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new AppException("RoleName could not be empty");

            RoleName = roleName.Trim();
        }

        public void SetSiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("SiteId could not be empty");

            SiteId = siteId.Trim();
        }
    }
}
