using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Romarr.Http.Authentication
{
    public class BypassableDenyAnonymousAuthorizationRequirement : DenyAnonymousAuthorizationRequirement
    {
    }
}
