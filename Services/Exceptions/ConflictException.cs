using System.Net;

namespace Services.Exceptions
{
    public class ConflictException : BusinessException
    {
        public ConflictException(string message) : base(message, HttpStatusCode.Conflict)
        {
        }
    }
}
