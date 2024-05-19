
using Microsoft.Extensions.Logging;
using VerificationProvider.Models;


namespace VerificationProvider.Services;

public class SendMessageService(ILogger<SendMessageService> logger) : ISendMessageService
{
    private readonly ILogger<SendMessageService> _logger = logger;

    public EmailRequest SendMessageConfirmation(string email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            try
            {
                var emailRequest = new EmailRequest()
                {
                    To = email,
                    Subject = $"Confirmation message",
                    HtmlBody = $@"
                         <html>
                            <head>
                            </head>
                            <body>
                                <div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; border-radius: 5px;'>
                                    <p style='color: #333;'>Dear user,</p>
                                    <p style='color: #333;'>You request successfully reached us! Soon you will be joined to the course.</p>
                                </div>
                            </body>
                        </html>",
                    PlainText = $"You request successfully reached us! Soon you will be joined to the course."
                };
                return emailRequest;

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendMessageService.SendMessageConfirmation() :: {ex.Message}");
            }
        }
        return null!;

    }
}