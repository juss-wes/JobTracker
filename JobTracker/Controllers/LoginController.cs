using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JobTracker.Data;
using JobTracker.Models.Login;
using JobTracker.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Controllers
{
    public class LoginController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHelper _passwordHelper;

        public LoginController(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _passwordHelper = new PasswordHelper(new HashingOptions());
        }

        /// <summary>
        /// By default, route users to the Login page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(LoginModel loginData)
        {
            return View("Login", loginData ?? new LoginModel());
        }

        /// <summary>
        /// Returns the Manage Account page, loaded with the current user's data
        /// </summary>
        /// <returns>Returns the Manage Account page, loaded with the current user's data</returns>
        public IActionResult ManageAccount()
        {
            var user = _dbContext.Users.SingleOrDefault(x => x.UserName == HttpContext.User.Identity.Name);
            return View(new UserRegistrationModel
            {
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn,
                FirstName = user.FirstName,
                ID = user.ID,
                IsActive = user.IsActive,
                IsAdmin = user.IsAdmin,
                LastModifiedBy = user.LastModifiedBy,
                LastModifiedOn = user.LastModifiedOn,
                LastName = user.LastName,
                UserName = user.UserName
            });
        }

        /// <summary>
        /// Handles a user attempting to log in
        /// </summary>
        /// <param name="loginData">The form data supplied by the user</param>
        /// <returns>
        /// Model Errors to diplay on login fail, or redirects to the site homepage (/Inventory/Index)
        /// </returns>
        [HttpPost, ValidateAntiForgeryToken]
        [Route("Login/TryLoginUserAsync")]
        public async Task<IActionResult> TryLoginUserAsync(LoginModel loginData)
        {
            if (ModelState.IsValid)
            {
                ModelState.ClearValidationState("ErrorMessage");
                //validate the user exists
                var user = _dbContext.Users.SingleOrDefault(x => x.UserName == loginData.UserName);
                if (user == null)
                {
                    //return error. Best practice is not to specify whether it was the username or the password that failed
                    ModelState.AddModelError("ErrorMessage", "User Name or Password is invalid");
                    return View("Login", loginData);
                }

                var hasher = new PasswordHelper(new HashingOptions());
                var hashCheckResult = hasher.Check(user.HashedPassword, loginData.Password);
                if (hashCheckResult.NeedsUpgrade)
                {
                    //TODO: decide if we care to actually handle this - probably via emailing someone
                }
                if (!hashCheckResult.Verified)
                {
                    //return error. Best practice is not to specify whether it was the username or the password that failed
                    ModelState.AddModelError("ErrorMessage", "User Name or Password is invalid");
                    return View("Login", loginData);
                }

                //login the user and issue a claims identity stored in a cookie
                await LoginUserAsync(loginData.UserName, loginData.RememberMe);
                return RedirectToAction("Index", "Inventory");
            }
            else
            {
                ModelState.AddModelError("ErrorMessage", "User Name or Password not specified");
                return View("Login", loginData);
            }
        }

        /// <summary>
        /// Log in the user
        /// </summary>
        /// <param name="userName">The user name</param>
        /// <param name="rememberMe">True if the login should be persisted across browser sessions</param>
        /// <returns>An async task to await completion of</returns>
        private async Task LoginUserAsync(string userName, bool rememberMe)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));
            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = rememberMe });
        }


        /// <summary>
        /// Handles a user logging out
        /// </summary>
        [Route("Login/LogoutAsync")]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
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
        public async Task<IActionResult> RegisterNewUserAsync(UserRegistrationModel newUser)
        {
            var adminNewUserSetupMode = ViewBag.IsAdmin ?? false;

            //update metadata
            newUser.RefreshMetadata(newUser.UserName);

            //hash and store the password
            newUser.HashedPassword = _passwordHelper.Hash(newUser.Password);

            //mark the user as active by default
            newUser.IsActive = true;

            //validate input model
            ModelState.Clear();
            TryValidateModel(newUser);
            if (!ModelState.IsValid)
            {
                //if invalid, return validation errors
                ModelState.AddModelError("", "Invalid input detected");
                newUser.Password = "";
                newUser.HashedPassword = "";
                return View("Register", newUser);
            }

            //if another user with the same user name exists, add a validation message and return the validation message to the page
            if(_dbContext.Users.Any(x => x.UserName == newUser.UserName))
            {
                ModelState.AddModelError("UserName", "User Name already exists - please pick a different user name.");
                newUser.Password = "";
                newUser.HashedPassword = "";
                return View("Register", newUser);
            }

            //if this is the only user in the system, make them an admin
            if (_dbContext.Users.Count() == 0)
                newUser.IsAdmin = true;
            else
                newUser.IsAdmin = false;


            //persist to database
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            if (adminNewUserSetupMode)
            {
                //redirect back to user management screen
                return RedirectToAction("ManageUsers", "Admin");
            }
            else
            {
                //log in the user
                await LoginUserAsync(newUser.UserName, newUser.RememberMe);

                //redirect to the home page
                return RedirectToAction("Index", "Inventory");
            }
        }

        /// <summary>
        /// Handles changing an existing user's password
        /// </summary>
        /// <param name="existingUser">The user's info</param>
        /// <returns>
        /// Model Errors to diplay on user registration fail, or redirects to the site homepage (/Home/Index) 
        /// </returns>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangePassword(UserRegistrationModel existingUser)
        {
            //hash and store the password
            existingUser.HashedPassword = _passwordHelper.Hash(existingUser.Password);
            existingUser.RefreshMetadata(existingUser.UserName);

            //validate input model
            ModelState.Clear();
            TryValidateModel(existingUser);
            if (!ModelState.IsValid)
            {
                //if invalid, return validation errors
                ModelState.AddModelError("", "Invalid input detected");
                existingUser.Password = "";
                existingUser.HashedPassword = "";
                return View("ManageAccount", existingUser);
            }


            // Look for the user
            var user = _dbContext.Users.SingleOrDefault(x => x.UserName == HttpContext.User.Identity.Name);

            //sanity checks
            if (existingUser.UserName != user.UserName)
            {
                //if invalid, return validation errors
                ModelState.AddModelError("UserName", "You can't change another user's password!");
                existingUser.Password = "";
                existingUser.HashedPassword = "";
                existingUser.UserName = user.UserName;
                return View("ManageAccount", existingUser);
            }

            //update metadata and password on DB copy of data (so we dont risk a request being allowed to update random unrelated fields!)
            user.RefreshMetadata(existingUser.UserName);
            user.HashedPassword = existingUser.HashedPassword;//already computed, so no need to do so again
            
            //persist to database
            _dbContext.Users.Update(user);
            _dbContext.SaveChanges();

            //refresh the page
            existingUser.Password = "";
            existingUser.HashedPassword = "";
            existingUser.StatusMessage = "Password successfully changed!";
            return View("ManageAccount", existingUser);
        }
    }
}