using JobTracker.Data.Entities.Login;
using System.ComponentModel.DataAnnotations;

namespace JobTracker.Models.Login
{
    public class UserRegistrationModel : User
    {
        /// <summary>
        /// The unhashed password entered by the user
        /// </summary>
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// True if the login should be persisted across browser sessions
        /// </summary>
        [Required]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Helper for storing any status message we care to show the user
        /// </summary>
        public string StatusMessage { get; set; }
    }
}
