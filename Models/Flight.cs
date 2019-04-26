using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Post information with relationships.
    /// </summary>
    [Table("Flight")]
    public class Flight{
        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [ForeignKey("Booking")]
        public int? Id { get; set; }
        /// <summary>
        /// DepartureNumber Getter and Setter.
        /// </summary>
        public string DepartureNumber { get; set; }
        /// <summary>
        /// ReturnNumber Getter and Setter.
        /// </summary>
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
        /// Destination Getter and Setter.
        /// </summary>
        public string Destination { get; set; }

        //Navigational propertie for Flight and Booking Relationship.
        public virtual Booking Booking { get; set; }
    }
}