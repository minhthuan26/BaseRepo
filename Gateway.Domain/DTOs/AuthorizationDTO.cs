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
    }
}
