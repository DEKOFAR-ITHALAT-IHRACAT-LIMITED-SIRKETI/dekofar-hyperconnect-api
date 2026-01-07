using Dekofar.HyperConnect.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dekofar.HyperConnect.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        // =====================================================
        // 🛍️ SHOPIFY (KRİTİK – EKSİK OLAN KISIM)
        // =====================================================
        DbSet<ShopifyStore> ShopifyStores { get; }
        DbSet<ShopifyWebhookEvent> ShopifyWebhookEvents { get; }

        // =====================================================
        // 🎧 Support
        // =====================================================
        DbSet<SupportTicket> SupportTickets { get; }
        DbSet<SupportCategory> SupportCategories { get; }
        DbSet<SupportCategoryRole> SupportCategoryRoles { get; }
        DbSet<SupportTicketReply> SupportTicketReplies { get; }

        // =====================================================
        // 🛒 Orders
        // =====================================================
        DbSet<ManualOrder> ManualOrders { get; }
        DbSet<ManualOrderItem> ManualOrderItems { get; }
        DbSet<OrderCommission> OrderCommissions { get; }
        DbSet<Order> Orders { get; }
        DbSet<Commission> Commissions { get; }

        // =====================================================
        // 💸 Discounts
        // =====================================================
        DbSet<Discount> Discounts { get; }

        // =====================================================
        // 🧠 Notes & Logs
        // =====================================================
        DbSet<Note> Notes { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<ActivityLog> ActivityLogs { get; }
        DbSet<JobStat> JobStats { get; }

        // =====================================================
        // 🔔 Notifications & UI
        // =====================================================
        DbSet<UserNotification> UserNotifications { get; }
        DbSet<UserBadge> UserBadges { get; }
        DbSet<UserUIPreference> UserUIPreferences { get; }
        DbSet<UserMessage> UserMessages { get; }

        // =====================================================
        // 🔐 Identity & Security
        // =====================================================
        DbSet<ApplicationUser> Users { get; }
        DbSet<IdentityUserRole<Guid>> UserRoles { get; }
        DbSet<IdentityRole<Guid>> Roles { get; }
        DbSet<Permission> Permissions { get; }
        DbSet<RolePermission> RolePermissions { get; }

        // =====================================================
        // 🖼 Media
        // =====================================================
        DbSet<PinCoverImage> PinCoverImages { get; }

        // =====================================================
        // 🧹 Moderation
        // =====================================================
        DbSet<ModerationRule> ModerationRules { get; }
        DbSet<ModerationLog> ModerationLogs { get; }

        // =====================================================
        // ⏱ Work & Calendar
        // =====================================================
        DbSet<WorkSession> WorkSessions { get; }

        // =====================================================
        // 🧩 Templates
        // =====================================================
        DbSet<ResponseTemplate> ResponseTemplates { get; }

        // =====================================================
        // 💾 Save
        // =====================================================
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
