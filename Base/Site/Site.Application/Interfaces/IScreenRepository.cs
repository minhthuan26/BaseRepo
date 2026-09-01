using CommonLibs.DTOs;
using Site.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Application.Interfaces
{
    public interface IScreenRepository
    {
        Task<ScreenOutputDTO> CreateScreen(ScreenInputDTO input, CancellationToken cancellationToken);
        Task<ScreenOutputDTO> UpdateScreen(ScreenInputDTO input, CancellationToken cancellationToken);
        Task<ScreenOutputDTO> GetScreenById(string screenId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<ScreenOutputDTO>> GetAllScreens(ScreenFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteScreen(DeleteScreenInputDTO input, CancellationToken cancellationToken);
        Task<List<ScreenOutputDTO>> ImportScreens(Stream stream, string fileName, CancellationToken cancellationToken);
        Task<string> ExportScreens(CancellationToken cancellationToken);
    }
}
