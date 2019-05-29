using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.AccountViewModels
{
    /// <summary>
    /// LoginViewModel Class.
    /// </summary>
    public class LoginViewModel
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
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /// <summary>
        /// RememberMe Getter and Setter.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
