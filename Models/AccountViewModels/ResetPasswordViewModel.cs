using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.AccountViewModels
{
    /// <summary>
    /// ResetPasswordViewModel Class.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Email Getter and Setter.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        /// <summary>
        /// Password Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /// <summary>
        /// ConfirmPassword Getter and Setter.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        /// <summary>
        /// Code Getter and Setter.
        /// </summary>
        public string Code { get; set; }
    }
}
