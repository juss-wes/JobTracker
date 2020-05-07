﻿using JobTracker.Data.Entities.Login;
using System.ComponentModel.DataAnnotations;

namespace JobTracker.Models.Login
{
    public class LoginModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string UserName { get; set; }
        public bool RememberMe { get; set; }
    }
}