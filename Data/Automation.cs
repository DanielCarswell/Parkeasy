using System;
using System.Linq;
using System.Collections.Generic;
using Parkeasy.Models;
using Parkeasy.Services;

namespace Parkeasy.Data
{
    public class Automation
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public Automation(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
            UpdateSlotData();
            ReminderInvoice();
        }

        public void ReminderInvoice()
        {
            var bookings = _context.Bookings;

            foreach(Booking b in bookings)
            {
                if(b.ReminderSent.Equals(true) || !b.Status.Equals("Booked"))
                    continue;
                
                if(b.DepartureDate.Day == DateTime.Now.Day && b.ReminderSent.Equals(false))
                {
                        int slotId = _context.Slots.Find(b.SlotId).Id;
                     _emailSender.SendEmailAsync(_context.Users.Where(u => u.Id == b.ApplicationUserId).FirstOrDefault().Email,
                    "Parkeasy - Booking Reminder", 
                    "This is a reminder that you have a booking made for today, you are assigned to Slot " 
                    + slotId.ToString());

                    Invoice newInvoice = new Invoice
                    {
                        InvoiceBody = "This is a reminder that you have a booking made for today, you are assigned to Slot " + slotId.ToString(),
                        InvoiceType = "Booking Reminder"
                    };

                    b.ReminderSent = true;

                    _context.Bookings.Update(b);
                    _context.Invoices.Add(newInvoice);
                    _context.SaveChanges();
                }

            }
        }

        public void UpdateSlotData()
        {
            Booking nextBooking = new Booking();
            var slots = _context.Slots;

            foreach (Slot s in slots)
            {
                if (!s.Status.Equals("Checked Out"))
                    continue;

                Booking updateOldBooking = _context.Bookings.FirstOrDefault(b => b.Id == s.LastBookingsId);
                updateOldBooking.Status = "Complete";

                _context.Update(updateOldBooking);
                _context.SaveChanges();

                if (s.Bookings.Count == 0)
                    s.Bookings = _context.Bookings.Where(b => b.SlotId == s.Id).ToList();

                foreach (Booking b in s.Bookings)
                {
                    if (!b.Status.Equals("Booked"))
                        continue;

                    if (b.DepartureDate >= DateTime.Now && b.DepartureDate < nextBooking.DepartureDate)
                        nextBooking = b;
                }

                s.LastBookingsId.Equals(nextBooking.Id);
                s.Arrived.Equals(null);
                s.DaysOverCheckout = 0;
                s.Status = "Reserved";
                s.ToBeAvailable = nextBooking.ReturnDate;


                _context.Slots.Update(s);
                _context.SaveChanges();
            }
        }
    }
}