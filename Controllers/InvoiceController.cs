using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Parkeasy.Data;
using Parkeasy.Models;
using Parkeasy.Services;

namespace Parkeasy.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public InvoiceController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Invoice
        public async Task<IActionResult> Index()
        {
            return View(await _context.Invoices.ToListAsync());
        }

        [Authorize(Roles = "Invoice Clerk,Admin,Manager,Customer")]
        public IActionResult SendInvoice()
        {
            var users = _context.Users;
            List<Invoice> model = new List<Invoice>();

            foreach(var u in users)
            {
                model.Add(new Invoice{Email = u.Email});
            }

            ViewData["Emails"] = new SelectList(model, "Email", "Email");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Invoice Clerk,Admin,Manager,Customer")]
        public async Task<IActionResult> SendInvoice(SendInvoiceModel invoice)
        {
            await _emailSender.SendEmailAsync(invoice.Emails.First(), invoice.InvoiceType, invoice.InvoiceBody);

            _context.Invoices.Add(
                new Invoice{Email = invoice.Emails.First(), 
                InvoiceType = invoice.InvoiceType, 
                InvoiceBody = invoice.InvoiceBody});
                
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }
    }
}
