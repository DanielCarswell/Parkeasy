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
    [Table("Booking Report")]
    public class BookingReport
    {
        /// <summary>
        /// Blank Constructor.
        /// </summary>
        public BookingReport(){}

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
        [Display(Name="Contact")]
        public string ContactDetails { get; set; }
        public string Registration { get; set; }
        public string Model { get; set; }
        [Display(Name="Arrival")]
        public DateTime ArrivalTime { get; set; }
        [Display(Name="Departure")]
        public DateTime DepartureTime { get; set; }
        public int BookingId { get; set; }
        public int ReportDay { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
    }
}