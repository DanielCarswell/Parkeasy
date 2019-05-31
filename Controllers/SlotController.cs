using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Data;
using Parkeasy.Models;

namespace Parkeasy.Controllers
{
    /// <summary>
    /// Slot Controller
    /// </summary>
    public class SlotController : Controller
    {
        /// <summary>
        /// Global Variable of type ApplicationDbContext class.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Overloaded Constructor.
        /// </summary>
        /// <param name="context">Instance of ApplicationDbContext.</param>
        public SlotController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Index method for displaying all slots and there data.
        /// </summary>
        /// <returns>Index View</returns>
        [Authorize(Roles = "Admin,Manager,Booking Clerk")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Slots.ToListAsync());
        }

        /// <summary>
        /// Checks if a Slot Exists in the database.
        /// </summary>
        /// <param name="id">integer value</param>
        /// <returns>True or False</returns>
        private bool SlotExists(int id)
        {
            return _context.Slots.Any(e => e.Id == id);
        }
    }
}
