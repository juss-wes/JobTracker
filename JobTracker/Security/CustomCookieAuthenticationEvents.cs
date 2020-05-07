using System.Linq;
using System.Threading.Tasks;
using JobTracker.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace JobTracker.Security
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomCookieAuthenticationEvents(ApplicationDbContext dbContext)
        {
            // Get the database from registered DI services.
            _dbContext = dbContext;
        }

        /// <summary>
        /// Custom validation. When a request is recieved by the server, this logic will check to verify the user exists
        /// and is active in the system. If the user has been deleted or inactivated since the last time they connect,
        /// the request will be rejected.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            // Look for the user
            var user = _dbContext.Users.SingleOrDefault(x => x.UserName == userPrincipal.Identity.Name);

            if (user == null || !user.IsActive)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}