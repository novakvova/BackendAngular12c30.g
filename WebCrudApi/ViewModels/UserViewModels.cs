using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebCrudApi.Helpers;

namespace WebCrudApi.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public int Age { get; set; }
    }
    public class UserAddViewModel
    {
        [Required(ErrorMessage = "Can't be empty")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Can't be empty")]
        public string LastName { get; set; }

        [CustomEmail(ErrorMessage = "Already exist")]
        [Required(ErrorMessage = "Can't be empty")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Can't be empty")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).{6,24}$", ErrorMessage = "Password must be at least 6 characters and contain digits, upper and lower case")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Can't be empty")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Can't be empty")]
        public int Age { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required(ErrorMessage = "Cant't be empty")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).{6,24}$", ErrorMessage = "Password must be at least 6 characters and contain digits, upper and lower case")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Cant't be empty")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).{6,24}$", ErrorMessage = "Password must be at least 6 characters and contain digits, upper and lower case")]
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
    public class ConfirmEmailViewModel
    {
        [Required(ErrorMessage = "Cant't be empty")]
        public string Code { get; set; }
    }

}
