



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Data;
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
                    Role=role, 
                    message = "Only Provider can Create Service",
                    success = false
                });

            }
            var service = new Models.Service
            {
                Title = dto.title,
                Description = dto.description,
                CategoryId = dto.categoryId,
                ProviderId = dto.providerId,
                Price = dto.price,
                District = dto.district,
                ImageUrl = dto.imageUrl,
                IsActive = dto.IsActive   // default false if not sent
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            return Ok(new
            { 
                Role=role,
                message = "Service added successfully",
                success = true,
                data = service
            });


        }

    }






}