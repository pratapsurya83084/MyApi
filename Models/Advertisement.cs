using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Models
{
    public class Advertisement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(250)]
        public string ImageUrl { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string District { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}