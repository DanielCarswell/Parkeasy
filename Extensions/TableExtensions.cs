using Parkeasy.Models.BookingViewModels;
using Parkeasy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;

namespace Parkeasy.Extensions
{
    /// <summary>
    /// Class containing methods to get data that requires table joins.
    /// </summary>
    public static class TableExtensions
    {
        /// <summary>
        /// Returns a list of a specific users bookings.
        /// </summary>
        /// <param name="userBookings">List of UserBookingViewModel class instances.</param>
        /// <param name="context">Instance of ApplicationDbContext class.</param>
        /// <returns>List of UserBookingViewModel Class Instances.</returns>
        public static async Task GetBookingAndSlotData(this List<UserBookingViewModel> userBookings, ApplicationDbContext context)
        {
            //Adds UserBookingViewModel class instances to userBookings variable ordered by startdate.
            //Gets data by joining users to their respective bookings and associated slots to those bookings.
            userBookings.AddRange((
             from u in context.Users
             join b in context.Bookings on u.Id equals b.ApplicationUserId
             select new UserBookingViewModel
             {
                 Id = b.Id,
                 FirstName = u.FirstName,
                 LastName = u.LastName,
                 StartDate = b.DepartureDate,
                 EndDate = b.ReturnDate,
                 SlotNumber = b.SlotId,
                 Paid = b.Price
             }).OrderBy(o => o.StartDate).ToList());
        }
    }
}    