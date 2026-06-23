using CommonLibs.DTOs;
using Site.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IScreenDTO
    {
        Task<ScreenOutputDTO> CreateScreen(ScreenInputDTO input, CancellationToken cancellationToken);
        Task<ScreenOutputDTO> UpdateScreen(ScreenInputDTO input, CancellationToken cancellationToken);
        Task<ScreenOutputDTO> GetScreenById(string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<ScreenOutputDTO>> GetAllScreens(ScreenFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteScreen(string siteId, CancellationToken cancellationToken);
        Task<List<ScreenOutputDTO>> ImportScreens(Stream stream);
        Task<string> ExportScreens(CancellationToken cancellationToken);
    }
}
