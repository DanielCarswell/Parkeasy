using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.ManageViewModels
{
    /// <summary>
    /// ChangePasswordViewModel Class.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// OldPassword Getter and Setter.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        /// <summary>
        /// NewPassword Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
        
        /// <summary>
        /// ConfirmPassword Getter and Setter.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// StatusMessage Getter and Setter.
        /// </summary>
        public string StatusMessage { get; set; }
    }
}
