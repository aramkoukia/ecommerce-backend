using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Services
{
    public class EmailSender  : IEmailSender
    {
        public EmailSender(IOptions<EmailSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public EmailSenderOptions Options { get; }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string textMessage = null, Stream attachment = null, string attachmentName = null)
        {
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(this.Options.emailFromAddress, this.Options.emailFromName),
                Body = textMessage,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(toEmail);

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

            using (SmtpClient client = new SmtpClient(this.Options.host, this.Options.port))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(this.Options.username, this.Options.password);
                client.EnableSsl = this.Options.enableSSL;
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
