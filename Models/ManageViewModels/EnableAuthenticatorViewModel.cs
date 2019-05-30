using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Parkeasy.Models.ManageViewModels
{
    /// <summary>
    /// EnableAuthenticatorViewModel Class.
    /// </summary>
    public class EnableAuthenticatorViewModel
    {
        /// <summary>
        /// Code Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }
        /// <summary>
        /// SharedKey Getter and Setter.
        /// </summary>
        [BindNever]
        public string SharedKey { get; set; }

        /// <summary>
        /// AuthenticationUri Getter and Setter.
        /// </summary>
        [BindNever]
        public string AuthenticatorUri { get; set; }
    }
}
