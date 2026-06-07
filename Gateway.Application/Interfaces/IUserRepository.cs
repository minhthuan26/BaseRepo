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
        Task<UserOutputDTO> GetUserById(string userId, CancellationToken cancellationToken);
        Task<List<UserOutputDTO>> GetAllUsers(CancellationToken cancellationToken);
        Task<int> DeleteUser(string userId, CancellationToken cancellationToken);
        Task<List<UserOutputDTO>> ImportUsers(Stream stream);
    }
}
