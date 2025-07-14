using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ExamTimeTable.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(email);

            await client.SendMailAsync(mail);
        }
    }
}
