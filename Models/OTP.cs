using System;
using System.ComponentModel.DataAnnotations;

namespace MyApi.Models
{
    public class OTP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = null!;

        [Required]
        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; } = false;

        public int AttemptCount { get; set; } = 0;
    }
}