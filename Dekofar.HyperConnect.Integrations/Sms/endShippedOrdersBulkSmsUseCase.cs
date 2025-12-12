using Dekofar.HyperConnect.Integrations.Shopify.UseCases.Orders;
using Dekofar.HyperConnect.Integrations.Sms.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Integrations.Sms
{
    public class SendShippedOrdersBulkSmsUseCase
    {
        private readonly IGetFulfilledOrdersUseCase _orders;
        private readonly ISmsSender _sms;

        public SendShippedOrdersBulkSmsUseCase(
            IGetFulfilledOrdersUseCase orders,
            ISmsSender sms)
        {
            _orders = orders;
            _sms = sms;
        }

        public async Task ExecuteAsync(CancellationToken ct = default)
        {
            var end = DateTime.UtcNow;
            var start = end.AddHours(-24); // veya bugün

            var shippedOrders = await _orders.ExecuteAsync(start, end, ct);

            foreach (var order in shippedOrders)
            {
                if (string.IsNullOrWhiteSpace(order.Phone))
                    continue;

                foreach (var tracking in order.TrackingNumbers)
                {
                    var msg =
                        $"Siparişiniz kargoya verildi.\n" +
                        $"Takip No: {tracking}\n" +
                        $"Dekofar";

                    await _sms.SendAsync(order.Phone, msg, ct);
                }
            }
        }

    }

}
