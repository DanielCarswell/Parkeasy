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
        public string Model { get; set; }
        public String Registration { get; set; }
        public int BookingId { get; set; }
        public int ReportDay { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
    }
}