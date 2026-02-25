namespace MyApi.DTOs
{
    public class SignupDto
    {
        public string Username { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
