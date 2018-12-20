using System.Threading.Tasks;

namespace EcommerceApi.Controllers
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string textMessage = null);
    }
}