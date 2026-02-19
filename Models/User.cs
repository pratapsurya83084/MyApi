namespace MyApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        // OTP fields
        public string? Otp { get; set; }

        public DateTime? OtpExpireAt { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
