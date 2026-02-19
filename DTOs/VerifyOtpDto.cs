namespace MyApi.DTOs
{
    public class OtpVerifyDto
    {
        public int UserId { get; set; } 
        public string Otp { get; set; } = null!;
    }
}
