using System.Net;

namespace CommonLibs.Utils
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public HttpStatusCode Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
