using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobTracker.Models;

namespace JobTracker.Controllers
{
    public class TimeReportingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
