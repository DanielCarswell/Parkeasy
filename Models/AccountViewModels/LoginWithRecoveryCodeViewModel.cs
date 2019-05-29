using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.AccountViewModels
{
    /// <summary>
    /// LoginWithRecoveryCodeViewModel class.
    /// </summary>
    public class LoginWithRecoveryCodeViewModel
    {
        /// <summary>
        /// RecoveryCode Getter and Setter.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }
}
