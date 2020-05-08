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
    }
}
