using System.ComponentModel.DataAnnotations;

namespace JobTracker.Data.Entities.Login
{
    public class User : BaseEntity
    {
        //TODO: Add data annotations specifying sizes so we dont use varchar(max)
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
