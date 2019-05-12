using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeasy.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class.
    /// <summary>
    /// ApplicationUser Class used for Users data when creating users and during runtime.
    /// </summary>
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
        public string Telephone { get; set; }
        /// <summary>
        /// PostCode Getter And Setter.
        /// </summary>
        public string PostCode { get; set; }
        /// <summary>
        /// RegisteredAt Getter And Setter.
        /// </summary>
        public DateTime? RegisteredAt { get; set; }
        /// <summary>
        /// JobTitle Getter and Setter.
        /// </summary>
        public string JobTitle { get; set; }
        /// <summary>
        /// CurrentQualification Getter and Setter.
        /// </summary>
        public string CurrentQualification { get; set; }
        /// <summary>
        /// EmergencyContact Getter and Setter.
        /// </summary>
        /// <value></value>
        public string EmergencyContact { get; set; }
        //Contains 1:M Relationship with Booking. (This is the one side)
        /// <summary>
        /// Relationship Properties for ApplicationUser With Booking.
        /// </summary>
        public virtual ICollection<Booking> Bookings { get; set; }
        //Contains 1:M Relationship with Invoice. (This is the one side)
        /// <summary>
        /// Relationship Properties for ApplicationUser With Invoice.
        /// </summary>
        public virtual ICollection<Invoice> Invoices { get; set; }
        /// <summary>
        /// Blank Constructor, Sets Navigational Property Bookings to new List of type Booking(Class).
        /// And Navigational Property Invoices to new List of type Invoice(Class).
        /// </summary>
        public ApplicationUser()
        {
            Bookings = new List<Booking>();
            Invoices = new List<Invoice>();
        }
    }
}
