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
    public class UserModule : BaseEntity
    {
        // --- Properties ---
        public string ModuleId { get; private set; } = null!;
        public string UserId { get; private set; } = null!;

        // --- Constructors ---
        protected UserModule()
        {
        }

        public UserModule(string moduleId, string userId)
        {
            SetModuleId(moduleId);
            SetUserId(userId);
        }

        public UserModule(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetModuleId(row.Field<string>("ModuleId") ?? throw new AppException("ModuleId is required"));
            SetUserId(row.Field<string>("UserId") ?? throw new AppException("UserId is required"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new AppException("ModuleId could not be empty");

            ModuleId = moduleId.Trim();
        }

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new AppException("UserId could not be empty");

            UserId = userId.Trim();
        }
    }
}
