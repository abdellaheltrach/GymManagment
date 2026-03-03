using GymManagement.Domain.Interfaces;
using GymManagement.Domain.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace GymManagement.Infrastructure.Identity
{
    public class SmtpEmailService(EmailSettings settings) : IEmailService
    {
        public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            
            // For development/debugging purposes, we might want to skip certificate validation
            // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(settings.Host, settings.Port, settings.UseSsl, ct);
            
            if (!string.IsNullOrEmpty(settings.Username))
            {
                await client.AuthenticateAsync(settings.Username, settings.Password, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
