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
        /// Title Getter And Setter.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }
        /// <summary>
        /// Duration Getter And Setter.
        /// </summary>
        [Required]
        public int Duration { get; set; }

        //Contains M:1 Relationship with ApplicationUser. (This is the 1 side)
        /// <summary>
        /// Relationship Properties for Post With ApplicationUser.
        /// </summary>
        [InverseProperty("AspNetUsers")]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        //Contains M:1 Relationship with ApplicationUser. (This is the 1 side)
        /// <summary>
        /// Relationship Properties for Post With ApplicationUser.
        /// </summary>
        [InverseProperty("Flight")]
        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }

        //Contains M:1 Relationship with ApplicationUser. (This is the 1 side)
        /// <summary>
        /// Relationship Properties for Post With ApplicationUser.
        /// </summary>
        [InverseProperty("Vehicle")]
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; }

        /// <summary>
        /// Blank Constructor, Sets Navigational Property  to new List of type  (Class).
        /// </summary>
        public Booking()
        {
             = new List<Comment>();
        }
    }
}