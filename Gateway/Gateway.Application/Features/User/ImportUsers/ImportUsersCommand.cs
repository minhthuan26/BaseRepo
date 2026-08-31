using Gateway.Contract.DTOs;
using MediatR;

namespace Gateway.Application.Features.User.ImportUsers
{
    public class ImportUsersCommand : IRequest<List<UserOutputDTO>>
    {
        public Stream Data { get; set; }
        public string FileName { get; set; }
        // Site của người gọi (claim "site_id") - mọi User trong file CSV luôn được import vào đúng Site này,
        // không đọc/tin theo bất kỳ SiteId nào trong file (nếu có).
        public string SiteId { get; set; }
    }
}
