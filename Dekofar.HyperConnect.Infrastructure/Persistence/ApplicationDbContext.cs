using Dekofar.HyperConnect.Application.Common.Interfaces;
using Dekofar.HyperConnect.Domain.Entities;
using Dekofar.HyperConnect.Domain.Entities.Orders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using DomainOrder = Dekofar.HyperConnect.Domain.Entities.Order;
using DomainCommission = Dekofar.HyperConnect.Domain.Entities.Commission;

namespace Dekofar.HyperConnect.Infrastructure.Persistence
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
          IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // =====================================================
        // 🔐 Identity
        // =====================================================
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
        public DbSet<IdentityRole<Guid>> Roles => Set<IdentityRole<Guid>>();

        // =====================================================
        // 🎧 Support
        // =====================================================
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
        public DbSet<SupportCategory> SupportCategories => Set<SupportCategory>();
        public DbSet<SupportCategoryRole> SupportCategoryRoles => Set<SupportCategoryRole>();
        public DbSet<SupportTicketReply> SupportTicketReplies => Set<SupportTicketReply>();

        // =====================================================
        // 🏷 Tags
        // =====================================================
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<OrderTag> OrderTags => Set<OrderTag>();

        // =====================================================
        // 🛒 Orders
        // =====================================================
        public DbSet<ManualOrder> ManualOrders => Set<ManualOrder>();
        public DbSet<ManualOrderItem> ManualOrderItems => Set<ManualOrderItem>();
        public DbSet<OrderCommission> OrderCommissions => Set<OrderCommission>();
        public DbSet<DomainOrder> Orders => Set<DomainOrder>();
        public DbSet<DomainCommission> Commissions => Set<DomainCommission>();

        // =====================================================
        // 💸 Discounts
        // =====================================================
        public DbSet<Discount> Discounts => Set<Discount>();

        // =====================================================
        // 🧠 Notes & Logs
        // =====================================================
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<JobStat> JobStats => Set<JobStat>();
        public DbSet<DeploymentLog> DeploymentLogs => Set<DeploymentLog>();

        // =====================================================
        // 🔔 Notifications & UI
        // =====================================================
        public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
        public DbSet<UserBadge> UserBadges => Set<UserBadge>();
        public DbSet<UserUIPreference> UserUIPreferences => Set<UserUIPreference>();
        public DbSet<UserMessage> UserMessages => Set<UserMessage>();

        // =====================================================
        // 🔐 Permissions & Security
        // =====================================================
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<AllowedAdminIp> AllowedAdminIps => Set<AllowedAdminIp>();
        public DbSet<BlacklistEntry> BlacklistEntries => Set<BlacklistEntry>();

        // =====================================================
        // 🖼 Media
        // =====================================================
        public DbSet<PinCoverImage> PinCoverImages => Set<PinCoverImage>();

        // =====================================================
        // 🧹 Moderation
        // =====================================================
        public DbSet<ModerationRule> ModerationRules => Set<ModerationRule>();
        public DbSet<ModerationLog> ModerationLogs => Set<ModerationLog>();

        // =====================================================
        // ⏱ Work & Calendar
        // =====================================================
        public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
        public DbSet<CalendarTask> CalendarTasks => Set<CalendarTask>();

        // =====================================================
        // 🧩 Templates
        // =====================================================
        public DbSet<ResponseTemplate> ResponseTemplates => Set<ResponseTemplate>();

        // =====================================================
        // 🛍️ SHOPIFY (SON EKLENEN – KRİTİK)
        // =====================================================
        public DbSet<ShopifyWebhookEvent> ShopifyWebhookEvents
            => Set<ShopifyWebhookEvent>();

        // =====================================================
        // 💾 SaveChanges
        // =====================================================
        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => await base.SaveChangesAsync(cancellationToken);

        // =====================================================
        // 🔧 Model Config
        // =====================================================
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ================= JobStats =================
            builder.Entity<JobStat>(e =>
            {
                e.ToTable("JobStats");
                e.HasKey(x => x.Id);
                e.Property(x => x.Date).IsRequired();
                e.Property(x => x.PaidMarked).HasDefaultValue(0);
                e.Property(x => x.CancelTagged).HasDefaultValue(0);
            });

            // ================= Support =================
            builder.Entity<SupportCategory>(e =>
            {
                e.ToTable("SupportCategories");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(250);
                e.Property(x => x.CreatedAt).IsRequired();

                e.HasMany(x => x.Roles)
                 .WithOne(r => r.Category)
                 .HasForeignKey(r => r.SupportCategoryId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SupportCategoryRole>(e =>
            {
                e.ToTable("SupportCategoryRoles");
                e.HasKey(x => x.Id);
                e.Property(x => x.RoleName).IsRequired().HasMaxLength(100);
            });

            builder.Entity<SupportTicket>(e =>
            {
                e.ToTable("SupportTickets");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).IsRequired();
                e.Property(x => x.FilePath).HasMaxLength(500);
                e.Property(x => x.UnreadReplyCount).HasDefaultValue(0);

                e.HasOne(x => x.Category)
                 .WithMany(c => c.Tickets)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ================= Orders =================
            builder.Entity<ManualOrder>(e =>
            {
                e.ToTable("ManualOrders");
                e.HasKey(x => x.Id);
                e.Property(x => x.CustomerName).IsRequired().HasMaxLength(100);
                e.Property(x => x.CustomerSurname).IsRequired().HasMaxLength(100);
                e.Property(x => x.Phone).HasMaxLength(50);
                e.Property(x => x.Email).HasMaxLength(100);
                e.Property(x => x.Address).IsRequired().HasMaxLength(500);
                e.Property(x => x.City).HasMaxLength(100);
                e.Property(x => x.District).HasMaxLength(100);
                e.Property(x => x.PaymentType).HasMaxLength(50);
                e.Property(x => x.OrderNote).HasMaxLength(500);
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            });

            builder.Entity<DomainOrder>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Id);
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
                e.Property(x => x.Status).HasConversion<int>();

                e.HasOne(x => x.Seller)
                 .WithMany(u => u.Orders)
                 .HasForeignKey(x => x.SellerId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.Customer)
                 .WithMany()
                 .HasForeignKey(x => x.CustomerId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<DomainCommission>(e =>
            {
                e.ToTable("Commissions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Amount).HasColumnType("decimal(18,2)");

                e.HasOne(x => x.User)
                 .WithMany(u => u.Commissions)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Order)
                 .WithMany()
                 .HasForeignKey(x => x.OrderId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ================= Templates =================
            builder.Entity<ResponseTemplate>(e =>
            {
                e.ToTable("ResponseTemplates");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Body).IsRequired();
                e.Property(x => x.CreatedAt).IsRequired();
                e.Property(x => x.ModuleScope).HasMaxLength(100);
            });

            // ================= Shopify =================
            builder.Entity<ShopifyWebhookEvent>(e =>
            {
                e.ToTable("ShopifyWebhookEvents");
                e.HasKey(x => x.Id);

                e.Property(x => x.Topic).IsRequired();
                e.Property(x => x.ExternalId).HasMaxLength(100);
                e.Property(x => x.Payload).IsRequired();
                e.Property(x => x.CreatedAtUtc).IsRequired();

                e.HasIndex(x => new { x.Topic, x.ExternalId });
            });
        }
    }
}
