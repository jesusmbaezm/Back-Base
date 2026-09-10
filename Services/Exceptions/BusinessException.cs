using System.Net;

namespace Services.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
