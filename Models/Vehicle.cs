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
        public string Registration { get; set; }
        /// <summary>
        /// Model Getter and Setter.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// Colour Getter and Setter.
        /// </summary>
        public string Colour { get; set; }
        /// <summary>
        /// Travellers Getter and Setter.
        /// </summary>
        public int Travellers { get; set; }
        /// <summary>
        /// Navigational propertie for Vehicle and Booking Relationship.
        /// </summary>
        public virtual Booking Booking { get; set; }
    }
}