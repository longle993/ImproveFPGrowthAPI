using System.Net;

namespace CNDM
{
    public class ResponseInfo
    {
        public HttpStatusCode statusCode { get; set; }
        public string? error_code { get; set; }
        public string? message { get; set; }
        public object data { get; set; }
    }
}
