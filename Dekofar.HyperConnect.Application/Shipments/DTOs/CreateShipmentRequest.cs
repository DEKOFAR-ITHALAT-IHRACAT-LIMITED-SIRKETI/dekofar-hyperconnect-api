using System.ComponentModel.DataAnnotations;

namespace Dekofar.HyperConnect.Application.Shipments.DTOs;

public class CreateShipmentRequest
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public string ReferenceId { get; set; } = null!;

    [Required]
    public string ReceiverName { get; set; } = null!;

    [Required]
    public string ReceiverPhone { get; set; } = null!;

    [Required]
    public string ReceiverAddress { get; set; } = null!;

    [Required]
    public string ReceiverCity { get; set; } = null!;

    [Required]
    public string ReceiverDistrict { get; set; } = null!;

    public bool IsCashOnDelivery { get; set; }

    public decimal? CashOnDeliveryAmount { get; set; }
}
