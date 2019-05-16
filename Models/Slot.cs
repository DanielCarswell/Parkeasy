using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace Parkeasy.Models
{
    /// <summary>
    /// Slot Class with Properties.
    /// </summary>
    [Table("Slot")]
    public class Slot
    {
        /// <summary>
        /// Slot Blank Constructor.
        /// </summary>
        public Slot()
        {
            Bookings = new List<Booking>();    
        }

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Status Getter and Setter.
        /// </summary>
        /// <value>Available/Reserved/Occupied</value>
        public string Status { get; set; }
        /// <summary>
        /// ToBeAvailable Getter and Setter.
        /// </summary>
        /// <value>05/05/2019 00:20:00</value>
        public DateTime? ToBeAvailable { get; set; }
        public string Arrived { get; set; }
        public int DaysOverCheckout { get; set; }
        public int LastBookingsId { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}