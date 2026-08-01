using Gateway.Domain.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.ImportUsers
{
    public class ImportUsersCommand : IRequest<List<UserOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
    }
}
