using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Parkeasy.Models.BookingViewModels
{
    /// <summary>
    /// ViewModel class for handling Booking details, will then be passed into the correct models
    /// before being saved into the database. I created this viewmodel so that I could handle class 1 to 
    /// optional dependencies by passing in and creating booking first so booking id existed first for
    /// Vehicle and Flights Id dependencies.
    /// </summary>
    public class BookingViewModel
    {
        /// <summary>
        /// Flight Getter and Setter.
        /// </summary>
        public Flight Flight{ get; set; }
        /// <summary>
        /// Vehicle Getter and Setter.
        /// </summary>
        public Vehicle Vehicle{ get; set; }
        /// <summary>
        /// Booking Getter and Setter.
        /// </summary>
        public Booking Booking{ get; set; }
        /// <summary>
        /// Charge Getter and Setter.
        /// </summary>
        public int Charge{ get; set; }
    }
}