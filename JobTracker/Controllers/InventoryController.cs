using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobTracker.Models;
using Microsoft.AspNetCore.Authorization;

namespace JobTracker.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
