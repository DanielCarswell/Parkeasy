using System;
using System.ComponentModel.DataAnnotations;

namespace Parkeasy.Models
{
    public class Pricing
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Cost Per Day: ")]
        public double PerDay { get; set; }
        [Required]
        [Display(Name = "Servicing Cost: ")]
        public double ServicingCost { get; set; }

        public Pricing(){}
    }
}