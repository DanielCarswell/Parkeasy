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
    [Table("Car Valeting Report")]
    public class ValetingReport
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public ValetingReport(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Model Getter and Setter.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// Registration Getter and Setter.
        /// </summary>
        public String Registration { get; set; }
        /// <summary>
        /// BookingId Getter and Setter.
        /// </summary>
        public int BookingId { get; set; }
        /// <summary>
        /// ReportDay Getter and Setter.
        /// </summary>
        public int ReportDay { get; set; }
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