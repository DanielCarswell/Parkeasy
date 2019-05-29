using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Parkeasy.Models.BookingViewModels
{
    /// <summary>
    /// UserBookingViewModel class.
    /// </summary>
    public class UserBookingViewModel
    {
        /// <summary>
        /// Id Getter and Setter.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// FirstName Getter and Setter.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// LastName Getter and Setter.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// StartDate Getter and Setter.
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// EndDate Getter and Setter.
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// SlotNumber Getter and Setter.
        /// </summary>
        public int SlotNumber { get; set; }
        /// <summary>
        /// Paid Getter and Setter.
        /// </summary>
        public double Paid { get; set; }
    }
}