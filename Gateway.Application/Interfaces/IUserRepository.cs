using CommonLibs.DTOs;
using Gateway.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<UserOutputDTO> CreateUser(UserInputDTO input, CancellationToken cancellationToken);
        Task<UserOutputDTO> UpdateUser(UserInputDTO input, CancellationToken cancellationToken);
        Task<UserOutputDTO> GetUserById(string userId, string siteId, CancellationToken cancellationToken);
        Task<BaseFilterOutput<UserOutputDTO>> GetAllUsers(UserFilterDTO filter, CancellationToken cancellationToken);
        Task<string> DeleteUser(DeleteUserInputDTO input, CancellationToken cancellationToken);
        Task<List<UserOutputDTO>> ImportUsers(Stream stream, string fileName, string siteId, CancellationToken cancellationToken);
        Task<string> ExportUsers(string siteId, CancellationToken cancellationToken);
        Task<UserOutputDTO> ChangePassword(ChangePasswordInputDTO input, CancellationToken cancellationToken);
    }
}
