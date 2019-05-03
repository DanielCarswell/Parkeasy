using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    /// <summary>
    /// Slot Class with Properties.
    /// </summary>
    [Table("Slot")]
    public class Slot
    {
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
    }
}