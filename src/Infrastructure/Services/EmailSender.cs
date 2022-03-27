using System.Threading.Tasks;
using Application.Common;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly AuthMessageSenderOptions _options;

        public EmailSender(ILogger<EmailSender> logger, IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string text)
        {
            if (_options.SendGridKey == null || _options.Email == null)
            {
                _logger.LogError("Configuration for sending mails is not found");
                return;
            }

            var client = new SendGridClient(_options.SendGridKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_options.Email),
                Subject = subject,
                PlainTextContent = text,
                HtmlContent = text
            };

            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation(LogEvents.SendingConfirmationEmail, response.IsSuccessStatusCode
                ? $"Sent email to {email}"
                : $"Failed to send email to {email}");
        }
    }
}