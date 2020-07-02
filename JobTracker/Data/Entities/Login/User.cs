using System.ComponentModel.DataAnnotations;

namespace JobTracker.Data.Entities.Login
{
    public class User : BaseEntity
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name must be no more than {1} characters")]//string formatting automatically occurs at runtime. {0} will inject the property name (eg, FirstName). {1} will inject the MaxLength value. {2} will inject the MinLength value
        public string FirstName { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be no more than {1} characters")]
        public string LastName { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "User name is required")]
        [StringLength(50, ErrorMessage = "User Name must be no more than {1} characters")]
        public string UserName { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
