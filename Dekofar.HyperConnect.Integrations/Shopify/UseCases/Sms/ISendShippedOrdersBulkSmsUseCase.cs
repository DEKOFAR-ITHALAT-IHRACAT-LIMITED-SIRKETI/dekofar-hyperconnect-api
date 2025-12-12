namespace Dekofar.HyperConnect.Integrations.Shopify.UseCases.Sms
{
    public interface ISendShippedOrdersBulkSmsUseCase
    {
        Task ExecuteAsync(CancellationToken ct = default);
    }
}
