using CommonLibs.Entities;
using CommonLibs.Utils;
using System;
using System.Data;

namespace Site.Domain.Entities
{
    public class Screen : BaseEntity
    {
        // --- Properties ---
        public string ScreenId { get; private set; } = null!;
        public string ScreenName { get; private set; } = null!;
        public string ModuleId { get; private set; } = null!;

        // --- Constructors ---
        protected Screen()
        {
        }

        public Screen(string screenId, string screenName, string moduleId)
        {
            SetScreenId(screenId);
            SetScreenName(screenName);
            SetModuleId(moduleId);
        }

        public Screen(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetScreenId(row.Field<string>("ScreenId") ?? throw new AppException("ScreenId is required"));
            SetScreenName(row.Field<string>("ScreenName") ?? throw new AppException("ScreenName is required"));
            SetModuleId(row.Field<string>("ModuleId") ?? throw new AppException("ModuleId is required"));
        }

        // --- Guard Methods / Setters ---
        public void SetAutoId(long autoId)
        {
            if (autoId <= 0)
                throw new AppException("AutoId must be greater than 0");

            AutoId = autoId;
        }

        public void SetScreenId(string screenId)
        {
            if (string.IsNullOrWhiteSpace(screenId))
                throw new AppException("ScreenId could not be empty");

            ScreenId = screenId.Trim();
        }

        public void SetScreenName(string screenName)
        {
            if (string.IsNullOrWhiteSpace(screenName))
                throw new AppException("ScreenName could not be empty");

            ScreenName = screenName.Trim();
        }

        public void SetModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new AppException("ModuleId could not be empty");

            ModuleId = moduleId.Trim();
        }
    }
}