using Microsoft.AspNetCore.Authorization;

namespace Controllers.Attributes
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(permission) { }
    }
}
