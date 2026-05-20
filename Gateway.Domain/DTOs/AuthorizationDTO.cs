using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Domain.DTOs
{
    public class AuthorizationOutputDTO : BaseDTO
    {
        public string Id { get; set; }
        public string AccessToken { get; set; }
        public string AccountName { get; set; }
        public string RefreshToken { get; set; }
        public string PublicKey { get; set; }
    }

    public class AuthorizationInputDTO
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
    }
}
