using Gateway.Domain.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Application.Features.User.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserOutputDTO>
    {
        public string Data { get; set; }
        // Site của người gọi (claim "site_id") - bắt buộc, UserId không còn unique toàn cục.
        public string SiteId { get; set; }
    }
}
