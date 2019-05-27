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
        public double Price { get; set; }
        public int ReportDay { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
    }
}