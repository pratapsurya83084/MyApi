using System.Net;
using System.Net.Mail;

namespace MyApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmail(string toEmail, string otp)
        {
            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpHost"])
            {
                Port = int.Parse(_config["EmailSettings:Port"]!),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:FromEmail"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:FromEmail"]!),
                Subject = "Your OTP Code",
                Body = $"Your OTP is {otp}. It will expire in 1 minute.",
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);

            await smtpClient.SendMailAsync(mail);
        }
    }
}
