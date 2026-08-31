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
    /// <summary>
    /// Entitlement + bật/tắt Module theo từng Site (xem tài liệu thiết kế multi-tenant RBAC, mục 04+07).
    /// Sự tồn tại của 1 dòng = Site đã "mua"/được cấp Module đó (provisioning, việc của đội vận hành).
    /// <see cref="Status"/> = bật/tắt Module đã được cấp cho Site (quyền Site Admin bình thường) - không tồn tại
    /// dòng nào thì không có cách nào Site tự "bật" ra được, kể cả Site Admin.
    /// </summary>
    public class SiteModule : BaseEntity
    {
        // --- Properties ---
        public string SiteId { get; private set; } = null!;
        public string ModuleId { get; private set; } = null!;

        // --- Constructors ---
        protected SiteModule()
        {
        }

        public SiteModule(string siteId, string moduleId)
        {
            SetSiteId(siteId);
            SetModuleId(moduleId);
        }

        public SiteModule(DataRow row)
        {
            if (row == null)
                throw new AppException("DataRow cannot be null");

            SetAutoId(row.Field<long>("AutoId"));
            SetSiteId(row.Field<string>("SiteId") ?? throw new AppException("SiteId is required"));
            SetModuleId(row.Field<string>("ModuleId") ?? throw new AppException("ModuleId is required"));
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

        public void SetModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new AppException("ModuleId could not be empty");

            ModuleId = moduleId.Trim();
        }
    }
}
