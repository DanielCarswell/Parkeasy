using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Models;

namespace Parkeasy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        /// <summary>
        /// Initialises Bookings Table in Database.
        /// </summary>
        public DbSet<Booking> Bookings { get; set; }
        /// <summary>
        /// Initialises Vehicles Table in Database.
        /// </summary>
        public DbSet<Vehicle> Vehicles{ get; set; }
        /// <summary>
        /// Initialises Flights Table in Database.
        /// </summary>
        public DbSet<Flight> Flights { get; set; }
        /// <summary>
        /// Initialises Invoices Table in Database.
        /// </summary>
        public DbSet<Invoice> Invoices{ get; set; }
        /// <summary>
        /// Initialises Slots Table in Database.
        /// </summary>
        public DbSet<Slot> Slots{ get; set; }
    }
}
