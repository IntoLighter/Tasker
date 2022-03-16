using System;
using System.Threading.Tasks;
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

        public EmailSender(ILogger<EmailSender> logger, IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            _logger = logger;
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } // Should be filled automatically

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(Options.SendGridKey)) throw new Exception("Null SendGridKey");

            await Execute(Options.SendGridKey, subject, message, email);
        }

        public async Task Execute(string apiKey, string subject, string messageText, string email)
        {
            var client = new SendGridClient(apiKey);
            var message = new SendGridMessage
            {
                From = new EmailAddress("intolighter.net@gmail.com"),
                Subject = subject,
                PlainTextContent = messageText,
                HtmlContent = messageText
            };

            message.AddTo(new EmailAddress(email));
            message.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(message);
            _logger.LogInformation(response.IsSuccessStatusCode
                ? $"Email to {email} queued successfully!"
                : $"Failure Email to {email}");
        }
    }
}