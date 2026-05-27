using FashionEcommerce.API.Models.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FashionEcommerce.API.Services.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
                string.IsNullOrWhiteSpace(_settings.FromEmail) ||
                string.IsNullOrWhiteSpace(_settings.Username) ||
                string.IsNullOrWhiteSpace(_settings.Password))
            {
                throw new InvalidOperationException("EmailSettings is not configured.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            try
            {
                var socketOptions = _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOptions);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                // Log success with Message-Id if available
                var msgId = message.MessageId ?? string.Empty;
                if (!string.IsNullOrEmpty(msgId))
                {
                    _logger.LogInformation("Email sent to {ToEmail} (Message-Id: {MessageId})", toEmail, msgId);
                }
                else
                {
                    _logger.LogInformation("Email sent to {ToEmail}", toEmail);
                }
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}