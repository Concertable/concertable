using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Web.Authorization
{
    public class AdminAuthorizeHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IWebHostEnvironment environment;

        public AdminAuthorizeHandler(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
            if (requirement.AllowedRoles.Any(context.User.IsInRole) || IsAdmin(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsAdmin(AuthorizationHandlerContext context)
        {
            if (context.User.IsInRole("Admin"))
                return true;
            return false;
        }
    }
}
