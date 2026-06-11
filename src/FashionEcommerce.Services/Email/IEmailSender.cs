using System.Threading.Tasks;

namespace FashionEcommerce.Services.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
