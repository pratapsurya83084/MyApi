using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }

    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = null!;

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Optional: navigation properties
        // [ForeignKey("ServiceId")]
        // public Service Service { get; set; }

        // [ForeignKey("CustomerId")]
        // public User Customer { get; set; }
    }
}