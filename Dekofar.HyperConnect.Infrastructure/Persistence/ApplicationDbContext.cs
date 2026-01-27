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
        // 🧱 DbSets
        // =====================================================
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
        public DbSet<SupportCategory> SupportCategories => Set<SupportCategory>();
        public DbSet<SupportCategoryRole> SupportCategoryRoles => Set<SupportCategoryRole>();

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
        public DbSet<IdentityRole<Guid>> Roles => Set<IdentityRole<Guid>>();

        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<OrderTag> OrderTags => Set<OrderTag>();

        public DbSet<ManualOrder> ManualOrders => Set<ManualOrder>();
        public DbSet<ManualOrderItem> ManualOrderItems => Set<ManualOrderItem>();

        public DbSet<OrderCommission> OrderCommissions => Set<OrderCommission>();
        public DbSet<DomainOrder> Orders => Set<DomainOrder>();
        public DbSet<DomainCommission> Commissions => Set<DomainCommission>();

        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
        public DbSet<UserBadge> UserBadges => Set<UserBadge>();
        public DbSet<UserUIPreference> UserUIPreferences => Set<UserUIPreference>();

        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<UserMessage> UserMessages => Set<UserMessage>();
        public DbSet<SupportTicketReply> SupportTicketReplies => Set<SupportTicketReply>();

        public DbSet<PinCoverImage> PinCoverImages => Set<PinCoverImage>();
        public DbSet<BlacklistEntry> BlacklistEntries => Set<BlacklistEntry>();
        public DbSet<CalendarTask> CalendarTasks => Set<CalendarTask>();
        public DbSet<AllowedAdminIp> AllowedAdminIps => Set<AllowedAdminIp>();
        public DbSet<DeploymentLog> DeploymentLogs => Set<DeploymentLog>();

        public DbSet<ResponseTemplate> ResponseTemplates => Set<ResponseTemplate>();
        public DbSet<ModerationRule> ModerationRules => Set<ModerationRule>();
        public DbSet<ModerationLog> ModerationLogs => Set<ModerationLog>();

        public DbSet<JobStat> JobStats => Set<JobStat>();
        public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
        public DbSet<ShopifyStore> ShopifyStores => Set<ShopifyStore>();

        // =======================
        // 📦 KARGO / PTT
        // =======================
        public DbSet<Shipment> Shipments => Set<Shipment>();

        // =====================================================
        // 💾 SaveChanges
        // =====================================================
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await base.SaveChangesAsync(cancellationToken);

        public async Task<int> SaveChangesAsync()
            => await base.SaveChangesAsync();

        // =====================================================
        // 🧠 Model Config
        // =====================================================
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<JobStat>(entity =>
            {
                entity.ToTable("JobStats");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.PaidMarked).HasDefaultValue(0);
                entity.Property(e => e.CancelTagged).HasDefaultValue(0);
            });

            builder.Entity<SupportCategory>(entity =>
            {
                entity.ToTable("SupportCategories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            builder.Entity<Shipment>(entity =>
            {
                entity.ToTable("Shipments");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ReferenceId)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(x => x.ReferenceId)
                      .IsUnique();

                entity.Property(x => x.TrackingNo)
                      .HasMaxLength(100);

                entity.Property(x => x.Status)
                      .HasConversion<int>();
            });
        }
    }
}
