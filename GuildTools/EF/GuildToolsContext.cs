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
using GuildTools.EF.Models.StoredBlizzardModels;

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
        public virtual DbSet<UserWithData> UserData { get; set; }
        public virtual DbSet<ValueStore> ValueStore { get; set; }
        public virtual DbSet<PlayerMain> PlayerMains { get; set; }
        public virtual DbSet<PlayerAlt> PlayerAlts { get; set; }
        public virtual DbSet<StoredRealm> StoredRealms { get; set; }
        public virtual DbSet<StoredPlayer> StoredPlayers { get; set; }
        public virtual DbSet<StoredGuild> StoredGuilds { get; set; }
        public virtual DbSet<PendingAccessRequest> PendingAccessRequests { get; set; }

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
                entity.Property(e => e.Id).HasMaxLength(50);

                entity.Property(e => e.ExpiresOn).HasColumnType("datetime");

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<GuildProfile>(entity =>
            {
                entity.HasIndex(e => e.Id);

                entity
                    .HasOne(e => e.Creator)
                    .WithMany(f => f.GuildProfiles)
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(e => e.Realm)
                    .WithMany(f => f.Profiles)
                    .HasForeignKey(e => e.RealmId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(e => e.CreatorGuild)
                    .WithMany(f => f.CreatorGuilds)
                    .HasForeignKey(e => e.CreatorGuildId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<UserWithData>(entity =>
            {
                entity.Property(e => e.GuildName).HasMaxLength(50);

                entity.Property(e => e.GuildRealm).HasMaxLength(50);
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
                entity.HasKey(e => new { e.UserId, e.ProfileId });

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
            });

            modelBuilder.Entity<StoredRealm>(entity =>
            {
                entity.HasIndex(e => e.Id);

                entity.HasIndex(e => new { e.Id, e.RegionId });

                entity.HasOne(e => e.Region)
                    .WithMany(f => f.Realms)
                    .HasForeignKey(e => e.RegionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StoredGuild>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.ProfileId);

                entity.HasOne(e => e.Realm)
                    .WithMany(f => f.Guilds)
                    .HasForeignKey(e => e.RealmId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Profile)
                    .WithMany(f => f.Guilds)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StoredPlayer>(entity =>
            {
                entity.HasIndex(e => e.Id);

                entity.HasOne(e => e.Profile)
                    .WithMany(f => f.Players)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Realm)
                    .WithMany(f => f.Players)
                    .HasForeignKey(e => e.RealmId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Guild)
                    .WithMany(f => f.StoredPlayers)
                    .HasForeignKey(e => e.GuildId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PlayerMain>(entity =>
            {
                entity.HasIndex(e => e.Id );
                entity.HasIndex(e => e.ProfileId);

                entity.HasOne(e => e.Profile)
                    .WithMany(f => f.PlayerMains)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Player)
                    .WithOne(f => f.Main)
                    .HasForeignKey<PlayerMain>(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Alts)
                    .WithOne(f => f.PlayerMain)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PlayerAlt>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.PlayerMainId);
                entity.HasIndex(e => e.ProfileId);

                entity.HasOne(e => e.Profile)
                    .WithMany(f => f.PlayerAlts)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Player)
                    .WithOne(f => f.Alt)
                    .HasForeignKey<PlayerAlt>(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<PendingAccessRequest>(entity =>
            {
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.ProfileId);

                entity.HasOne(e => e.Profile)
                    .WithMany(f => f.AccessRequests)
                    .HasForeignKey(e => e.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Requester)
                    .WithMany(f => f.PendingAccessRequests)
                    .HasForeignKey(e => e.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

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
