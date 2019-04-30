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
        #region FlightDetails
        /// <summary>
        /// DepartureNumber Getter and Setter.
        /// </summary>
        public string DepartureNumber { get; set; }
        /// <summary>
        /// ReturnNumber Getter and Setter.
        /// </summary>
        public string ReturnNumber { get; set; }
        /// <summary>
        /// DepartureDateTime Getter and Setter.
        /// </summary>
        public DateTime DepartureDateTime { get; set; }
        /// <summary>
        /// ReturnDateTime Getter and Setter.
        /// </summary>
        public DateTime ReturnDateTime { get; set; }
        /// <summary>
        /// Destination Getter and Setter.
        /// </summary>
        public string Destination { get; set; }

        #endregion
        #region Vehicle Details
        /// <summary>
        /// Registration Getter and Setter.
        /// </summary>
        public int Registration { get; set; }
        /// <summary>
        /// Model Getter and Setter.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// Colour Getter and Setter.
        /// </summary>
        public string Colour { get; set; }
        /// <summary>
        /// Travellers Getter and Setter.
        /// </summary>
        public int Travellers { get; set; }

        #endregion
    }
}