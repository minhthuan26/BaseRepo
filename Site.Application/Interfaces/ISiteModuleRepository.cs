using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface ISiteModuleRepository
    {
        /// <summary>Nội bộ - đội vận hành cấp entitlement mới cho 1 Site. Xem ProvisionSiteModuleInputDTO.</summary>
        Task<SiteModuleOutputDTO> ProvisionSiteModule(ProvisionSiteModuleInputDTO input, CancellationToken cancellationToken);

        /// <summary>Nội bộ - đội vận hành xem toàn bộ entitlement (mọi Status) của 1 Site bất kỳ để đối soát.</summary>
        Task<List<SiteModuleOutputDTO>> GetSiteModulesBySite(string siteId, CancellationToken cancellationToken);

        /// <summary>Site Admin bật/tắt 1 Module đã được cấp cho đúng Site của họ. Ném lỗi nếu Module chưa
        /// được đội vận hành cấp (chưa có dòng nào trong tbl_Site_Module cho cặp SiteId/ModuleId này).</summary>
        Task<SiteModuleOutputDTO> ToggleSiteModule(string siteId, ToggleSiteModuleInputDTO input, CancellationToken cancellationToken);
    }
}
