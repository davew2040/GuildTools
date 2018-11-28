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
using System.Threading.Tasks;
using GuildTools.Permissions;
using GuildTools.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GuildTools.Controllers.Cache;
using GuildTools.ExternalServices;
using GuildTools.Scheduler;
using Serilog;
using Serilog.Sinks.MSSqlServer;

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
            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<JwtSettings>(Configuration.GetSection("JWTSettings"));
            services.Configure<BlizzardApiSecrets>(Configuration.GetSection("BlizzardApiSecrets"));
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services
                .AddDbContext<UserDbContext>(options => options.UseSqlServer(Configuration.GetValue<string>("ConnectionStrings:Database")));

            services
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<UserDbContext>();
            
            // secretKey contains a secret passphrase only your server knows
            var secretKey = Configuration.GetSection("JWTSettings:SecretKey").Value;
            var issuer = Configuration.GetSection("JWTSettings:Issuer").Value;
            var audience = Configuration.GetSection("JWTSettings:Audience").Value;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = issuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = audience
            };

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(options =>
                {
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
            services.AddSingleton<IBackgroundTaskQueue>(backgroundTaskQueue);

            BlizzardApiSecrets blizzardSecrets = new BlizzardApiSecrets()
            {
                ClientId = Configuration.GetValue<string>("BlizzardApiSecrets:ClientId"),
                ClientSecret = Configuration.GetValue<string>("BlizzardApiSecrets:ClientSecret")
            };

            IBlizzardService blizzardService = new BlizzardService(
                Configuration.GetValue<string>("ConnectionStrings:Database"), 
                blizzardSecrets);
            IRaiderIoService raiderIoService = new RaiderIoService();
            
            services.AddSingleton<IBlizzardService>(blizzardService);
            services.AddSingleton<IRaiderIoService>(raiderIoService);
            services.AddSingleton<IGuildCache>(new GuildCache(Configuration, blizzardService));
            services.AddSingleton<IGuildMemberCache>(new GuildMemberCache(Configuration, blizzardService, raiderIoService, backgroundTaskQueue));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            IServiceProvider serviceProvider)
        { 
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

            this.CreateRoles(serviceProvider);
        }

        private void CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var adminExists = roleManager.RoleExistsAsync(GuildToolsRoles.SuperAdmin);
            adminExists.Wait();

            if (!adminExists.Result)
            {
                roleManager.CreateAsync(new IdentityRole(GuildToolsRoles.SuperAdmin)).Wait();
            }

            var standardExists = roleManager.RoleExistsAsync(GuildToolsRoles.Standard);
            standardExists.Wait();

            if (!standardExists.Result)
            {
                roleManager.CreateAsync(new IdentityRole(GuildToolsRoles.Standard)).Wait();
            }
        }
    }
}
