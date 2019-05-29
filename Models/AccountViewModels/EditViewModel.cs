using System.Web;
using System.Linq;
using Parkeasy.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models.AccountViewModels
{
    /// <summary>
    /// EditViewModel Class.
    /// </summary>
    public class EditViewModel
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
        [Required]
        [Display(Name = "First Name")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {1} characters long", MinimumLength = 2)]
        public string FirstName { get; set; }
        /// <summary>
        /// LastName Getter and Setter.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {1} characters long", MinimumLength = 2)]
        public string LastName { get; set; }
        /// <summary>
        /// Telephone Getter and Setter.
        /// </summary>
        public string Telephone { get; set; }
        /// <summary>
        /// Address Getter and Setter.
        /// </summary>
        [Required]
        public string Address { get; set; }
        /// <summary>
        /// PostCode Getter and Setter.
        /// </summary>
        [Required]
        public string PostCode { get; set; }
    }
}