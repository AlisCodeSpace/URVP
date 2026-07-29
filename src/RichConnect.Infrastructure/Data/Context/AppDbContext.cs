using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using MicrosoftDataProtectionKey = Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Domain.Entities.Files;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Domain.Entities.Users;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Domain.Entities.Themes;
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using RICHConnect.Backend.Domain.Entities.Partners;
using RICHConnect.Backend.Domain.Entities.Faculty;
using RICHConnect.Backend.Domain.Entities.Admin;
using RICHConnect.Backend.Domain.Entities.RDProjects;
using RICHConnect.Backend.Domain.Entities.System;

namespace RICHConnect.Backend.Infrastructure.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ----------- DbSets -----------
        public DbSet<User> Users { get; set; }
        public DbSet<CommunityPartner> CommunityPartners { get; set; }
        public DbSet<FacultySpecialist> FacultySpecialists { get; set; }
        public DbSet<ResearchField> ResearchFields { get; set; }
        public DbSet<FacultySpecialistResearchField> FacultySpecialistResearchFields { get; set; }
        public DbSet<ResearchTheme> Themes { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<ChallengeEditRequest> ChallengeEditRequests { get; set; }
        public DbSet<ChallengeMatchInvite> ChallengeMatchInvites { get; set; }
        public DbSet<ChallengeMatchedFacultySpecialist> ChallengeMatchedFacultySpecialists { get; set; }
        public DbSet<AdminActionLog> AdminActionLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<UserNotificationSettings> UserNotificationSettings { get; set; }
        public DbSet<NotificationOutbox> NotificationOutbox { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<FileStorage> FileStorage { get; set; }
        public DbSet<RDProject> RDProjects { get; set; }
        public DbSet<RDProjectSupportType> RDProjectSupportTypes { get; set; }
        public DbSet<RDProjectMatchInvite> RDProjectMatchInvites { get; set; }
        public DbSet<RDProjectMatchedFacultySpecialist> RDProjectMatchedFacultySpecialists { get; set; }
        public DbSet<RDProjectEditRequest> RDProjectEditRequests { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        
        // IDataProtectionKeyContext implementation - uses Microsoft's DataProtectionKey type
        DbSet<MicrosoftDataProtectionKey> IDataProtectionKeyContext.DataProtectionKeys => Set<MicrosoftDataProtectionKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Unique Indexes ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email");

            // --- Non-Unique Indexes for Provider Lookups ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.B2CUserId)
                .HasDatabaseName("IX_User_B2CUserId")
                .HasFilter("[B2CUserId] IS NOT NULL");

            // --- One-to-One Profiles ---
            modelBuilder.Entity<FacultySpecialist>()
                .HasKey(fs => fs.UserId);
            modelBuilder.Entity<FacultySpecialist>()
                .HasOne(fs => fs.User)
                .WithOne(u => u.FacultySpecialist)
                .HasForeignKey<FacultySpecialist>(fs => fs.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure table name to match legacy name
            modelBuilder.Entity<FacultySpecialist>()
                .ToTable("FacultySpecialists");

            // --- CommunityPartner ? User relationship ---
            modelBuilder.Entity<CommunityPartner>()
                .HasOne(cp => cp.User)
                .WithOne() // Assuming a one-to-one relationship from User to CommunityPartner
                .HasForeignKey<CommunityPartner>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their partner profile



            // --- Theme ? Challenge ? Participation ---

            // ----------- ResearchField configuration -----------
            modelBuilder.Entity<ResearchField>()
                .ToTable("ResearchFields");

            modelBuilder.Entity<ResearchField>()
                .HasIndex(f => f.Slug)
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL");

            // ----------- FacultySpecialistResearchField configuration -----------
            modelBuilder.Entity<FacultySpecialistResearchField>()
                .ToTable("FacultySpecialistResearchFields");

            modelBuilder.Entity<FacultySpecialistResearchField>()
                .HasOne(fsrf => fsrf.FacultySpecialist)
                .WithMany(fs => fs.ResearchFieldLinks)
                .HasForeignKey(fsrf => fsrf.FacultySpecialistUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FacultySpecialistResearchField>()
                .HasOne(fsrf => fsrf.ResearchField)
                .WithMany(rf => rf.FacultySpecialists)
                .HasForeignKey(fsrf => fsrf.ResearchFieldId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cascade path conflicts

            // Ensure unique constraint: one faculty can only link to each research field once
            modelBuilder.Entity<FacultySpecialistResearchField>()
                .HasIndex(fsrf => new { fsrf.FacultySpecialistUserId, fsrf.ResearchFieldId })
                .IsUnique();

            // ----------- ResearchTheme ? User relationships -----------
            modelBuilder.Entity<ResearchTheme>()
                .ToTable("ResearchThemes");

            modelBuilder.Entity<ResearchTheme>()
                .HasIndex(t => t.Slug)
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL");

            modelBuilder.Entity<ResearchTheme>()
                .HasOne(t => t.UserSubmitted)
                .WithMany(u => u.ThemesSubmitted)
                .HasForeignKey(t => t.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResearchTheme>()
                .HasOne(t => t.UserApproved)
                .WithMany()                           
                .HasForeignKey(t => t.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);   
                                                      


            modelBuilder.Entity<Challenge>()
                .HasOne(c => c.ResearchField)
                .WithMany()
                .HasForeignKey(c => c.ResearchFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Challenge>()
                .HasOne(c => c.UserSubmitted)
                .WithMany(u => u.ChallengesSubmitted)
                .HasForeignKey(c => c.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Challenge>()
                .HasOne(c => c.UserApproved)
                .WithMany()
                .HasForeignKey(c => c.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure estimated cost column is stored as decimal(18,2) (financial precision).
            modelBuilder.Entity<Challenge>()
                .Property(c => c.EstimatedCost)
                .HasColumnType("decimal(18, 2)");

            // Configure ChallengeMatchedFacultySpecialist relationship
            modelBuilder.Entity<ChallengeMatchedFacultySpecialist>()
                .HasOne(cmp => cmp.Challenge)
                .WithMany(c => c.MatchedFacultySpecialists)
                .HasForeignKey(cmp => cmp.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<ChallengeMatchedFacultySpecialist>()
                .HasOne(cmp => cmp.FacultySpecialist)
                .WithMany()
                .HasForeignKey(cmp => cmp.FacultySpecialistUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<ChallengeMatchedFacultySpecialist>()
                .HasOne(cmp => cmp.MatchedByUser)
                .WithMany()
                .HasForeignKey(cmp => cmp.MatchedByUserId)
                .OnDelete(DeleteBehavior.Restrict);



            // --- ChallengeMatchInvite Relationships & Constraints ---
            modelBuilder.Entity<ChallengeMatchInvite>()
                .HasOne(i => i.Challenge)
                .WithMany(c => c.MatchInvites)
                .HasForeignKey(i => i.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChallengeMatchInvite>()
                .HasOne(i => i.FacultySpecialist)
                .WithMany()
                .HasForeignKey(i => i.FacultySpecialistUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChallengeMatchInvite>()
                .HasIndex(i => new { i.ChallengeId, i.FacultySpecialistUserId })
                .IsUnique();

            // --- ChallengeEditRequest Relationships & Constraints ---
            modelBuilder.Entity<ChallengeEditRequest>()
                .HasOne(cer => cer.Challenge)
                .WithMany()
                .HasForeignKey(cer => cer.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChallengeEditRequest>()
                .HasOne(cer => cer.RequestedByUser)
                .WithMany()
                .HasForeignKey(cer => cer.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChallengeEditRequest>()
                .HasOne(cer => cer.RespondedByUser)
                .WithMany()
                .HasForeignKey(cer => cer.RespondedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChallengeEditRequest>()
                .HasIndex(cer => cer.ChallengeId);

            modelBuilder.Entity<ChallengeEditRequest>()
                .HasIndex(cer => cer.RequestedBy);

            modelBuilder.Entity<ChallengeEditRequest>()
                .HasIndex(cer => cer.RespondedBy);

            modelBuilder.Entity<ChallengeEditRequest>()
                .HasIndex(cer => cer.Status);

            // --- AdminActionLog ? User relationship ---
            modelBuilder.Entity<AdminActionLog>()
                .HasOne(log => log.AdminUser)
                .WithMany(u => u.AdminActions)
                .HasForeignKey(log => log.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade); // If admin user is deleted, delete their logs

            // --- Notification ? User relationship ---
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their notifications
                
            // --- Notification unique index for idempotency ---
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.Type, n.ReferenceId })
                .IsUnique()
                .HasFilter("[ReferenceId] IS NOT NULL");

            // --- UserNotificationSettings ? User relationship ---
            modelBuilder.Entity<UserNotificationSettings>()
                .HasOne(s => s.User)
                .WithOne(u => u.NotificationSettings)
                .HasForeignKey<UserNotificationSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their settings
                
            // --- NotificationOutbox ? Notification relationship ---
            modelBuilder.Entity<NotificationOutbox>()
                .HasOne(o => o.Notification)
                .WithMany()
                .HasForeignKey(o => o.NotificationId)
                .OnDelete(DeleteBehavior.Cascade); // If notification is deleted, delete its outbox items

            // --- RDProject Relationships ---
            modelBuilder.Entity<RDProject>()
                .HasOne(p => p.ResearchField)
                .WithMany()
                .HasForeignKey(p => p.ResearchFieldId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RDProject>()
                .HasOne(p => p.UserSubmitted)
                .WithMany()
                .HasForeignKey(p => p.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RDProject>()
                .HasOne(p => p.UserApproved)
                .WithMany()
                .HasForeignKey(p => p.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RDProjectSupportType>()
                .HasOne(st => st.RDProject)
                .WithMany(p => p.SupportTypes)
                .HasForeignKey(st => st.RDProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RDProjectMatchedFacultySpecialist>()
                .HasOne(mp => mp.RDProject)
                .WithMany(p => p.MatchedFacultySpecialists)
                .HasForeignKey(mp => mp.RDProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<RDProjectMatchedFacultySpecialist>()
                .HasOne(mp => mp.FacultySpecialist)
                .WithMany()
                .HasForeignKey(mp => mp.FacultySpecialistUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<RDProjectMatchedFacultySpecialist>()
                .HasOne(mp => mp.MatchedByUser)
                .WithMany()
                .HasForeignKey(mp => mp.MatchedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RDProjectMatchInvite>()
                .HasOne(i => i.RDProject)
                .WithMany(p => p.MatchInvites)
                .HasForeignKey(i => i.RDProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RDProjectMatchInvite>()
                .HasOne(i => i.FacultySpecialist)
                .WithMany()
                .HasForeignKey(i => i.FacultySpecialistUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RDProjectMatchInvite>()
                .HasIndex(i => new { i.RDProjectId, i.FacultySpecialistUserId })
                .IsUnique();

            modelBuilder.Entity<RDProjectEditRequest>()
                .HasOne(r => r.RDProject)
                .WithMany()
                .HasForeignKey(r => r.RDProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RDProjectEditRequest>()
                .HasOne(r => r.RequestedByUser)
                .WithMany()
                .HasForeignKey(r => r.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RDProjectEditRequest>()
                .HasOne(r => r.RespondedByUser)
                .WithMany()
                .HasForeignKey(r => r.RespondedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // --- FileStorage Configuration ---
            modelBuilder.Entity<FileStorage>()
                .ToTable("FileStorage");

            #pragma warning disable CS0618 // Suppress obsolete warnings for HasCheckConstraint until EF configuration is migrated
            // EntityType constraint - must be one of: Challenge, Partner, Theme, ResearchField, RDProject
            modelBuilder.Entity<FileStorage>()
                .HasCheckConstraint("CK_FileStorage_EntityType", 
                    "[EntityType] IN ('Challenge', 'Partner', 'Theme', 'ResearchField', 'RDProject')");

            // File size constraints based on category (5MB for images, 10MB for PDFs)
            modelBuilder.Entity<FileStorage>()
                .HasCheckConstraint("CK_FileStorage_FileSize",
                    "([FileCategory] IN ('Logo', 'Image') AND [FileSize] <= 5242880) OR " +
                    "([FileCategory] IN ('SupportingDocument', 'Document') AND [FileSize] <= 10485760)");
            #pragma warning restore CS0618

            // Index for efficient lookups by entity (allows multiple files per category per entity)
            // Note: Removed unique constraint to support multiple files per category (Phase 7: Multi-file support)
            modelBuilder.Entity<FileStorage>()
                .HasIndex(f => new { f.EntityType, f.EntityId, f.FileCategory, f.IsDeleted })
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_FileStorage_Entity");

            // ContentHash index for integrity verification and queries (non-unique to allow duplicate content)
            modelBuilder.Entity<FileStorage>()
                .HasIndex(f => f.ContentHash)
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_FileStorage_ContentHash");

            // Index on IsDeleted for efficient soft-delete queries
            modelBuilder.Entity<FileStorage>()
                .HasIndex(f => f.IsDeleted)
                .HasDatabaseName("IX_FileStorage_IsDeleted");

            // --- AppSettings Configuration ---
            modelBuilder.Entity<AppSetting>()
                .ToTable("AppSettings");
            modelBuilder.Entity<AppSetting>()
                .HasIndex(s => s.Key)
                .IsUnique()
                .HasDatabaseName("IX_AppSettings_Key");
            modelBuilder.Entity<AppSetting>()
                .HasIndex(s => s.Category)
                .HasDatabaseName("IX_AppSettings_Category");
            modelBuilder.Entity<AppSetting>()
                .HasOne(s => s.UpdatedByUser)
                .WithMany()
                .HasForeignKey(s => s.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Map our DataProtectionKey entity to the same table as Microsoft's DataProtectionKey
            modelBuilder.Entity<RICHConnect.Backend.Domain.Entities.System.DataProtectionKey>()
                .ToTable("DataProtectionKeys");
        }
    }
}
