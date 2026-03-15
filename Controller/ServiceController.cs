
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using System.Security.Claims;


namespace MyApi.Controllers
{

    [ApiController]
    [Route("api/service-provider")]
    public class ServiceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        // private readonly IConfiguration _configuration;
        public ServiceController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

        }


        //add service
        // add service
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddService([FromBody] ServiceDto dto)
        {

            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Invalid data",
                    success = false
                });
            }



            var userId = User.FindFirst("userId")?.Value;
            var mobile = User.FindFirst("mobileNo")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;


            if (role != "Provider")
            {
                return Ok(new
                {
                    Role = role,
                    message = "Only Provider can Create Service",
                    success = false
                });

            }
            var service = new Models.Service
            {
                Title = dto.title,
                Description = dto.description,
                Category = dto.category,
                ProviderId = int.Parse(userId),
                Price = dto.price,
                District = dto.district,
                IsActive = dto.IsActive   // default false if not sent
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Role = role,
                message = "Service added successfully",
                success = true,
                data = service
            });


        }

        //get provider specific services 
        //check role and userid exists in service table 

        [Authorize]
        [HttpGet("get-provider-specific-service")]
        public async Task<IActionResult> GetServiceProviderSpecific()
        {
            var userId = User.FindFirst("userId")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Provider")
            {
                return Ok(new
                {
                    message = "Only Provider can view their services",
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

            var services = await _context.Services
                .Where(s => s.ProviderId.ToString() == userId)
                .ToListAsync();

            if (services == null || services.Count == 0)
            {
                return Ok(new
                {
                    message = "No services found for this provider",
                    success = false
                });
            }

            return Ok(new
            {
                message = "Services fetched successfully",
                success = true,
                data = services
            });


        }

        //get user provider specific service -only admin can view


        [HttpGet("provider/{userId}")]
        public async Task<IActionResult> GetServiceByAdminProviderSpecific(int userId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Only Admin can view provider services",
                    success = false
                });
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found",
                    success = false
                });
            }

            var services = await _context.Services
                            .Where(s => s.ProviderId == userId)
                            .ToListAsync();

            if (services.Count == 0)
            {
                return Ok(new
                {
                    message = "No services found for this provider",
                    success = false
                });
            }

            return Ok(new
            {
                message = "Services fetched successfully",
                success = true,
                data = services
            });
        }



        // get all services - if role is admin then it is accessible
        [Authorize]
        [HttpGet("getallServices")]
        public async Task<IActionResult> GetAllSerivices()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // if (role != "Admin")
            // {
            //     return Ok(new
            //     {
            //         message = "Unauthorized Access, Please login as admin",
            //         success = false
            //     });
            // }

            // Admin logic
            // var services = await _context.Services.ToListAsync();

            var services = await (
         from s in _context.Services
         join p in _context.Users
         on s.ProviderId equals p.Id
         select new
         {
             s.Id,
             s.Title,
             s.Description,
             s.Category,
             s.Price,
             s.District,
             s.IsActive,
             s.CreatedAt,
             ProviderId = p.Id,
             ProviderName = p.Username
         }
     ).ToListAsync();

            return Ok(new
            {
                success = true,
                data = services
            });

        }



        //admin can activate service if isActive is true then make it false ,else true
        [Authorize]
        [HttpPatch("ServicestatusUpdateByAdmin/{sid}")]
        public async Task<IActionResult> UpdateStatusByAdmin(int sid)
        {
            // check role
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Unauthorized Access ,Please Login as Admin",
                    success = false
                });
            }

            // find service by sid
            var service = await _context.Services
                            .FirstOrDefaultAsync(s => s.Id == sid);

            if (service == null)
            {
                return Ok(new
                {
                    message = "Service Not Found",
                    success = false
                });
            }

            // update status
            service.IsActive = !service.IsActive;

            // _context.Services.Update(service);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Service Status Updated Successfully",
                success = true
            });
        }



        [Authorize]
        [HttpDelete("deleteService/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            // check role
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Unauthorized Access, Please Login as Admin",
                    success = false
                });
            }

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return Ok(new
                {
                    message = "Service Not Found",
                    success = false
                });
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Service Deleted Successfully",
                success = true
            });
        }



        // get-single-service/30  - id wise for singlepage service

        [Authorize]
        [HttpGet("get-single-service/{id}")]
        public async Task<IActionResult> GetSingleService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "Service not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Service fetched successfully",
                data = service
            });
        }


    }



}