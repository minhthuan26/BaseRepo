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
    public class UserRole : BaseEntity
    {
        // --- Properties ---
        public string RoleId { get; private set; } = null!;
        public string UserId { get; private set; } = null!;

        // --- Constructors ---
        protected UserRole()
        {
        }

        public UserRole(string roleId, string userId)
        {
            SetRoleId(roleId);
            SetUserId(userId);
        }

        public UserRole(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetRoleId(row.Field<string>("RoleId") ?? throw new AppException("RoleId is required"));
            SetUserId(row.Field<string>("UserId") ?? throw new AppException("UserId is required"));
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

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new AppException("UserId could not be empty");

            UserId = userId.Trim();
        }
    }
}
