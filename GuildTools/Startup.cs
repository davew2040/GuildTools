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
                .AddIdentity<IdentityUser, IdentityRole>(x =>
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

            BlizzardApiSecrets blizzardSecrets = new BlizzardApiSecrets()
            {
                ClientId = Configuration.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = Configuration.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            IBlizzardService blizzardService = new BlizzardService(
                Configuration.GetValue<string>("ConnectionStrings:Database"), 
                blizzardSecrets);

            IGuildMemberService guildMemberService = new GuildMemberService(blizzardService);
            IRaiderIoService raiderIoService = new RaiderIoService();

            services.AddScoped<IKeyedResourceManager, KeyedResourceManager>();
            services.AddSingleton(guildMemberService);
            services.AddScoped<IDatabaseCache, DatabaseCache>();
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddSingleton(blizzardService);
            services.AddSingleton(raiderIoService);
            services.AddSingleton<IGuildCache>(new GuildCache(Configuration, blizzardService));
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<IGuildMemberCache, GuildMemberCache>();

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
            
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSV Test API V1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
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
            services.AddScoped<RealmsCache>();
        }

        private void UpdateClaims(IServiceProvider serviceProvider)
        {
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

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var adminEmail = Configuration.GetValue<string>("AdminCredentials:Email");

            var adminUser = userManager.FindByEmailAsync(adminEmail).Result;

            if (null == adminUser)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };

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

            if (context.UserData.FirstOrDefault(u => u.UserId == adminUser.Id) == null)
            {
                context.UserData.Add(new EF.Models.UserData()
                {
                    UserId = adminUser.Id,
                    GuildName = "Longanimity",
                    Username = "Krom",
                    GuildRealm = "burning-blade"
                });
                context.SaveChanges();
            }
        }

        private void MigrateDatabase(IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {

                    serviceScope.ServiceProvider.GetService<GuildToolsContext>()
                        .Database.Migrate();
                }
            }
            catch (Exception e)
            {
                var msg = e.Message;
                var stacktrace = e.StackTrace;
            }
        }
    }
}
