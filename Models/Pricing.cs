using System;
using System.ComponentModel.DataAnnotations;

namespace Parkeasy.Models
{
    /// <summary>
    /// Pricing Class.
    /// </summary>
    public class Pricing
    {
        /// <summary>
        /// Id Getter and Setter.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// PerDay Getter and Setter.
        /// </summary>
        [Required]
        [Display(Name = "Cost Per Day: ")]
        public double PerDay { get; set; }
        /// <summary>
        /// ServicingCost Getter and Setter.
        /// </summary>
        [Required]
        [Display(Name = "Servicing Cost: ")]
        public double ServicingCost { get; set; }

        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public Pricing(){}
    }
}