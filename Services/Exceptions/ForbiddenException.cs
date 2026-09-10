using System.Net;

namespace Services.Exceptions
{
    public class ForbiddenException : BusinessException
    {
        public ForbiddenException(string message) : base(message, HttpStatusCode.Forbidden)
        {
        }
    }
}
