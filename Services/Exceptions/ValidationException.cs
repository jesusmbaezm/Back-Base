using System.Net;

namespace Services.Exceptions
{
    public class ValidationException : BusinessException
    {
        public ValidationException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
