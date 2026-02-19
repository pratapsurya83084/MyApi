using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyApi.Services;
using Microsoft.AspNetCore.Authorization;



namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        public UserController(AppDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        // ============================
        // SIGNUP + GENERATE OTP
        // ============================
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // CASE 1: User exists AND already verified
            if (user != null && user.IsVerified)
            {
                return Ok(new
                {
                    message = "you are logged User",
                    success = true
                });
            }

            var otp = new Random().Next(100000, 999999).ToString();

            // CASE 2: User exists but NOT verified → regenerate OTP
            if (user != null)
            {
                user.Otp = otp;
                user.OtpExpireAt = DateTime.UtcNow.AddMinutes(1);

                await _context.SaveChangesAsync();
            }
            // CASE 3: User does NOT exist → create user + OTP
            else
            {
                user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Otp = otp,
                    OtpExpireAt = DateTime.UtcNow.AddMinutes(1),
                    IsVerified = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // SEND OTP EMAIL
            await _emailService.SendOtpEmail(dto.Email, otp);

            return Ok(new
            {
                message = "OTP sent to email. It will expire in 1 minute.",
                success = true
            });
        }


        // VERIFY OTP (LOGIN)
        // ============================

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(OtpVerifyDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);

            if (user == null)
                return BadRequest("User not found");

            if (user.Otp == null || user.OtpExpireAt == null)
                return BadRequest("OTP not generated");

            if (user.OtpExpireAt < DateTime.UtcNow){
                    user.IsVerified=false;
                return BadRequest("OTP expired");
             }
            if (user.Otp != dto.Otp)
                return BadRequest("Invalid OTP");

            // OTP VERIFIED
            user.IsVerified = true;
            user.Otp = null;
            user.OtpExpireAt = null;

            await _context.SaveChangesAsync();

            // =========================
            // GENERATE JWT (LOGIN)
            // =========================
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email!)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(1),
                claims: claims,
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "OTP verified & logged in",
                token = jwtToken,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email
                }
            });
        }



        [Authorize]
        [HttpGet("get-alluser")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.IsVerified,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                count = users.Count,
                data = users
            });
        }
    }

}
