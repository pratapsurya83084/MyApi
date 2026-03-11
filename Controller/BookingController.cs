
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;
using System.Security.Claims;


namespace MyApi.Controllers
{

    [ApiController]
    [Route("api/booking")]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        // private readonly IConfiguration _configuration;
        public BookingController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

        }

        [Authorize]
        [HttpPost("add-booking")]
        public async Task<IActionResult> AddBooking([FromBody] BookingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid data",
                    success = false
                });
            }

            var userId = User.FindFirst("userId")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Allow only Farmer
            if (role != "Farmer")
            {
                return Ok(new
                {
                    message = "Only Farmer can create booking",
                    success = false
                });
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid token",
                    success = false
                });
            }

            var booking = new Models.Booking
            {
                ServiceId = dto.ServiceId,
                CustomerId = Convert.ToInt32(userId),   // from token
                BookingDate = DateTime.UtcNow,
                Status = Models.BookingStatus.Pending,
                Address = dto.Address,
                Notes = dto.Notes
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking created successfully",
                success = true,
                data = booking
            });
        }

        //farmer can view booked services  - userid get  and check in Booking table CustomerId filed compare with userId and return those rows
        [HttpGet("farmer-bookings")]
        public async Task<IActionResult> GetFarmerBookings()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst("userId")?.Value;



            if (role != "Farmer")
            {
                return Unauthorized(new
                {
                    message = "Only Farmer can view booked services",
                    success = false
                });
            }

            var bookings = await _context.Bookings
                            .Where(b => b.CustomerId.ToString() == userId)
                            .ToListAsync();

            return Ok(new
            {
                message = "Bookings fetched successfully",
                success = true,
                data = bookings
            });
        }


        //only admin can delete booking or update
        [HttpPut("admin/update-booking/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, Booking updatedBooking)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    message = "Only Admin can update bookings",
                    success = false
                });
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking not found",
                    success = false
                });
            }

            booking.ServiceId = updatedBooking.ServiceId;
            booking.BookingDate = updatedBooking.BookingDate;
            booking.Status = updatedBooking.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking updated successfully",
                success = true
            });
        }


        [HttpDelete("admin/delete-booking/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    message = "Only Admin can delete bookings",
                    success = false
                });
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking not found",
                    success = false
                });
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking deleted successfully",
                success = true
            });
        }




        //getallbookings for admin view
        [Authorize]
        [HttpGet("getallbookings")]
        public async Task<IActionResult> Getallbookings()
        {

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Only Admin can delete bookings",
                    success = false
                });
            }

            var booking = await _context.Bookings.ToListAsync();

            return Ok(new
            {
                data = booking,
                message = "All bookings fetches successfully",
                success = true
            });

        }


    }
}













