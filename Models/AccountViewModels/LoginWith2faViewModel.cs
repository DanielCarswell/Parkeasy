using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.AccountViewModels
{
    /// <summary>
    /// LoginWith2faViewModel Class.
    /// </summary>
    public class LoginWith2faViewModel
    {
        /// <summary>
        /// TwoFactorCode Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; }
        /// <summary>
        /// RememberMachine Getter and Setter.
        /// </summary>
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
        /// <summary>
        /// RememberMe Getter and Setter.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
