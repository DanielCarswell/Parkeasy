using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Parkeasy.Models.BookingViewModels
{
    /// <summary>
    /// ViewModel class for handling Amending of bookings, this will then be passed into the correct models
    /// before being saved into the database. I created this viewmodel so that I could handle class 1 to 
    /// optional dependencies by passing in and creating booking first so booking id existed first for
    /// Vehicle and Flights Id dependencies.
    /// </summary>
    public class AmendViewModel
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
        /// FlightId Getter and Setter.
        /// </summary>
        public int FlightId { get; set; }
        /// <summary>
        /// VehicleId Getter and Setter.
        /// </summary>
        public int VehicleId { get; set; }
        /// <summary>
        /// BookingId Getter and Setter.
        /// </summary>
        public int BookingId { get; set; }
        /// <summary>
        /// Charge Getter and Setter.
        /// </summary>
        public int Charge{ get; set; }
    }
}