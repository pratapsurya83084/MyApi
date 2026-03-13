using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApi.Data;

using MyApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyApi.DTOs;


namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        // private readonly IConfiguration _configuration;
        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

        }



        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if mobile already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.MobileNumber == dto.MobileNumber);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User already exists with this mobile number"
                });
            }

            // Check if email already exists
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingEmail != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email already registered"
                });
            }

            var passwordHasher = new PasswordHasher<User>();

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
            {
                return BadRequest(new { message = "Invalid role value" });
            }

            var user = new User
            {
                MobileNumber = dto.MobileNumber,
                Username = dto.Username,
                Email = dto.Email,
                Role = parsedRole,     // Default role
                IsApproved = false,       // Needs admin approval
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Hash password
            user.Password = passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "User registered successfully. Waiting for admin approval."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.MobileNumber) || string.IsNullOrEmpty(dto.Password))
            {
                return Ok(new
                {
                    message = "Mobile number and password required",
                    success = false
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.MobileNumber == dto.MobileNumber);

            if (user == null)
            {
                return Ok(new
                {
                    message = "Invalid mobile number",
                    success = false
                });
            }

            var passwordHasher = new PasswordHasher<User>();

            var result = passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                dto.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                return Ok(new
                {
                    message = "Invalid password",
                    success = false
                });
            }

            // 🔥 Correct Role Claim
            var claims = new[]
            {
            new Claim("userId", user.Id.ToString()),
            new Claim("mobileNo", user.MobileNumber),
            new Claim(ClaimTypes.Role, user.Role.ToString()), // IMPORTANT
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_MY_SECRET_KEY_123456789_ABC")
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            Response.Cookies.Append("token", jwtToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new
            {
                message = "Login successful",
                success = true,

                token = jwtToken
            });
        }

        [Authorize]
        [HttpGet("get-alluser")]
        public async Task<IActionResult> GetAllUsers()
        {

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Unauthorized access , please login as admin",
                    success = false

                });
            }

            var users = await _context.Users
              .Select(u => new
              {
                  u.Id,
                  u.Username,
                  u.MobileNumber,
                  u.Email,
                  u.Role,
                  u.IsActive,
                  u.IsApproved,
                  u.CreatedAt
              })
              .ToListAsync();

            return Ok(new
            {
                message = "Retrieve All Users",
                success = true,
                count = users.Count,
                data = users
            });







        }



        // get farmer list only check role If Farmer then return those row only
        [HttpGet("get-farmers")]
        public async Task<IActionResult> GetFarmers()
        {
            var farmers = await _context.Users
                            .Where(u => u.Role == UserRole.Farmer)
                            .ToListAsync();

            return Ok(farmers);
        }

        // get farmer list only check role If provider then return those row only
        [HttpGet("get-provider")]
        public async Task<IActionResult> GetProviders()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    message = "Unauthorized Access, Please login as admin",
                    success = false
                });
            }
            var providers = await _context.Users
                            .Where(u => u.Role == UserRole.Provider)
                            .ToListAsync();

            return Ok(new
            {
                providers,
                success = true,
                message = "fetched provider successfully"
            });
        }

        //get profile by userId
        [Authorize]
        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("userId")?.Value;
            var UserProfile = await _context.Users
                            .Where(u => u.Id.ToString() == userId)
                            .ToListAsync();

            return Ok(UserProfile);
        }


        //user deletion by admin
        [Authorize]
        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            // check role
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Ok(new
                {
                    success = false,
                    message = "Unauthorized Access, Please Login as Admin"
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "User Not Found"
                });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "User Deleted Successfully"
            });
        }


    }

}
