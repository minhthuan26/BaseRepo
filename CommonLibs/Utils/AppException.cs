using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CommonLibs.Utils
{
    public class AppException : Exception
    {
        public HttpStatusCode Code { get; set; }
        public AppException(string message, HttpStatusCode code = HttpStatusCode.BadRequest)
        : base(message)
        {
            Code = code;
        }

        public AppException(string message, Exception inner, HttpStatusCode code = HttpStatusCode.BadRequest)
            : base(message, inner)
        {
            Code = code;
        }
    }
}
