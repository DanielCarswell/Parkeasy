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
    public class MonthlyBookingReport
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public MonthlyBookingReport(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// NoOfBookings Getter and Setter.
        /// </summary>
        [Display(Name = "Number Of Bookings")]
        public int NoOfBookings{ get; set; }
        /// <summary>
        /// TotalAmount Getter and Setter.
        /// </summary>
        [Display(Name="Income")]
        public double TotalAmount { get; set; }
        /// <summary>
        /// ReportDay Getter and Setter.
        /// </summary>
        [Display(Name = "Day ")]
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