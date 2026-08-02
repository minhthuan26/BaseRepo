using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.DeleteUserSite
{
    public class DeleteUserSiteCommand : IRequest<string>
    {
        public DeleteUserSiteInputDTO Data { get; set; }
    }
}
