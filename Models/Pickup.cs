using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Parkeasy.Models
{
    /// <summary>
    /// Pickup Class for handling driver pickups and dropoffs.
    /// </summary>
    [Table("Pick Ups")]
    public class Pickup
    {
        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// PickupDate Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Pickup Date And Time")]
        public DateTime PickupDate { get; set; }
        /// <summary>
        /// Location Getter And Setter.
        /// </summary>
        [Required]
        public string Location{ get; set; }
        /// <summary>
        /// Status Getter and Setter.
        /// </summary>
        public string Status{ get; set; }
    }
}