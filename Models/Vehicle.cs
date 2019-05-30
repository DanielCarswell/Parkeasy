using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Vehicle Class with Properties.
    /// </summary>
    [Table("Vehicle")]
    public class Vehicle
    {
        /// <summary>
        /// Vehicle Blank Constructor.
        /// </summary>
        public Vehicle(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [ForeignKey("Booking")]
        public int? Id { get; set; }
        /// <summary>
        /// Registration Getter and Setter.
        /// </summary>
        [Required]
        public string Registration { get; set; }
        /// <summary>
        /// Model Getter and Setter.
        /// </summary>
        [Required]
        public string Model { get; set; }
        /// <summary>
        /// Colour Getter and Setter.
        /// </summary>
        [Required]
        public string Colour { get; set; }
        /// <summary>
        /// Travellers Getter and Setter.
        /// </summary>
        [Required]
        [Range(1, 8, ErrorMessage = "Please enter a value bigger than {1} and less than {2}")]
        public int Travellers { get; set; }
        /// <summary>
        /// Navigational propertie for Vehicle and Booking Relationship.
        /// </summary>
        public virtual Booking Booking { get; set; }
    }
}