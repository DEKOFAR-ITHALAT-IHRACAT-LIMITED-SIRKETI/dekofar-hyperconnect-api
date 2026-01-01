using Microsoft.Extensions.Logging;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Sms;

public class DummySmsSender : ISmsSender
{
    private readonly ILogger<DummySmsSender> _logger;

    public DummySmsSender(ILogger<DummySmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phone, string message, CancellationToken ct)
    {
        _logger.LogInformation(
            "SMS (DUMMY) → {Phone}\n{Message}",
            phone,
            message);

        return Task.CompletedTask;
    }
}
