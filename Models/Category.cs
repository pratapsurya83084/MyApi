using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(250)]
        public string? IconUrl { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Navigation property if you want to link to Services
        // public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}