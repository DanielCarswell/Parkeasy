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
    public class ReportDate
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public ReportDate(){}

        /// <summary>
        /// Id Getter And Setter.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Name Getter and Setter.
        /// </summary>
        public string ReportType { get; set; }
        /// <summary>
        /// Price Getter and Setter.
        /// </summary>
        public double Price { get; set; }
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