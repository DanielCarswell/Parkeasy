using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Parkeasy.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// FirstName Getter And Setter.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// LastName Getter And Setter.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Address Getter And Setter.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Telephone Getter And Setter.
        /// </summary>
        public int? Telephone { get; set; }
        /// <summary>
        /// Mobile Getter And Setter.
        /// </summary>
        public int? Mobile { get; set; }
        /// <summary>
        /// PostCode Getter And Setter.
        /// </summary>
        public string PostCode { get; set; }
        /// <summary>
        /// RegisteredAt Getter And Setter.
        /// </summary>
        public DateTime RegisteredAt { get; set; }


        //Contains 1:M Relationship with Booking. (This is the one side)
        /// <summary>
        /// Relationship Properties for ApplicationUser With Booking.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; }

        /// <summary>
        /// Blank Constructor, Sets Navigational Property Bookings to new List of type Booking(Class).
        /// </summary>
        public ApplicationUser()
        {
            Bookings = new List<Booking>();
        }
    }
}
