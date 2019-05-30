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
    [Table("Car Release Report")]
    public class ReleaseReport
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public ReleaseReport(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Name Getter and Setter.
        /// </summary>
        [Display(Name = "Customer")]
        public string Name { get; set; }
        /// <summary>
        /// ContactDetails Getter and Setter.
        /// </summary>
        [Display(Name="Contact")]
        public string ContactDetails { get; set; }
        /// <summary>
        /// Registration Getter and Setter.
        /// </summary>
        public string Registration { get; set; }
        /// <summary>
        /// Model Getter and Setter.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// ArrivalTime Getter and Setter.
        /// </summary>
        [Display(Name="Arrival")]
        public DateTime ArrivalTime { get; set; }
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
        public int BookingId { get; set; }
    }
}