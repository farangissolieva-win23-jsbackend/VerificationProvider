using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface ISendMessageService
    {
        EmailRequest SendMessageConfirmation(string email);
    }
}