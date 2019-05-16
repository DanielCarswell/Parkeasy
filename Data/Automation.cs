using System;
using System.Linq;
using System.Collections.Generic;
using Parkeasy.Models;

namespace Parkeasy.Data
{
    public class Automation
    {
        private readonly ApplicationDbContext _context;

        public Automation(ApplicationDbContext context)
        {
            _context = context;
            UpdateSlotData();
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