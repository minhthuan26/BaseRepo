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

        // --- Constructors ---
        protected RoleFunction()
        {
        }

        public RoleFunction(string roleId, string functionId)
        {
            SetRoleId(roleId);
            SetFunctionId(functionId);
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
    }
}
