using System;
using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.DTOs
{
    public class BookingDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime? BookingDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = null!;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}