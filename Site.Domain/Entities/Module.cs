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
    public class Module : BaseEntity
    {
        // --- Properties ---
        public string ModuleId { get; private set; } = null!;
        public string ModuleName { get; private set; } = null!;
        public string SiteId { get; private set; } = null!;

        // --- Constructors ---
        protected Module()
        {
        }

        public Module(string moduleId, string moduleName, string siteId)
        {
            SetModuleId(moduleId);
            SetModuleName(moduleName);
            SetSiteId(siteId);
        }

        public Module(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetModuleId(row.Field<string>("ModuleId") ?? throw new AppException("ModuleId is required"));
            SetModuleName(row.Field<string>("ModuleName") ?? throw new AppException("ModuleName is required"));
            SetSiteId(row.Field<string>("SiteId") ?? throw new AppException("SiteId is required"));
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

        public void SetModuleName(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new AppException("ModuleName could not be empty");

            ModuleName = moduleName.Trim();
        }

        public void SetSiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new AppException("SiteId could not be empty");

            SiteId = siteId.Trim();
        }
    }
}
