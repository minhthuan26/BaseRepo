using MediatR;

namespace Gateway.Application.Features.User.ExportUsers
{
    public class ExportUsersQuery : IRequest<string>
    {
        // Site của người gọi (claim "site_id") - bắt buộc, để không export chéo dữ liệu User của Site khác.
        public string SiteId { get; set; }
    }
}
