using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class SendMessage(ILogger<SendMessage> logger, IVerificationService verificationService, ISendMessageService sendMessageService)
{
    private readonly ILogger<SendMessage> _logger = logger;
    private readonly IVerificationService _verificationService = verificationService;
    private readonly ISendMessageService _sendMessageService = sendMessageService;

    [Function(nameof(SendMessage))]
    [ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task<string> Run(
        [ServiceBusTrigger("message_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,ServiceBusMessageActions messageActions)
    {
        try
        {
            var verificationRequest = _verificationService.UnpackVerificationRequest(message);
            if (verificationRequest != null)
            {
                var emailRequest = _sendMessageService.SendMessageConfirmation(verificationRequest.Email);
                if (emailRequest != null)
                {
                    string payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
                    if (!string.IsNullOrEmpty(payload))
                    {
                        await messageActions.CompleteMessageAsync(message);
                        return payload;
                    }
                }
               

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($": GenerateVerificationCode.Run() :: {ex.Message}");
        }

        return null!;

    }
}
