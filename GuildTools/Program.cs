using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Logging;
using System.IO;

namespace GuildTools
{
    public class Program
    {
        private static string GetKeyVaultEndpoint() => "https://davew2040-key-vault.vault.azure.net/";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            BuildWebHost(args)
            .UseStartup<Startup>();

        private static IWebHostBuilder BuildWebHost(string[] args)
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    if (!env.IsDevelopment())
                    {
                        var keyVaultEndpoint = GetKeyVaultEndpoint();
                        if (!string.IsNullOrEmpty(keyVaultEndpoint))
                        {
                            var azureServiceTokenProvider = new AzureServiceTokenProvider();
                            var keyVaultClient = new KeyVaultClient(
                                new KeyVaultClient.AuthenticationCallback(
                                    azureServiceTokenProvider.KeyVaultTokenCallback));
                            config.AddAzureKeyVault(
                                keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                        }
                    }
                    if (env.IsDevelopment())
                    {
                        config.AddUserSecrets<Startup>();
                    }

                    config
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile(
                            $"appsettings.{env.EnvironmentName}.json",
                            optional: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseIISIntegration()
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes =
                    context.HostingEnvironment.IsDevelopment();
                });

            return builder;
        }
    }
}
