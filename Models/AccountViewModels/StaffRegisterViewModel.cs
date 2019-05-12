using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.AccountViewModels
{
    public class StaffRegisterViewModel
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public StaffRegisterViewModel(){}
        /// <summary>
        /// Email Getter and Setter.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        /// <summary>
        /// Password Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        /// <summary>
        /// ConfirmPassword Getter and Setter.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        /// <summary>
        /// FirstName Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string FirstName { get; set; }
        /// <summary>
        /// LastName Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string LastName { get; set; }
        /// <summary>
        /// JobTitle Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Job Title")]
        public string Name { get; set; }
        /// <summary>
        /// Qualification Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Current Qualification")]
        public string Qualification { get; set; }
        /// <summary>
        /// EmergencyContact Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Emergency Contact No")]
        public string EmergencyContact { get; set; }
    }
}
