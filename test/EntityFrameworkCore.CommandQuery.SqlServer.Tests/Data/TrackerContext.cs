using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data
{
    public partial class TrackerContext : DbContext
    {
        public TrackerContext(DbContextOptions<TrackerContext> options)
            : base(options)
        {
        }

        #region Generated Properties
        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.Audit> Audits { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.Priority> Priorities { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.Role> Roles { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.Status> Statuses { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.TaskExtended> TaskExtendeds { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.Task> Tasks { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.Tenant> Tenants { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.UserLogin> UserLogins { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.UserRole> UserRoles { get; set; }

        public virtual DbSet<EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Entities.User> Users { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Generated Configuration
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.AuditMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.PriorityMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.RoleMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.StatusMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.TaskExtendedMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.TaskMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.TenantMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.UserLoginMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.UserMap());
            modelBuilder.ApplyConfiguration(new EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data.Mapping.UserRoleMap());
            #endregion
        }
    }
}
