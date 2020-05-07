using System;
using System.Security.Claims;
using System.Threading.Tasks;
using JobTracker.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View("Login", new LoginModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("Login/TryLoginUserAsync")]
        public async Task<IActionResult> TryLoginUserAsync(LoginModel loginData)
        {
            if (ModelState.IsValid)
            {
                var isValid = (loginData.UserName == "username" && loginData.Password == "password");//TODO: validate login against DB
                if (!isValid)
                {
                    ModelState.AddModelError("", "username or password is invalid");
                    return View("Login");
                }
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, loginData.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Name, loginData.UserName));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = loginData.RememberMe });
                return RedirectToPage("/Home/Index");
            }
            else
            {
                ModelState.AddModelError("", "username or password is blank");
                return View("Login");
            }
        }
    }
}