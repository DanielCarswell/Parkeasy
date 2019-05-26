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
    public class TurnoverReport
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public TurnoverReport(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Name Getter and Setter.
        /// </summary>
        public int BookingId { get; set; }
        [Display(Name="Contact")]
        public DateTime BookingDate { get; set; }
        public double Price { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
    }
}