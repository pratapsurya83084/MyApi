using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(100)]
        public string District { get; set; } = null!;

        [MaxLength(250)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional navigation properties if you have Category and Provider models
        // public Category Category { get; set; } = null!;
        // public User Provider { get; set; } = null!;
    }
}