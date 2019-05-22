using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Data;
using Parkeasy.Models;

namespace Parkeasy.Controllers
{
    public class SlotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SlotController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Slot
        public async Task<IActionResult> Index()
        {
            return View(await _context.Slots.ToListAsync());
        }

        private bool SlotExists(int id)
        {
            return _context.Slots.Any(e => e.Id == id);
        }
    }
}
