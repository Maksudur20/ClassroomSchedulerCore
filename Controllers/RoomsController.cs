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

namespace ClassroomSchedulerCore.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rooms.ToListAsync());
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (room == null)
            {
                return NotFound();
            }
            
            // Get upcoming bookings for this room and sort them chronologically
            var upcomingBookings = room.Bookings
                .Where(b => b.StartTime >= DateTime.Now)
                .OrderBy(b => b.StartTime)
                .ToList();
            
            // Pass the bookings to the view
            ViewBag.Bookings = upcomingBookings;

            return View(room);
        }

        // GET: Rooms/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Location,Capacity,HasProjector,HasComputers,Description")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                
                // Log the action
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = userId,
                        UserName = User.Identity?.Name ?? "unknown",
                        Action = "Create",
                        EntityName = "Room",
                        EntityId = room.Id,
                        Details = $"Created room {room.Name}",
                        Timestamp = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location,Capacity,HasProjector,HasComputers,Description")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    
                    // Log the action
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        _context.AuditLogs.Add(new AuditLog
                        {
                            UserId = userId,
                            UserName = User.Identity?.Name ?? "unknown",
                            Action = "Update",
                            EntityName = "Room",
                            EntityId = room.Id,
                            Details = $"Updated room {room.Name}",
                            Timestamp = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            _context.Rooms.Remove(room);
            
            // Log the action
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    UserName = User.Identity?.Name ?? "unknown",
                    Action = "Delete",
                    EntityName = "Room",
                    EntityId = id,
                    Details = $"Deleted room {room.Name}",
                    Timestamp = DateTime.Now
                });
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}
