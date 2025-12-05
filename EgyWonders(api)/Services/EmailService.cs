using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using EgyWonders.DTO;
using EgyWonders.Interfaces;

namespace EgyWonders.Services
{
    // Don't forget to create the interface IEmailService in Interfaces folder:
    // public interface IEmailService { Task SendEmailAsync(string to, string subject, string body); }

    public class EmailService : IEmailService
    {
        private readonly EmailSettingsDTO _settings;

        public EmailService(IOptions<EmailSettingsDTO> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}