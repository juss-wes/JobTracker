using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobTracker.Data;

namespace JobTracker.Controllers
{
    [Authorize]
    public class TimeReportingController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        public TimeReportingController(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
