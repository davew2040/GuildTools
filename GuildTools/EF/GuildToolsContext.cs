using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GuildTools.EF.Models;
using EfEnums = GuildTools.EF.Models.Enums;
using EfModels = GuildTools.EF.Models;
using GuildTools.EF.Models.Enums;

namespace GuildTools.EF
{
    public partial class GuildToolsContext : IdentityDbContext<IdentityUser>
    {
        public GuildToolsContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<BigValueCache> BigValueCache { get; set; }
        public virtual DbSet<GuildProfile> GuildProfile { get; set; }
        public virtual DbSet<EfModels.GuildProfilePermissionLevel> GuildProfilePermissions { get; set; }
        public virtual DbSet<User_GuildProfilePermissions> User_GuildProfilePermissions { get; set; }
        public virtual DbSet<UserData> UserData { get; set; }
        public virtual DbSet<ValueStore> ValueStore { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<EfModels.GuildProfilePermissionLevel>().ToTable("GuildProfilePermissionLevels");

            modelBuilder.Entity<EfModels.GameRegion>().ToTable("GameRegions");

            modelBuilder.Entity<BigValueCache>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__BigValue__8D8F664F9682143E");

                entity.Property(e => e.Id).HasMaxLength(50);

                entity.Property(e => e.ExpiresOn).HasColumnType("datetime");

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<GuildProfile>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatorId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.GuildName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Realm)
                    .IsRequired()
                    .HasMaxLength(100);

                entity
                    .HasOne(e => e.Region)
                    .WithMany(f => f.GuildProfiles)
                    .HasForeignKey(e => e.RegionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_GuildProfile_GameRegion");

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.GuildProfile)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_GuildProfile_ToTable");
            });

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__UserData__1788CC4C5D60148F");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_Users_Column");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.GuildName).HasMaxLength(50);

                entity.Property(e => e.GuildRealm).HasMaxLength(50);

                entity.Property(e => e.Username).HasMaxLength(50);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserData)
                    .HasForeignKey<UserData>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Users_AspNetUsers");
            });

            modelBuilder.Entity<ValueStore>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<User_GuildProfilePermissions>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.PermissionLevelId, e.ProfileId });

                entity
                    .HasOne(e => e.PermissionLevel)
                    .WithMany(f => f.User_GuildProfilePermissions)
                    .HasForeignKey(e => e.PermissionLevelId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK__GuildProfilePermissions_GuildPermissions");

                entity
                    .HasOne(e => e.Profile)
                    .WithMany(f => f.User_GuildProfilePermissions)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK__GuildProfilePermissions_GuildProfiles");

                entity
                    .HasOne(e => e.User)
                    .WithMany(f => f.GuildProfilePermissions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK__GuildProfilePermissions_AspNetUsers");
            });

            modelBuilder.Ignore<AspNetRoleClaims>();
            modelBuilder.Ignore<AspNetRoles>();
            modelBuilder.Ignore<AspNetUserClaims>();
            modelBuilder.Ignore<AspNetUserLogins>();
            modelBuilder.Ignore<AspNetUserRoles>();
            modelBuilder.Ignore<AspNetUsers>();

            this.AddStaticEnumData(modelBuilder);
        }

        private void AddStaticEnumData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EfModels.GuildProfilePermissionLevel>().HasData(
                EnumUtilities.GetEnumValues<EfEnums.GuildProfilePermissionLevel>().Select(p => new EfModels.GuildProfilePermissionLevel()
                {
                    Id = (int)p,
                    PermissionName = p.ToString()
                }));

            modelBuilder.Entity<EfModels.GameRegion>().HasData(
                EnumUtilities.GetEnumValues<EfEnums.GameRegion>().Select(p => new EfModels.GameRegion()
                {
                    Id = (int)p,
                    RegionName = p.ToString()
                }));
        }
    }
}
