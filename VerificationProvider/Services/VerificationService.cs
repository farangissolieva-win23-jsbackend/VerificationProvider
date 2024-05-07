
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Functions;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider) : IVerificationService
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
            {
                return verificationRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($": GenerateVerificationCode.UnpackVerificationRequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);
            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($": GenerateVerificationCode.GenerateCode() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();
            var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new Data.Entities.VerificationRequestEntity() { Email = verificationRequest.Email, Code = code });
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($": GenerateVerificationCode.SaveVerificationRequest() :: {ex.Message}");
        }
        return false;
    }

    public EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest()
                {
                    To = verificationRequest.Email,
                    Subject = $"verification Code {code}",
                    HtmlBody = $@"
                         <html>
                            <head>
                            </head>
                            <body>
                                <div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; border-radius: 5px;'>
                                    <p style='color: #333;'>Dear user,</p>
                                    <p style='color: #333;'>Your verification code is:</p>
                                    <p style='font-size: 24px; font-weight: bold; color: #007bff;'>{code}</p>
                                    <p style='color: #333;'>Please use this code to verify your account.</p>
                                </div>
                            </body>
                        </html>",
                    PlainText = $"Please use this code to verify your account using this code: {code}."
                };

                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($": GenerateVerificationCode.GenerateEmailrequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($": GenerateVerificationCode.GenerateServiceBusEmailRequest() :: {ex.Message}");
        }
        return null!;
    }
}
