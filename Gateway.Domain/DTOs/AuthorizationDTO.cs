using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Domain.DTOs
{
    public class AuthorizationOutputDTO : BaseOutputDTO
    {
        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
        public string PublicKey { get; set; }
    }

    public class AuthorizationInputDTO : BaseInputDTO
    {
        public string UserId { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// SiteId của Site muốn đăng nhập vào. Bỏ trống được nếu hệ thống chỉ có đúng 1 Site đang active
        /// (bản deploy on-prem/silo cho khách có máy chủ riêng) - lúc đó Login tự suy ra Site duy nhất đó.
        /// Bắt buộc truyền khi hệ thống có nhiều hơn 1 Site (bản thuê trọn bộ, nhiều Site chung 1 cụm hạ tầng),
        /// vì UserId không còn unique toàn hệ thống mà chỉ unique trong phạm vi 1 Site.
        /// </summary>
        public string? SiteId { get; set; }
    }
}
