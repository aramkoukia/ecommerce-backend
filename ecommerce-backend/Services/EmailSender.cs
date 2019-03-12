using EcommerceApi.Models;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Services
{
    public class EmailSender  : IEmailSender
    {
        private readonly EcommerceContext _context;

        public EmailSender(EcommerceContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string textMessage = null, Stream attachment = null, string attachmentName = null, bool ccAdmins = false)
        {
            var settings = _context.Settings.FirstOrDefault();
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromEmail),
                Body = textMessage,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8
            };

            if (string.IsNullOrEmpty(toEmail))
                toEmail = settings.ReportEmail;

            mailMessage.To.Add(toEmail);

            if (ccAdmins) {
                mailMessage.To.Add(settings.ReportEmail);
            }

            if (!string.IsNullOrEmpty(attachmentName))
            {
                var file = new Attachment(attachment, attachmentName);
                mailMessage.Attachments.Add(file);
            }

            if (!string.IsNullOrEmpty(htmlMessage))
            {
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage);
                htmlView.ContentType = new System.Net.Mime.ContentType("text/html");
                mailMessage.AlternateViews.Add(htmlView);
            }

            using (SmtpClient client = new SmtpClient(settings.SmtpHost, settings.SmtpPort))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(settings.FromEmail, settings.FromEmailPassword);
                client.EnableSsl = settings.SmtpUseSsl;
                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendAdminReportAsync(string subject, string textMessage)
        {
            var settings = _context.Settings.FirstOrDefault();
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromEmail),
                Body = textMessage,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(settings.ReportEmail);

            using (SmtpClient client = new SmtpClient(settings.SmtpHost, settings.SmtpPort))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(settings.FromEmail, settings.FromEmailPassword);
                client.EnableSsl = settings.SmtpUseSsl;
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
