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
    public class RoleFunction : BaseEntity
    {
        // --- Properties ---
        public string RoleId { get; private set; } = null!;
        public string FunctionId { get; private set; } = null!;
        // SiteId (denormalize từ tbl_Roles) - cột đã tồn tại trên tbl_Role_Function từ
        // 05_add_siteid_to_role_tables.sql nhưng trước đây chưa được map vào entity này.
        public string SiteId { get; private set; } = null!;

        // --- Constructors ---
        protected RoleFunction()
        {
        }

        public RoleFunction(string roleId, string functionId, string siteId)
        {
            SetRoleId(roleId);
            SetFunctionId(functionId);
            SetSiteId(siteId);
        }

        public RoleFunction(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetRoleId(row.Field<string>("RoleId")
                ?? throw new AppException("RoleId is required"));

            SetFunctionId(row.Field<string>("FunctionId")
                ?? throw new AppException("FunctionId is required"));

            SetSiteId(row.Field<string>("SiteId")
                ?? throw new AppException("SiteId is required"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetRoleId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("RoleId could not be empty");

            RoleId = siteId.Trim();
        }

        public void SetFunctionId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new AppException("FunctionId could not be empty");

            FunctionId = userId.Trim();
        }

        public void SetSiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("SiteId could not be empty");

            SiteId = siteId.Trim();
        }
    }
}
