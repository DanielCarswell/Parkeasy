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
        public Flight Flight{ get; set; }
        public Vehicle Vehicle{ get; set; }
        public Booking Booking{ get; set; }
        public int FlightId { get; set; }
        public int VehicleId { get; set; }
        public int BookingId { get; set; }
        public int Charge{ get; set; }
    }
}