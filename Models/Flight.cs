using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Post information with relationships.
    /// </summary>
    [Table("Flight")]
    public class Flight
    {
        /// <summary>
        /// Flight Blank Constructor.
        /// </summary>
        public Flight(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [ForeignKey("Booking")]
        public int? Id { get; set; }
        /// <summary>
        /// DepartureNumber Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string DepartureNumber { get; set; }
        /// <summary>
        /// ReturnNumber Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string ReturnNumber { get; set; }
        /// <summary>
        /// DepartureDateTime Getter and Setter.
        /// </summary>
        public DateTime DepartureDateTime { get; set; }
        /// <summary>
        /// ReturnDateTime Getter and Setter.
        /// </summary>
        public DateTime ReturnDateTime { get; set; }
        /// <summary>
        /// ErrorMessage Getter and Setter.
        /// </summary>
        public string ErrorMessage{ get; set; }
        /// <summary>
        /// Destination Getter and Setter.
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string Destination { get; set; }

        //Navigational propertie for Flight and Booking Relationship.
        public virtual Booking Booking { get; set; }
    }
}