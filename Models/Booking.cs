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
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Key]
        public int Id { get; set; }
        /// <summary>
        /// Date Getter And Setter.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }
        /// <summary>
        /// Duration Getter And Setter.
        /// </summary>
        [Required]
        public int Duration { get; set; }

        //Contains 1:M Relationship with ApplicationUser. (This is the many side)
        /// <summary>
        /// Relationship Properties for Booking With ApplicationUser.
        /// </summary>
        [InverseProperty("AspNetUsers")]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        //Contains 1:1 Relationship with Flight. (This is the 1 side)
        /// <summary>
        /// Relationship Properties for Booking With Flight.
        /// </summary>
        [InverseProperty("Flight")]
        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }

        //Contains 1:1 Relationship with Vehicle. (This is the 1 side)
        /// <summary>
        /// Relationship Properties for Booking With Vehicle.
        /// </summary>
        [InverseProperty("Vehicle")]
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; }

        //Contains 1:1 Relationship with Payment. (This is the 1 side)
        /// <summary>
        /// Relationship Properties for Booking With Payment.
        /// </summary>
        [InverseProperty("Payment")]
        public int PaymentId { get; set; }
        public virtual Vehicle Payment { get; set; }
    }
}