﻿// <auto-generated />
using System;
using GuildTools.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GuildTools.Migrations
{
    [DbContext(typeof(GuildToolsContext))]
    partial class GuildToolsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GuildTools.EF.Models.BigValueCache", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50);

                    b.Property<DateTime>("ExpiresOn")
                        .HasColumnType("datetime");

                    b.Property<string>("Value")
                        .IsRequired();

                    b.HasKey("Id")
                        .HasName("PK__BigValue__8D8F664F9682143E");

                    b.ToTable("BigValueCache");
                });

            modelBuilder.Entity("GuildTools.EF.Models.GameRegion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("RegionName");

                    b.HasKey("Id");

                    b.ToTable("GameRegions");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            RegionName = "US"
                        },
                        new
                        {
                            Id = 2,
                            RegionName = "EU"
                        });
                });

            modelBuilder.Entity("GuildTools.EF.Models.GuildProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatorId")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<string>("GuildName")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("ProfileName")
                        .IsRequired()
                        .HasMaxLength(150);

                    b.Property<string>("Realm")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<int>("RegionId");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("Id");

                    b.HasIndex("RegionId");

                    b.ToTable("GuildProfile");
                });

            modelBuilder.Entity("GuildTools.EF.Models.GuildProfilePermissionLevel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("PermissionName");

                    b.HasKey("Id");

                    b.ToTable("GuildProfilePermissionLevels");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            PermissionName = "Admin"
                        },
                        new
                        {
                            Id = 2,
                            PermissionName = "Officer"
                        },
                        new
                        {
                            Id = 3,
                            PermissionName = "Member"
                        },
                        new
                        {
                            Id = 4,
                            PermissionName = "Visitor"
                        });
                });

            modelBuilder.Entity("GuildTools.EF.Models.UserData", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("GuildName")
                        .HasMaxLength(50);

                    b.Property<string>("GuildRealm")
                        .HasMaxLength(50);

                    b.Property<string>("Username")
                        .HasMaxLength(50);

                    b.HasKey("UserId")
                        .HasName("PK__UserData__1788CC4C5D60148F");

                    b.HasIndex("UserId")
                        .HasName("IX_Users_Column");

                    b.ToTable("UserData");
                });

            modelBuilder.Entity("GuildTools.EF.Models.User_GuildProfilePermissions", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<int>("PermissionLevelId");

                    b.Property<int>("ProfileId");

                    b.HasKey("UserId", "PermissionLevelId", "ProfileId");

                    b.HasIndex("PermissionLevelId");

                    b.HasIndex("ProfileId");

                    b.ToTable("User_GuildProfilePermissions");
                });

            modelBuilder.Entity("GuildTools.EF.Models.ValueStore", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50);

                    b.Property<string>("Value")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("ValueStore");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("GuildTools.EF.Models.GuildProfile", b =>
                {
                    b.HasOne("GuildTools.EF.Models.UserData", "Creator")
                        .WithMany("GuildProfiles")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("GuildTools.EF.Models.GameRegion", "Region")
                        .WithMany("GuildProfiles")
                        .HasForeignKey("RegionId")
                        .HasConstraintName("FK_GuildProfile_GameRegion")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("GuildTools.EF.Models.User_GuildProfilePermissions", b =>
                {
                    b.HasOne("GuildTools.EF.Models.GuildProfilePermissionLevel", "PermissionLevel")
                        .WithMany("User_GuildProfilePermissions")
                        .HasForeignKey("PermissionLevelId")
                        .HasConstraintName("FK__GuildProfilePermissions_GuildPermissions")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("GuildTools.EF.Models.GuildProfile", "Profile")
                        .WithMany("User_GuildProfilePermissions")
                        .HasForeignKey("ProfileId")
                        .HasConstraintName("FK__GuildProfilePermissions_GuildProfiles")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
