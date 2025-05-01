using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Homo.Bet.Api
{
    public partial class BargainingChipDBContext : DbContext
    {
        public BargainingChipDBContext() { }

        public BargainingChipDBContext(DbContextOptions<BargainingChipDBContext> options) : base(options) { }
        public virtual DbSet<RelationOfOrganizationAndUser> RelationOfOrganizationAndUser { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<VerifyCode> VerifyCode { get; set; }
        public virtual DbSet<RelationOfGroupAndUser> RelationOfGroupAndUser { get; set; }
        public virtual DbSet<Group> Group { get; set; }

        public virtual DbSet<Organization> Organization { get; set; }

        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Task> Task { get; set; }
        public virtual DbSet<CoinLog> CoinLog { get; set; }
        public virtual DbSet<Reward> Reward { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            OnModelCreatingPartial(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.Email });
                entity.HasIndex(p => new { p.Status });
                entity.HasIndex(p => new { p.IsManager });
                entity.HasIndex(p => new { p.Username });
                entity.HasIndex(p => new { p.FirstName });
                entity.HasIndex(p => new { p.LastName });
                entity.HasIndex(p => new { p.Gender });
                entity.HasIndex(p => new { p.County });
                entity.HasIndex(p => new { p.City });
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.Name });
                entity.HasIndex(p => new { p.Roles });
            });

            modelBuilder.Entity<VerifyCode>(entity =>
            {
                entity.HasIndex(p => new { p.Expiration });
                entity.HasIndex(p => new { p.Code });
                entity.HasIndex(p => new { p.Ip });
                entity.HasIndex(p => new { p.Phone });
                entity.HasIndex(p => new { p.Email });
                entity.HasIndex(p => new { p.IsUsed });
            });

            modelBuilder.Entity<RelationOfGroupAndUser>(entity =>
            {
                entity.HasIndex(p => new { p.UserId });
                entity.HasIndex(p => new { p.GroupId });
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasIndex(p => new { p.Name });
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.OwnerId });
            });

            modelBuilder.Entity<RelationOfOrganizationAndUser>(entity =>
            {
                entity.HasIndex(p => new { p.UserId });
                entity.HasIndex(p => new { p.OrganizationId });
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId);
                entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasIndex(p => new { p.Name });
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.OwnerId });
                entity.HasIndex(p => new { p.OrganizationId });
                entity.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId);
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasIndex(p => new { p.Name });
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.Type });
                entity.HasIndex(p => new { p.ExternalId });
                entity.HasIndex(p => new { p.AssigneeId });
                entity.HasOne(p => p.Project).WithMany().HasForeignKey(p => p.ProjectId);
                entity.HasOne(p => p.Assignee).WithMany().HasForeignKey(p => p.AssigneeId);
            });

            modelBuilder.Entity<CoinLog>(entity =>
            {
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.OwnerId });
                entity.HasIndex(p => new { p.TaskId });
                entity.HasIndex(p => new { p.IsLock });
                entity.HasOne(p => p.Task).WithMany().HasForeignKey(p => p.TaskId);
            });

            modelBuilder.Entity<Reward>(entity =>
            {
                entity.HasIndex(p => new { p.DeletedAt });
                entity.HasIndex(p => new { p.Type });
                entity.HasIndex(p => new { p.LeaveDate });
            });
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


    }
}