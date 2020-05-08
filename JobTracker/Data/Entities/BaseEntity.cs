using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobTracker.Data.Entities
{
    public abstract class BaseEntity
    {
        //TODO: Add data annotations specifying sizes so we dont use varchar(max)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} must be no more than {1} characters")]
        public string CreatedBy { get; set; }

        public DateTime? LastModifiedOn { get; set; }
        
        [StringLength(100, ErrorMessage = "{0} must be no more than {1} characters")]
        public string LastModifiedBy { get; set; }

        /// <summary>
        /// Helper method to automatically update the Created/Last Modified metadata
        /// </summary>
        /// <param name="userName">The login for the user making the change</param>
        public void RefreshMetadata(string userName)
        {
            if (string.IsNullOrEmpty(CreatedBy))
            {
                CreatedBy = userName;
                CreatedOn = DateTime.Now;
            }
            else
            {
                LastModifiedBy = userName;
                LastModifiedOn = DateTime.Now;
            }
        }
    }
}
