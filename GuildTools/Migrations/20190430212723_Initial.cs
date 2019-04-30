using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BigValueCache",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 50, nullable: false),
                    Value = table.Column<string>(nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BigValueCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameRegions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RegionName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildProfilePermissionLevels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PermissionName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildProfilePermissionLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserData",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Username = table.Column<string>(maxLength: 50, nullable: true),
                    GuildName = table.Column<string>(maxLength: 50, nullable: true),
                    GuildRealm = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserData", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ValueStore",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 50, nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueStore", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoredRealms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: false),
                    RegionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredRealms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredRealms_GameRegions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "GameRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuildProfile",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProfileName = table.Column<string>(maxLength: 150, nullable: false),
                    CreatorId = table.Column<string>(nullable: false),
                    RealmId = table.Column<int>(nullable: false),
                    CreatorGuildId = table.Column<int>(nullable: true),
                    GameRegionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildProfile_UserData_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserData",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildProfile_GameRegions_GameRegionId",
                        column: x => x.GameRegionId,
                        principalTable: "GameRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuildProfile_StoredRealms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "StoredRealms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoredGuilds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    RealmId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredGuilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredGuilds_GuildProfile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoredGuilds_StoredRealms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "StoredRealms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User_GuildProfilePermissions",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    PermissionLevelId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_GuildProfilePermissions", x => new { x.UserId, x.PermissionLevelId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK__GuildProfilePermissions_GuildPermissions",
                        column: x => x.PermissionLevelId,
                        principalTable: "GuildProfilePermissionLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__GuildProfilePermissions_GuildProfiles",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAlts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PlayerMainId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAlts", x => x.Id);
                    table.UniqueConstraint("AK_PlayerAlts_PlayerId", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_PlayerAlts_GuildProfile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoredPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    RealmId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    GuildId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredPlayers_StoredGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "StoredGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoredPlayers_PlayerAlts_Id",
                        column: x => x.Id,
                        principalTable: "PlayerAlts",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoredPlayers_GuildProfile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoredPlayers_StoredRealms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "StoredRealms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMains",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProfileId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: true),
                    Notes = table.Column<string>(maxLength: 4000, nullable: true),
                    OfficerNotes = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMains_StoredPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "StoredPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMains_GuildProfile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GameRegions",
                columns: new[] { "Id", "RegionName" },
                values: new object[,]
                {
                    { 1, "US" },
                    { 2, "EU" }
                });

            migrationBuilder.InsertData(
                table: "GuildProfilePermissionLevels",
                columns: new[] { "Id", "PermissionName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Officer" },
                    { 3, "Member" },
                    { 4, "Visitor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProfile_CreatorGuildId",
                table: "GuildProfile",
                column: "CreatorGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProfile_CreatorId",
                table: "GuildProfile",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProfile_GameRegionId",
                table: "GuildProfile",
                column: "GameRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProfile_Id",
                table: "GuildProfile",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GuildProfile_RealmId",
                table: "GuildProfile",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAlts_Id",
                table: "PlayerAlts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAlts_PlayerMainId",
                table: "PlayerAlts",
                column: "PlayerMainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAlts_ProfileId",
                table: "PlayerAlts",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMains_Id",
                table: "PlayerMains",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMains_PlayerId",
                table: "PlayerMains",
                column: "PlayerId",
                unique: true,
                filter: "[PlayerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMains_ProfileId",
                table: "PlayerMains",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredGuilds_Id",
                table: "StoredGuilds",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StoredGuilds_ProfileId",
                table: "StoredGuilds",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredGuilds_RealmId",
                table: "StoredGuilds",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_GuildId",
                table: "StoredPlayers",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_Id",
                table: "StoredPlayers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_ProfileId",
                table: "StoredPlayers",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_RealmId",
                table: "StoredPlayers",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredRealms_Id",
                table: "StoredRealms",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StoredRealms_RegionId",
                table: "StoredRealms",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredRealms_Id_RegionId",
                table: "StoredRealms",
                columns: new[] { "Id", "RegionId" });

            migrationBuilder.CreateIndex(
                name: "IX_User_GuildProfilePermissions_PermissionLevelId",
                table: "User_GuildProfilePermissions",
                column: "PermissionLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_User_GuildProfilePermissions_ProfileId",
                table: "User_GuildProfilePermissions",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Column",
                table: "UserData",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildProfile_StoredGuilds_CreatorGuildId",
                table: "GuildProfile",
                column: "CreatorGuildId",
                principalTable: "StoredGuilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAlts_PlayerMains_PlayerMainId",
                table: "PlayerAlts",
                column: "PlayerMainId",
                principalTable: "PlayerMains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildProfile_StoredGuilds_CreatorGuildId",
                table: "GuildProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredPlayers_StoredGuilds_GuildId",
                table: "StoredPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildProfile_UserData_CreatorId",
                table: "GuildProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildProfile_GameRegions_GameRegionId",
                table: "GuildProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredRealms_GameRegions_RegionId",
                table: "StoredRealms");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildProfile_StoredRealms_RealmId",
                table: "GuildProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredPlayers_StoredRealms_RealmId",
                table: "StoredPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAlts_PlayerMains_PlayerMainId",
                table: "PlayerAlts");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BigValueCache");

            migrationBuilder.DropTable(
                name: "User_GuildProfilePermissions");

            migrationBuilder.DropTable(
                name: "ValueStore");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "GuildProfilePermissionLevels");

            migrationBuilder.DropTable(
                name: "StoredGuilds");

            migrationBuilder.DropTable(
                name: "UserData");

            migrationBuilder.DropTable(
                name: "GameRegions");

            migrationBuilder.DropTable(
                name: "StoredRealms");

            migrationBuilder.DropTable(
                name: "PlayerMains");

            migrationBuilder.DropTable(
                name: "StoredPlayers");

            migrationBuilder.DropTable(
                name: "PlayerAlts");

            migrationBuilder.DropTable(
                name: "GuildProfile");
        }
    }
}
