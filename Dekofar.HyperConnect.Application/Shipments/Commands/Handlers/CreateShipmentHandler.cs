using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Application.Shipments.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.Commands.Handlers
{
    public class CreateShipmentHandler
        : IRequestHandler<CreateShipmentCommand, CreateShipmentResult>
    {
        private readonly IShipmentProvider _provider;
        private readonly IApplicationDbContext _db;

        public CreateShipmentHandler(
            IShipmentProvider provider,
            IApplicationDbContext db)
        {
            _provider = provider;
            _db = db;
        }

        public async Task<CreateShipmentResult> Handle(
            CreateShipmentCommand request,
            CancellationToken ct)
        {
            // Idempotency
            var exists = await _db.Shipments
                .AnyAsync(x => x.ReferenceId == request.Request.ReferenceId, ct);

            if (exists)
                return new() { Success = true };

            var result = await _provider.CreateAsync(request.Request);

            if (!result.Success)
                return result;

            _db.Shipments.Add(new Shipment
            {
                OrderId = request.Request.OrderId,
                ReferenceId = request.Request.ReferenceId,
                TrackingNo = result.TrackingNo,
                IsCashOnDelivery = request.Request.IsCashOnDelivery,
                CashOnDeliveryAmount = request.Request.CashOnDeliveryAmount,
                Status = ShipmentStatus.Accepted
            });

            await _db.SaveChangesAsync(ct);
            return result;
        }
    }

}
