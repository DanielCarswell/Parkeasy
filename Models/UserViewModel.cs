using System.Web;
using System.Linq;
using Parkeasy.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// UserViewModel class.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Id Getter and Setter.
        /// </summary>
        [Display(Name = "User Id")]
        public string Id { get; set; }
        /// <summary>
        /// Email Getter and Setter.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        /// <summary>
        /// FirstName Getter and Setter.
        /// </summary>
        [Display(Name = "First Name")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {1} characters long", MinimumLength = 2)]
        public string FirstName { get; set; }
        /// <summary>
        /// LastName Getter and Setter.
        /// </summary>
        [Display(Name = "Last Name")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {1} characters long", MinimumLength = 2)]
        public string LastName { get; set; }
        /// <summary>
        /// Password Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        /// <summary>
        /// Role Getter and Setter.
        /// </summary>
        public string Role { get; set; }
    }
}