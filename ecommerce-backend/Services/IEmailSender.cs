using System.IO;
using System.Threading.Tasks;

namespace EcommerceApi.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail,
                            string subject,
                            string textMessage = null,
                            Stream[] attachment = null,
                            string[] attachmentName = null,
                            bool ccAdmins = false,
                            string cc = null);
        Task SendAdminReportAsync(string subject, string textMessage);
    }
}
