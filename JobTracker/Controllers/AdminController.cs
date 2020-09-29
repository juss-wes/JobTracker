using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobTracker.Data;
using System.Linq;
using System;
using JobTracker.Models.Login;
using JobTracker.Security;
using JobTracker.Extensions;

namespace JobTracker.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHelper _passwordHelper;
        public AdminController(ApplicationDbContext dbContext) 
            : base(dbContext)
        {
            _dbContext = dbContext;
            _passwordHelper = new PasswordHelper(new HashingOptions());
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageUsers()
        {
            return View(_dbContext.Users.ToList());
        }

        [HttpGet]
        public IActionResult GetEditPartial(int userID)
        {
            try
            {
                var user = _dbContext.Users.Single(x => x.ID == userID);

                var userData = new UserRegistrationModel
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
                };
                return PartialView("_editUserPartial", userData);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return View("_editUserPartial");
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="updatedUser">The user's info</param>
        /// <returns>
        /// Model Errors to diplay on user update fail 
        /// </returns>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateUser(UserRegistrationModel updatedUser)
        {
            var resetPassword = false;
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                resetPassword = true;
                //hash and store the password
                updatedUser.HashedPassword = _passwordHelper.Hash(updatedUser.Password);
            }
            else
            {
                //set dummy values so validation doesnt complain when we check for modelstate errors
                updatedUser.Password = "blah";
                updatedUser.HashedPassword = "blah";
            }

            //dummy values for metadata that doesnt matter since it's on what the user passed in
            updatedUser.CreatedBy = "bah";
            updatedUser.CreatedOn = DateTime.Now;

            //validate input model
            ModelState.Clear();
            TryValidateModel(updatedUser);
            if (!ModelState.IsValid)
            {
                //if invalid, return validation errors
                updatedUser.Password = "";
                updatedUser.HashedPassword = ""; 
                return Json(new { error = true, html = this.RenderViewAsync("_editUserPartial", updatedUser).Result });
            }


            // Look for the user
            var user = _dbContext.Users.SingleOrDefault(x => x.ID == updatedUser.ID);

            //make sure we arent revoking admin privs for the only active admin in the system! this also covers the scenario where we are inactivating the last user
            if(user.IsAdmin && _dbContext.Users.Where(x => x.IsActive).Count(x => x.IsAdmin) == 1)
            {
                //if invalid, return validation errors
                ModelState.AddModelError("IsAdmin", "Warning - this user is the last admin in the system! You must create another account with admin access before you can deactivate this account's admin status.");
                updatedUser.Password = "";
                updatedUser.HashedPassword = "";
                return Json(new { error = true, html = this.RenderViewAsync("_editUserPartial", updatedUser).Result });
            }

            //update metadata on the copy we're saving to the DB
            user.RefreshMetadata(user.UserName);

            //update the user's info
            user.UserName = updatedUser.UserName;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.IsAdmin = updatedUser.IsAdmin;
            user.IsActive = updatedUser.IsActive;
            if (resetPassword)
                user.HashedPassword = updatedUser.HashedPassword;//already computed, so no need to do so again

            //persist to database
            _dbContext.Users.Update(user);
            _dbContext.SaveChanges();

            //refresh the page
            updatedUser.Password = "";
            updatedUser.HashedPassword = "";
            updatedUser.StatusMessage = "Save Successful!";
            return Json(new { error = false, html = this.RenderViewAsync("_editUserPartial", updatedUser).Result });
        }
    }
}
