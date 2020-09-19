using EcommerceApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Security;

namespace EcommerceApi.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EcommerceContext _context;

        public EmailSender(EcommerceContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string textMessage,
            Stream[] attachments = null,
            string[] attachmentNames = null,
            bool ccAdmins = false,
            string cc = null)
        {
            try
            {
                var settings = _context.Settings.FirstOrDefault();

                if (string.IsNullOrEmpty(toEmail))
                    toEmail = settings.ReportEmail;

                int port = settings.SmtpPort;
                string host = settings.SmtpHost;
                string username = settings.FromEmail;
                string password = settings.FromEmailPassword;
                string mailFrom = settings.FromEmail;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(mailFrom));
                message.To.Add(new MailboxAddress(toEmail));

                if (ccAdmins)
                {
                    var adminsEmails = settings.AdminEmail.Split(',').ToArray();
                    foreach (var item in adminsEmails)
                    {
                        message.Bcc.Add(new MailboxAddress(item));
                    }
                }

                if (!string.IsNullOrEmpty(cc))
                {
                    message.Bcc.Add(new MailboxAddress(cc));
                }

                message.Subject = subject;
                var builder = new BodyBuilder
                {
                    TextBody = textMessage
                };

                if (attachments != null && attachments.Length > 0)
                {
                    for(int i = 0; i < attachments.Length; i++)
                    {
                        builder.Attachments.Add(attachmentNames[i], attachments[i]);
                    }
                }

                message.Body = builder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(host, port, SecureSocketOptions.StartTls);
                    client.Authenticate(username, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task SendAdminReportAsync(string subject, string textMessage)
        {
            try
            {
                var settings = _context.Settings.FirstOrDefault();
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(settings.FromEmail));
                message.To.Add(new MailboxAddress(settings.ReportEmail));

                message.Subject = subject;
                var builder = new BodyBuilder
                {
                    TextBody = textMessage
                };

                message.Body = builder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(settings.SmtpHost, settings.SmtpPort, SecureSocketOptions.StartTls);
                    client.Authenticate(settings.FromEmail, settings.FromEmailPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
