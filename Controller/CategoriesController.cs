using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public CategoriesController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddCategories(CategoriesDto catdto)
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

            var category = new Category
            {
                Name = catdto.Name,
                Description = catdto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category added successfully",
                success = true,
                data = category
            });
        }


        [Authorize]
        [HttpGet("getCtegories")]
        public async Task<IActionResult> GetAllCategories()
        {
           
            var category = await _context.Categories.ToListAsync();
            return Ok(new
            {
                message = "retrieve all categories",
                success = true,
                category
            });

        }


        [Authorize]
        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
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

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found",
                    success = false
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully",
                success = true,
                data = category
            });
        }



        [Authorize]
        [HttpPatch("updatebyid/{id}")]
        public async Task<IActionResult> UpdateCategoryById(int id ,CategoriesDto catdto)
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

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found",
                    success = false
                });
            }

            category.Name = catdto.Name;
            category.Description = catdto.Description;
            category.CreatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category Updated successfully",
                success = true,
                data = category
            });
        }



    }
}