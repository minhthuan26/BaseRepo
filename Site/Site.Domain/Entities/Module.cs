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
        // SiteId ĐÃ BỊ XOÁ khỏi tbl_Modules (migration 04_globalize_module_catalog.sql) - Module giờ là danh mục
        // dùng chung toàn hệ thống, không còn thuộc riêng 1 Site nào. "Site nào có Module nào" nay xác định qua
        // bảng entitlement tbl_Site_Module (xem SiteModuleRepository), không còn thuộc entity Module này nữa.
        public string ModuleId { get; private set; } = null!;
        public string ModuleName { get; private set; } = null!;

        // --- Constructors ---
        protected Module()
        {
        }

        public Module(string moduleId, string moduleName)
        {
            SetModuleId(moduleId);
            SetModuleName(moduleName);
        }

        public Module(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetModuleId(row.Field<string>("ModuleId") ?? throw new AppException("ModuleId is required"));
            SetModuleName(row.Field<string>("ModuleName") ?? throw new AppException("ModuleName is required"));
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

    }
}
