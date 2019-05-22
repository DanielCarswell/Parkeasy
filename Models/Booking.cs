using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Parkeasy.Models
{
    /// <summary>
    /// Booking information with relationships.
    /// </summary>
    [Table("Booking")]
    public class Booking
    {
        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// DepartureDate Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }
        /// <summary>
        /// ReturnDate Getter And Setter.
        /// </summary>
        [Required]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate{ get; set; }
        /// <summary>
        /// BookedAt Getter and Setter.
        /// </summary>
        public DateTime BookedAt{ get; set; }
        /// <summary>
        /// Duration Getter And Setter.
        /// </summary>
        [Required]
        public int Duration { get; set; }
        /// <summary>
        /// Status Getter And Setter.
        /// </summary>
        [Required]
        public string Status { get; set; }
        [Required]
        public double Price { get; set; }
        public string PaymentId{ get; set; }
        [Display(Name = "Add Full Wash To Booking")]
        public bool Servicing { get; set; }
        public bool ReminderSent { get; set; }

        //Contains 1:M Relationship with ApplicationUser. (This is the many side)
        /// <summary>
        /// Relationship Properties for Booking With ApplicationUser.
        /// </summary>
        [InverseProperty("AspNetUsers")]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        //Contains 1:M Relationship with ApplicationUser. (This is the many side)
        /// <summary>
        /// Relationship Properties for Booking With ApplicationUser.
        /// </summary>
        [Display(Name = "Slot")]
        [InverseProperty("Slot")]
        public int SlotId { get; set; }
        public virtual Slot Slot { get; set; }
    }
}