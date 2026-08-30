using CommonLibs.DTOs;
using CommonLibs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Domain.DTOs
{
    #region Input DTOs
    /// <summary>
    /// Tạo dòng entitlement mới trong tbl_Site_Module - CHỈ đội vận hành gọi (nội bộ, xem
    /// SiteModuleController - endpoint này không có route YARP công khai). Mặc định tạo ở trạng thái
    /// TẮT (Status = Inactive) - Site Admin phải tự bật lại qua ToggleSiteModule sau khi được cấp.
    /// </summary>
    public class ProvisionSiteModuleInputDTO : BaseInputDTO
    {
        public string SiteId { get; set; }
        public string ModuleId { get; set; }

        public override Dictionary<string, string> HeaderMappings => new()
        {
            ["SiteId"] = nameof(SiteId),
            ["ModuleId"] = nameof(ModuleId),
            ["Status"] = nameof(Status)
        };
    }

    /// <summary>
    /// Bật/tắt 1 Module đã được cấp (đã có dòng trong tbl_Site_Module) cho đúng Site của người gọi -
    /// quyền Site Admin bình thường (RequireFunction "ToggleSiteModule"). SiteId KHÔNG nằm trong DTO này:
    /// luôn lấy từ claim "site_id" trong token của người gọi (xem SiteModuleController), không tin theo
    /// giá trị client tự truyền lên, để Site Admin của Site A không thể bật/tắt Module của Site B.
    /// </summary>
    public class ToggleSiteModuleInputDTO : BaseInputDTO
    {
        public string ModuleId { get; set; }

        /// <summary>true = bật (Active), false = tắt (Inactive). Set thẳng trạng thái đích thay vì "lật cờ"
        /// để thao tác idempotent - gọi lại nhiều lần với cùng giá trị không đổi kết quả.</summary>
        public bool Enable { get; set; }
    }
    #endregion

    #region Output DTOs
    public class SiteModuleOutputDTO : BaseOutputDTO
    {
        public string SiteId { get; set; }
        public string ModuleId { get; set; }
    }
    #endregion
}
