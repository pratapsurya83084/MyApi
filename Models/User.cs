using System.ComponentModel.DataAnnotations;

namespace MyApi.Models
{
  public enum UserRole
{
    Admin,
    // User,
    Provider,
    Farmer
}

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } 

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; }

        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Password { get; internal set; }
    }
}