using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassroomSchedulerCore.Data;
using ClassroomSchedulerCore.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ClassroomSchedulerCore.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString, DateTime? startDate, DateTime? endDate, int? roomId)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .AsQueryable();
            
            // Apply filters if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                bookingsQuery = bookingsQuery.Where(b => 
                    b.Title.Contains(searchString) || 
                    (b.Description != null && b.Description.Contains(searchString)) ||
                    (b.Room != null && b.Room.Name.Contains(searchString)));
            }
            
            if (startDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.StartTime.Date >= startDate.Value.Date);
            }
            
            if (endDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.StartTime.Date <= endDate.Value.Date);
            }
            
            if (roomId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.RoomId == roomId.Value);
            }
            
            // Order results by start time (most recent first)
            bookingsQuery = bookingsQuery.OrderByDescending(b => b.StartTime);
            
            // Pass search parameters to the view for form rehydration
            ViewBag.SearchString = searchString;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.RoomId = roomId;
            
            // Load rooms for the room filter dropdown
            ViewBag.Rooms = await _context.Rooms.OrderBy(r => r.Name).ToListAsync();
            
            return View(await bookingsQuery.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        [Authorize(Policy = "StudentBookingPolicy")]
        public IActionResult Create(int? roomId)
        {
            try
            {
                // Get all rooms for dropdown
                var rooms = _context.Rooms.OrderBy(r => r.Name).ToList();
                ViewBag.Rooms = rooms;
                
                // Current user's ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                // Default start time (rounded to the nearest half hour)
                var now = DateTime.Now;
                var roundedMinutes = now.Minute >= 30 ? 30 : 0;
                var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, roundedMinutes, 0);
                
                // Create default booking
                var booking = new Booking
                {
                    StartTime = startTime,
                    EndTime = startTime.AddHours(1),
                    Status = BookingStatus.Reserved,
                    UserId = userId ?? string.Empty,
                    Notes = string.Empty,
                    Description = string.Empty,
                    Title = string.Empty
                };
                
                // Pre-select room if specified
                if (roomId.HasValue)
                {
                    booking.RoomId = roomId.Value;
                }
                
                return View(booking);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in Create GET: {ex.Message}");
                
                // Return with empty ViewBag
                ViewBag.Rooms = new List<Room>();
                return View(new Booking
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddHours(1),
                    Status = BookingStatus.Reserved
                });
            }
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StudentBookingPolicy")]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                // Set the current user ID
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                booking.UserId = currentUserId ?? string.Empty;
                
                // Set creation timestamp
                booking.CreatedAt = DateTime.Now;
                
                // Initialize nullable fields to prevent DB errors
                booking.Notes = booking.Notes ?? string.Empty;
                booking.Description = booking.Description ?? string.Empty;
                
                // Sync IsEmergency flag with Status
                booking.IsEmergency = booking.Status == BookingStatus.Emergency;
                
                // Explicitly load the Room and User entities to satisfy model validation
                booking.Room = await _context.Rooms.FindAsync(booking.RoomId);
                booking.User = await _context.Users.FindAsync(booking.UserId);
                
                // Basic validation
                if (booking.RoomId <= 0)
                {
                    ModelState.AddModelError("RoomId", "Please select a room.");
                }
                
                if (string.IsNullOrEmpty(booking.Title))
                {
                    ModelState.AddModelError("Title", "Please provide a title for the booking.");
                }
                
                if (booking.EndTime <= booking.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time.");
                }
                
                if (ModelState.IsValid)
                {
                    // Check for booking conflicts
                    bool hasConflict = await CheckForBookingConflict(booking);
                    
                    if (hasConflict && booking.Status != BookingStatus.Emergency)
                    {
                        ModelState.AddModelError(string.Empty, "This time slot conflicts with an existing booking. Please choose another time or use emergency booking.");
                    }
                    else
                    {
                        // Handle emergency booking conflicts
                        if (booking.Status == BookingStatus.Emergency && hasConflict)
                        {
                            await HandleEmergencyOverride(booking);
                        }
                        
                        // Save the booking
                        _context.Add(booking);
                        await _context.SaveChangesAsync();
                        
                        // Log the action
                        _context.AuditLogs.Add(new AuditLog
                        {
                            UserId = currentUserId,
                            UserName = User.Identity?.Name ?? "unknown",
                            Action = "Create",
                            EntityName = "Booking",
                            EntityId = booking.Id,
                            Details = $"Created {(booking.IsEmergency ? "emergency " : "")}booking '{booking.Title}'",
                            Timestamp = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                        
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error creating booking: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while creating your booking. Please try again.");
            }
            
            // If we get here, something went wrong - redisplay the form
            var rooms = _context.Rooms.OrderBy(r => r.Name).ToList();
            ViewBag.Rooms = rooms;
            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Policy = "StudentBookingPolicy")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Get all rooms for dropdown
            var rooms = _context.Rooms.OrderBy(r => r.Name).ToList();
            ViewBag.Rooms = rooms;
            
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name", booking.RoomId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StudentBookingPolicy")]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            // Only admins or the creator can edit bookings
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && booking.UserId != userId)
            {
                return Forbid();
            }

            // Initialize nullable fields
            booking.Notes = booking.Notes ?? string.Empty;
            booking.Description = booking.Description ?? string.Empty;
            booking.UpdatedAt = DateTime.Now;
            
            // Sync IsEmergency flag with Status
            booking.IsEmergency = booking.Status == BookingStatus.Emergency;
            
            // Explicitly load the Room and User entities to satisfy model validation
            booking.Room = await _context.Rooms.FindAsync(booking.RoomId);
            booking.User = await _context.Users.FindAsync(booking.UserId);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name", booking.RoomId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Policy = "StudentBookingPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StudentBookingPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
        
        private async Task<bool> CheckForBookingConflict(Booking booking, int? excludeBookingId = null)
        {
            // Look for any booking that overlaps with the requested time slot
            var query = _context.Bookings
                .Where(b => b.RoomId == booking.RoomId &&
                           ((booking.StartTime >= b.StartTime && booking.StartTime < b.EndTime) ||
                            (booking.EndTime > b.StartTime && booking.EndTime <= b.EndTime) ||
                            (booking.StartTime <= b.StartTime && booking.EndTime >= b.EndTime)));
            
            // Exclude the current booking if we're editing
            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }
            
            return await query.AnyAsync();
        }
        
        private async Task HandleEmergencyOverride(Booking emergencyBooking)
        {
            // Find all conflicting bookings
            var conflictingBookings = await _context.Bookings
                .Where(b => b.RoomId == emergencyBooking.RoomId &&
                           ((emergencyBooking.StartTime >= b.StartTime && emergencyBooking.StartTime < b.EndTime) ||
                            (emergencyBooking.EndTime > b.StartTime && emergencyBooking.EndTime <= b.EndTime) ||
                            (emergencyBooking.StartTime <= b.StartTime && emergencyBooking.EndTime >= b.EndTime)))
                .ToListAsync();
            
            // Handle conflicts - add a note to the conflicting bookings
            foreach (var booking in conflictingBookings)
            {
                // Don't override the current booking if we're editing
                if (booking.Id == emergencyBooking.Id)
                    continue;
                    
                // Initialize Notes with empty string if null before appending to it
                booking.Notes = booking.Notes ?? string.Empty;
                booking.Notes += $"\n[EMERGENCY OVERRIDE] This booking was overridden by an emergency booking '{emergencyBooking.Title}' at {DateTime.Now}.";
                _context.Entry(booking).State = EntityState.Modified;
            }
            
            // Save all the changes to conflicting bookings
            await _context.SaveChangesAsync();
        }
        
        // GET: Bookings/CreateForRoom
        [Authorize(Policy = "StudentBookingPolicy")]
        public IActionResult CreateForRoom(int roomId)
        {
            // Redirect to the Create action with the roomId parameter
            return RedirectToAction(nameof(Create), new { roomId });
        }
        
        // GET: Bookings/MyBookings
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // User is not authenticated
            }
            
            // Get all bookings for the current user
            var userBookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.StartTime) // Show upcoming bookings first
                .ToListAsync();
                
            // Group bookings by day for easier viewing
            var groupedBookings = userBookings
                .GroupBy(b => b.StartTime.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
                
            ViewBag.GroupedBookings = groupedBookings;
            ViewBag.TotalBookings = userBookings.Count;
            
            return View(userBookings);
        }
    }
}
