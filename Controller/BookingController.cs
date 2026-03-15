
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
        [Authorize]
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

            var bookings = await (from b in _context.Bookings
                                  join s in _context.Services
                                      on b.ServiceId equals s.Id
                                  join u in _context.Users
                                      on b.CustomerId equals u.Id
                                  where b.CustomerId.ToString() == userId
                                  select new
                                  {
                                      BookingId = b.Id,
                                      BookingStatus = b.Status,
                                      Address = b.Address,
                                      Notes = b.Notes,
                                      BookingDate = b.BookingDate,

                                      // Service Details
                                      ServiceId = s.Id,
                                      ServiceTitle = s.Title,
                                      ServiceDescription = s.Description,
                                      Price = s.Price,

                                      // User Details
                                      CustomerId = u.Id,
                                      CustomerName = u.Username,
                                      CustomerEmail = u.Email,
                                      CustomerMobile = u.MobileNumber
                                  }).ToListAsync();

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
                return Ok(new
                {
                    message = "Unauthorized Access , Please Login as Admin",
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
        // [Authorize]
        // [HttpGet("getallbookings")]
        // public async Task<IActionResult> Getallbookings()
        // {
        //     var userId = User.FindFirst("userId")?.Value;
        //     var role = User.FindFirst(ClaimTypes.Role)?.Value;

        //     if (role != "Admin")
        //     {
        //         return Ok(new
        //         {
        //             message = "Unauthorized Access , Please Login as Admin",
        //             success = false
        //         });
        //     }

        //     //find customerId in users and find serviceId in services
        //     // var booking = await _context.Bookings.ToListAsync();
        //     var booking = await (from b in _context.Bookings
        //                          join u in _context.Users
        //                             on b.CustomerId equals u.Id
        //                          join s in _context.Services
        //                             on b.ServiceId equals s.Id
        //                          select new
        //                          {
        //                              BookingId = b.Id,
        //                              CustomerId = b.CustomerId,
        //                              date = b.BookingDate,
        //                              address = b.Address,
        //                              notes = b.Notes,
        //                              status = b.Status,
        //                              title = s.Title,
        //                              description = s.Description,
        //                              UserName = u.Username,
        //                              mobileNumber = u.MobileNumber,
        //                              UserEmail = u.Email
        //                          }).ToListAsync();


        //     return Ok(new
        //     {
        //         data = booking,
        //         message = "All bookings fetches successfully",
        //         success = true
        //     });

        // }

        [Authorize]
        [HttpGet("getallbookings")]
        public async Task<IActionResult> Getallbookings()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Unauthorized Access , Please Login as Admin",
                    success = false
                });
            }

            var booking = await (
                from b in _context.Bookings
                join u in _context.Users
                    on b.CustomerId equals u.Id into userJoin
                from u in userJoin.DefaultIfEmpty()

                join s in _context.Services
                    on b.ServiceId equals s.Id into serviceJoin
                from s in serviceJoin.DefaultIfEmpty()

                select new
                {
                    BookingId = b.Id,
                    CustomerId = b.CustomerId,
                    date = b.BookingDate,
                    address = b.Address,
                    notes = b.Notes,
                    status = b.Status,

                    title = s != null ? s.Title : null,
                    description = s != null ? s.Description : null,

                    UserName = u != null ? u.Username : null,
                    mobileNumber = u != null ? u.MobileNumber : null,
                    UserEmail = u != null ? u.Email : null
                }
            ).ToListAsync();

            return Ok(new
            {
                data = booking,
                message = "All bookings fetched successfully",
                success = true
            });
        }


        // get  bookings for provider 
        [Authorize]
        [HttpGet("get-user-requested-services-for-provider-Specific")]
        public async Task<IActionResult> GetAllProviderSpecificUserRequest()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var providerId = User.FindFirst("userId")?.Value;

            if (role != "Provider")
            {
                return Ok(new
                {
                    message = "Unauthorized access, please login as Provider",
                    success = false
                });
            }

            int pid = int.Parse(providerId);

            // var bookings = await (
            //     from b in _context.Bookings
            //     join s in _context.Services on b.ServiceId equals s.Id


            //     where b.ServiceId== s.Id  &&   s.ProviderId == pid  
            //     select new
            //     {
            //         bookingId = b.Id,//return
            //         serviceId = b.ServiceId,//ret
            //         serviceTitle = s.Title,//show
            //         price = s.Price, // show
            //         bookingDate = b.BookingDate,//show
            //         BookingStatus = b.Status,
            //         customerId = b.CustomerId, // ret but not show
            //         customerAddress = b.Address, // show
            //         providerId = s.ProviderId
            //     }
            // ).ToListAsync();

            var bookings = await (
    from b in _context.Bookings
    join s in _context.Services on b.ServiceId equals s.Id
    join u in _context.Users on b.CustomerId equals u.Id
    where s.ProviderId == pid
    select new
    {
        bookingId = b.Id,
        serviceId = b.ServiceId,
        serviceTitle = s.Title,
        price = s.Price,
        bookingDate = b.BookingDate,
        bookingStatus = b.Status,

        customerId = b.CustomerId,
        customerAddress = b.Address,

        providerId = s.ProviderId,

        farmerMobile = u.MobileNumber,
        farmerName = u.Username
    }
).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Provider booking requests fetched successfully",
                data = bookings //3 result expected object
            });
        }



        // check booking id in booking table exist or not if exists then change status confirm when click the confir

        [Authorize]
        [HttpPatch("updatebookingStatusbyid/{bookingid}")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingid, [FromQuery] BookingStatus status)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Provider")
            {
                return Ok(new
                {
                    message = "Unauthorized User, Please Login as Provider",
                    success = false
                });
            }

            var booking = await _context.Bookings.FindAsync(bookingid);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking not found",
                    success = false
                });
            }

            // update status
            booking.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking status updated successfully",
                success = true,
                data = booking
            });
        }
   
   
    }
}













