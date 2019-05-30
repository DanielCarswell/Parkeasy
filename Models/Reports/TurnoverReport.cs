using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Parkeasy.Models.Reports
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
        /// <summary>
        /// BookingDate Getter and Setter.
        /// </summary>
        [Display(Name="Booking Date")]
        public DateTime BookingDate { get; set; }
        /// <summary>
        /// Price Getter and Setter.
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// ReportMonth Getter and Setter.
        /// </summary>
        public int ReportMonth { get; set; }
        /// <summary>
        /// ReportYear Getter and Setter.
        /// </summary>
        public int ReportYear { get; set; }
    }
}