using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Dekofar.HyperConnect.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        //public string? Role { get; set; }

        /// <summary>
        ///     URL or file path of the user's profile image.
        /// </summary>
        public string? AvatarUrl { get; set; }
        //public ICollection<Order> Orders { get; set; } = new List<Order>();


        [NotMapped]
        public ICollection<Order> Orders { get; set; }

        /// <summary>
        ///     Stored hashed representation of the user's 4-digit PIN.
        /// </summary>
        public string? HashedPin { get; set; }

        /// <summary>
        ///     Timestamp when the user's PIN was last updated.
        /// </summary>
        public DateTime? PinLastUpdatedAt { get; set; }

        public DateTime MembershipDate { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public int TotalSalesCount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public int TotalSupportRequestCount { get; set; }
        public int UnreadMessageCount { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public DateTime? LastSupportActivity { get; set; }

        [NotMapped]
        public int TotalSupportTickets
        {
            get => TotalSupportRequestCount;
            set => TotalSupportRequestCount = value;
        }

        [NotMapped]
        public int OpenSupportTickets { get; set; }

        [NotMapped]
        public int ClosedSupportTickets { get; set; }

        [NotMapped]
        public DateTime? LastSupportActivityAt
        {
            get => LastSupportActivity;
            set => LastSupportActivity = value;
        }



        [JsonIgnore]
        public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
    }
}
