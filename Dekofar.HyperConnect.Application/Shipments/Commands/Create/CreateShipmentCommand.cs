using Dekofar.HyperConnect.Application.Shipments.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Shipments.Commands.Create
{
    public class CreateShipmentCommand : IRequest<CreateShipmentResult>
    {
        public CreateShipmentRequest Request { get; }

        public CreateShipmentCommand(CreateShipmentRequest request)
        {
            Request = request;
        }
    }

}
