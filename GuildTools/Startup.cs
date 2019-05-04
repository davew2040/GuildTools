using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using GuildTools.Permissions;
using GuildTools.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GuildTools.Cache;
using GuildTools.ExternalServices;
using GuildTools.Scheduler;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using GuildTools.Services;
using GuildTools.EF;
using GuildTools.Data;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.Services.Mail;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Reflection;
using GuildTools.Cache.SpecificCaches;
using System.Security.Claims;
using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using Microsoft.AspNetCore.Diagnostics;
using GuildTools.ErrorHandling;
using GuildTools.EF.Models;
using System.Collections.Generic;

namespace GuildTools
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.Configure<JwtSettings>(Configuration.GetSection("JWTSettings"));
            services.Configure<BlizzardApiSecrets>(Configuration.GetSection("BlizzardApiSecrets"));
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            
            services
                .AddDbContext<GuildToolsContext>(options => options.UseSqlServer(Configuration.GetValue<string>("ConnectionStrings:Database")));

            services
                .AddIdentity<UserWithData, IdentityRole>(x =>
                {
                    x.Password.RequiredLength = 8;
                    x.ClaimsIdentity.UserIdClaimType = GuildToolsClaims.UserId;
                })
                .AddEntityFrameworkStores<GuildToolsContext>()
                .AddDefaultTokenProviders();
            
            // secretKey contains a secret passphrase only your server knows
            var secretKey = Configuration.GetSection("JWTSettings:SecretKey").Value;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = tokenValidationParameters;
                });

            var logDB = Configuration.GetValue<string>("ConnectionStrings:Database");
            var logTable = "Logs";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    autoCreateSqlTable: true,
                    connectionString: logDB,
                    tableName: logTable)
                .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            IBackgroundTaskQueue backgroundTaskQueue = new BackgroundTaskQueue();

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton(backgroundTaskQueue);

            services.AddScoped<IKeyedResourceManager, KeyedResourceManager>();
            services.AddScoped<IDatabaseCache, DatabaseCache>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IBlizzardService, BlizzardService>();
            services.AddScoped<IGuildService, GuildService>();
            services.AddScoped<IGuildStatsCache, GuildStatsCache>();
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<IOldGuildMemberCache, OldGuildMemberCache>();
            services.AddScoped<IGuildMemberCache, GuildMemberCache>();
            services.AddScoped<IRealmStoreByValues, RealmStoreByValues>();
            services.AddScoped<IPlayerStoreByValue, PlayerStoreByValue>();
            services.AddScoped<IGuildStoreByName, GuildStoreByName>();

            this.InitializeCaches(services);

            IMailSender mailSender = new MailSender(Configuration.GetValue<string>("SendGridApiKey"));
            services.AddSingleton(mailSender);

            ICommonValuesProvider valuesProvider = new CommonValuesProvider()
            {
                AdminEmail = "dwinterm@gmail.com",
                AdminName = "GuildTools Administrator"
            };

            services.AddSingleton(valuesProvider);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "CSV TEST API",
                });

                c.AddSecurityDefinition("Bearer",
                    new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please enter into field the word 'Bearer' following by space and JWT",
                        Name = "Authorization",
                        Type = "apiKey"
                    });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() },
                });

                // comments path
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            IServiceProvider serviceProvider) 
        {
            if (!env.IsDevelopment())
            {
                this.MigrateDatabase(app);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.ConfigureCustomExceptionMiddleware();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSV Test API V1");
                c.RoutePrefix = "api";
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });

            this.UpdateClaims(serviceProvider);
        }

        private void InitializeCaches(IServiceCollection services)
        {
            services.AddScoped<IRealmsCache, RealmsCache>();
            services.AddScoped<IGuildCache, GuildCache>();
            services.AddScoped<IPlayerCache, PlayerCache>();
        }

        private void UpdateClaims(IServiceProvider serviceProvider)
        {
            var adminUsername = "Krom";
            var adminPlayername = "Kromp";
            var adminPlayerRealm = "Burning Blade";
            var adminGuild = "Longanimity";
            var adminGuildRealm = "Burning Blade";
            var adminRegion = "US";

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in GuildToolsRoles.AllRoleNames)
            {
                var role = roleManager.FindByNameAsync(roleName).Result;

                if (role == null)
                {
                    role = new IdentityRole(roleName);

                    roleManager.CreateAsync(role).Wait();
                }
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<UserWithData>>();

            var adminEmail = Configuration.GetValue<string>("AdminCredentials:Email");

            var adminUser = userManager.FindByEmailAsync(adminEmail).Result;

            if (null == adminUser)
            {
                adminUser = new UserWithData { UserName = adminUsername, Email = adminEmail };

                var adminPassword = Configuration.GetValue<string>("AdminCredentials:Password");
                var result = userManager.CreateAsync(adminUser, adminPassword).Result;

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create admin user.");
                }
            }

            if (!userManager.GetClaimsAsync(adminUser).Result.Any(c => c.Type == GuildToolsClaims.UserId))
            {
                userManager.AddClaimAsync(adminUser, new Claim(GuildToolsClaims.UserId, adminUser.Id)).Wait();
            }

            var adminUserRoles = userManager.GetRolesAsync(adminUser).Result;

            if (!adminUserRoles.Contains(GuildToolsRoles.AdminRole.Name))
            {
                userManager.AddToRoleAsync(adminUser, GuildToolsRoles.AdminRole.Name).Wait();
            }

            var context = serviceProvider.GetRequiredService<GuildToolsContext>();

            var efAdminUser = context.UserData.FirstOrDefault(u => u.Id == adminUser.Id);

            efAdminUser.EmailConfirmed = true;
            efAdminUser.Email = adminEmail;
            efAdminUser.UserName = adminUsername;
            efAdminUser.PlayerName = adminPlayername;
            efAdminUser.PlayerRealm = adminPlayerRealm;
            efAdminUser.PlayerRegion = adminRegion;
            efAdminUser.GuildName = adminGuild;
            efAdminUser.GuildRealm = adminGuildRealm;

            context.SaveChanges();
        }

        private void MigrateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {

                serviceScope.ServiceProvider.GetService<GuildToolsContext>()
                    .Database.Migrate();
            }
        }
    }
}
