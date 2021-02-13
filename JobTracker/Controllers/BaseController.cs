using JobTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace JobTracker.Controllers
{
    /// <summary>
    /// Abstract class that should be inherited by all controllers.
    /// </summary>
    public abstract class BaseController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BaseController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Runs when an action is about to execute. Retrieves and stores the user's IsAdmin flag value.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (string.IsNullOrEmpty(HttpContext.User.Identity.Name)) return;

            var user = _dbContext.Users.SingleOrDefault(x => x.UserName == HttpContext.User.Identity.Name);
            ViewBag.IsAdmin = user.IsAdmin;
        }
    }
}
