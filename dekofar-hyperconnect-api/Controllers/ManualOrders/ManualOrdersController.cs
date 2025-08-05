using Dekofar.HyperConnect.Application.ManualOrders.Commands;
using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dekofar.API.Controllers;

[ApiController]
[Route("api/manual-orders")]
public class ManualOrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public ManualOrdersController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    // ✅ Tüm manuel siparişleri getirir
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.ManualOrders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders);
    }

    // ✅ Belirli siparişi ID ile getirir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _context.ManualOrders.FindAsync(id);
        return order == null ? NotFound() : Ok(order);
    }

    // ✅ Yeni sipariş oluşturur (MediatR)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManualOrderCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _mediator.Send(command);
        return Ok(id);
    }

    // ✅ Siparişi günceller
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ManualOrder order)
    {
        if (id != order.Id) return BadRequest();

        var existing = await _context.ManualOrders.FindAsync(id);
        if (existing == null) return NotFound();

        // Alanları güncelle
        existing.CustomerName = order.CustomerName;
        existing.CustomerSurname = order.CustomerSurname;
        existing.Phone = order.Phone;
        existing.Email = order.Email;
        existing.Address = order.Address;
        existing.City = order.City;
        existing.District = order.District;
        existing.PaymentType = order.PaymentType;
        existing.OrderNote = order.OrderNote;
        existing.Status = order.Status;
        existing.TotalAmount = order.TotalAmount;
        existing.DiscountName = order.DiscountName;
        existing.DiscountType = order.DiscountType;
        existing.DiscountValue = order.DiscountValue;
        existing.BonusAmount = order.BonusAmount;

        await _context.SaveChangesAsync();
        return Ok();
    }

    // ✅ Siparişi siler
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _context.ManualOrders.FindAsync(id);
        if (existing == null) return NotFound();

        _context.ManualOrders.Remove(existing);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // ✅ Gelişmiş arama (isim, telefon, e-posta, not vs)
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<ManualOrder>());

        var q = query.ToLower();

        var results = await _context.ManualOrders
            .Where(o =>
                o.CustomerName.ToLower().Contains(q) ||
                o.CustomerSurname.ToLower().Contains(q) ||
                o.Phone.ToLower().Contains(q) ||
                o.Email.ToLower().Contains(q) ||
                o.OrderNote.ToLower().Contains(q) ||
                o.Address.ToLower().Contains(q)
            )
            .OrderByDescending(o => o.CreatedAt)
            .Take(250)
            .ToListAsync();

        return Ok(results);
    }
}
