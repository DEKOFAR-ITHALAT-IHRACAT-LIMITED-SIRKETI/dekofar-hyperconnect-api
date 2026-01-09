namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Sms;

public interface ISmsSender
{
    Task SendAsync(string phone, string message, CancellationToken ct);
}

