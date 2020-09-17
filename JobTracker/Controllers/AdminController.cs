using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobTracker.Data;
using System.Linq;

namespace JobTracker.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        public AdminController(ApplicationDbContext dbContext) 
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageUsers()
        {
            return View(_dbContext.Users.ToList());
        }
    }
}
