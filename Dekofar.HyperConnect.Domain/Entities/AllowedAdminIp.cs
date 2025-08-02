using System;

namespace Dekofar.HyperConnect.Domain.Entities
{
    public class AllowedAdminIp
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
