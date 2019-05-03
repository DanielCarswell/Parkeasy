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
    public class UserViewModel
    {
        //Private Attributes.
        private readonly UserManager<ApplicationUser> _userManager;

        //Properties.
        [Display(Name = "User Id")]
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "First Name")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {1} characters long", MinimumLength = 2)]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [StringLength(30, ErrorMessage = "The {0} must be atleast {1} characters long", MinimumLength = 2)]
        public string LastName { get; set; }
        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /*[NotMapped]
        public string CurrentRole
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUser>();

                }

                return _userManager.GetRoles(Id).FirstOrDefault();
            }
        }*/
    }
}