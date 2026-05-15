using Microsoft.AspNetCore.Authorization;

namespace HotelPro.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = permission;
    }
}
