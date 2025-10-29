using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.Models;

namespace CinemaTicketSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must login to view reservations!";
                return RedirectToAction("Login", "Users");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Screening)
                .ThenInclude(s => s.Cinema)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .ToListAsync();
            return View(reservations);
        }

        // GET: Reservations/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ScreeningId"] = new SelectList(
                await _context.Screenings.Include(s => s.Cinema).ToListAsync(), 
                "ScreeningId", 
                "FilmTitle"
            );
            ViewData["UserId"] = new SelectList(await _context.Users.ToListAsync(), "UserId", "Name");
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScreeningId,UserId,Row,SeatNumber")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                // Check if seat is already reserved
                var existingReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => 
                        r.ScreeningId == reservation.ScreeningId &&
                        r.Row == reservation.Row &&
                        r.SeatNumber == reservation.SeatNumber
                    );

                if (existingReservation != null)
                {
                    ModelState.AddModelError("", "This seat is already reserved!");
                    ViewData["ScreeningId"] = new SelectList(
                        await _context.Screenings.Include(s => s.Cinema).ToListAsync(), 
                        "ScreeningId", 
                        "FilmTitle", 
                        reservation.ScreeningId
                    );
                    ViewData["UserId"] = new SelectList(await _context.Users.ToListAsync(), "UserId", "Name", reservation.UserId);
                    return View(reservation);
                }

                // Validate row and seat number against cinema dimensions
                var screening = await _context.Screenings.Include(s => s.Cinema).FirstOrDefaultAsync(s => s.ScreeningId == reservation.ScreeningId);
                if (screening != null)
                {
                    if (reservation.Row < 1 || reservation.Row > screening.Cinema.Rows)
                    {
                        ModelState.AddModelError("Row", $"Row must be between 1 and {screening.Cinema.Rows}");
                    }
                    if (reservation.SeatNumber < 1 || reservation.SeatNumber > screening.Cinema.SeatsPerRow)
                    {
                        ModelState.AddModelError("SeatNumber", $"Seat number must be between 1 and {screening.Cinema.SeatsPerRow}");
                    }

                    if (!ModelState.IsValid)
                    {
                        ViewData["ScreeningId"] = new SelectList(
                            await _context.Screenings.Include(s => s.Cinema).ToListAsync(), 
                            "ScreeningId", 
                            "FilmTitle", 
                            reservation.ScreeningId
                        );
                        ViewData["UserId"] = new SelectList(await _context.Users.ToListAsync(), "UserId", "Name", reservation.UserId);
                        return View(reservation);
                    }
                }

                reservation.ReservationTime = DateTime.Now;
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reservation created successfully!";
                return RedirectToAction("Index");
            }

            ViewData["ScreeningId"] = new SelectList(
                await _context.Screenings.Include(s => s.Cinema).ToListAsync(), 
                "ScreeningId", 
                "FilmTitle", 
                reservation.ScreeningId
            );
            ViewData["UserId"] = new SelectList(await _context.Users.ToListAsync(), "UserId", "Name", reservation.UserId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Screening)
                .ThenInclude(s => s.Cinema)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
                return NotFound();

            // Check if user owns this reservation
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != reservation.UserId)
            {
                TempData["ErrorMessage"] = "You can only cancel your own reservations!";
                return RedirectToAction("Index");
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                // Verify user owns this reservation
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != reservation.UserId)
                {
                    TempData["ErrorMessage"] = "You can only cancel your own reservations!";
                    return RedirectToAction("Index");
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "✓ Reservation cancelled successfully!";
            }

            return RedirectToAction("Index");
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
