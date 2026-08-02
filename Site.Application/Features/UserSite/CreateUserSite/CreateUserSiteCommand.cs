using MediatR;
using Site.Domain.DTOs;

namespace Site.Application.Features.UserSite.CreateUserSite
{
    public class CreateUserSiteCommand : IRequest<UserSiteOutputDTO>
    {
        public UserSiteInputDTO Data { get; set; }
    }
}
