using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeasy.Models.AccountViewModels
{
    /// <summary>
    /// ChangeroleViewModel class.
    /// </summary>
    public class ChangeRoleViewModel
    {
        /// <summary>
        /// Id Getter and Setter.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// FirstName Getter and Setter.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// LastName Getter and Setter.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Role Getter and Setter.
        /// </summary>
        [Display(Name = "Current Role:  ")]
        public string Role { get; set; }
        /// <summary>
        /// Name Getter and Setter.
        /// </summary>
        public string Name { get; set; }
    }
}