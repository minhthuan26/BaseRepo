using CommonLibs.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Domain.DTOs
{
    public class UserOutputDTO : BaseOutputDTO
    {
        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public int Priority { get; private set; }

        public string? RefreshToken { get; private set; }

        public string? Signature { get; private set; }

        public string? Email { get; private set; }

        public string? QuickCode { get; private set; }
    }

    public class UserInputDTO : BaseInputDTO
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public int Priority { get; set; } = 1;
        public string? RefreshToken { get; set; }
        public string? Signature { get; set; }
        public string? Email { get; set; }
        public string? QuickCode { get; set; }
    }
}
