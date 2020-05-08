﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JobTracker.Data;
using JobTracker.Models.Login;
using JobTracker.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public LoginController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// By default, route users to the Login page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("Login", new LoginModel());
        }

        /// <summary>
        /// Handles a user attempting to log in
        /// </summary>
        /// <param name="loginData">The form data supplied by the user</param>
        /// <returns>
        /// Model Errors to diplay on login fail, or redirects to the site homepage (/Home/Index)
        /// </returns>
        [HttpPost, ValidateAntiForgeryToken]
        [Route("Login/TryLoginUserAsync")]
        public async Task<IActionResult> TryLoginUserAsync(LoginModel loginData)
        {
            if (ModelState.IsValid)
            {
                //validate the user exists
                var user = _dbContext.Users.SingleOrDefault(x => x.UserName == loginData.UserName);
                if (user == null)
                {
                    //return error. Best practice is not to specify whether it was the username or the password that failed
                    ModelState.AddModelError("", "username or password is invalid");
                    return View("Login");
                }

                var hasher = new PasswordHasher(new HashingOptions());
                var hashCheckResult = hasher.Check(user.HashedPassword, loginData.Password);
                if (hashCheckResult.NeedsUpgrade)
                {
                    //TODO: decide if we care to actually handle this - probably via emailing someone
                }
                if (!hashCheckResult.Verified)
                {
                    //return error. Best practice is not to specify whether it was the username or the password that failed
                    ModelState.AddModelError("", "username or password is invalid");
                    return View("Login");
                }

                //login the user and issue a claims identity stored in a cookie
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

        /// <summary>
        /// New user registration page
        /// </summary>
        /// <returns>The user registration page</returns>
        public IActionResult Register()
        {
            return View(new UserRegistrationModel());
        }

        /// <summary>
        /// Handles registration of a new user
        /// </summary>
        /// <param name="newUser">The new user's info</param>
        /// <returns>
        /// Model Errors to diplay on user registration fail, or redirects to the site homepage (/Home/Index) 
        /// </returns>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegisterNewUser(UserRegistrationModel newUser)
        {
            throw new NotImplementedException();
            //validate input model
            //  if invalid, return validation errors
            //  if another user with the same user name exists, add a validation message and return the validation message to the page

            //hash and store the password

            //if this is the only user in the system, make them an admin

            //update metadata

            //persist to database

            //redirect to the home page
            //return RedirectToPage("/Home/Index");
        }
    }
}