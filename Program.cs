using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KeyVault.PnP.Job
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            builder
                .UseEnvironment(string.IsNullOrEmpty(environmentName) ? Environments.Production : environmentName)
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        var secretClient = new SecretClient(new Uri("https://sp-keys.vault.azure.net/"), new DefaultAzureCredential(new DefaultAzureCredentialOptions
                        {
                            ExcludeVisualStudioCredential = true
                        }));
                        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    }

                    config.AddEnvironmentVariables();

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                .ConfigureWebJobs(b =>
                {
                    b
                    .AddAzureStorageCoreServices();
                })
                .ConfigureLogging((context, logBuilder) =>
                {
                    logBuilder
                    .AddConsole()
                    .AddDebug();
                })
                .ConfigureServices((context, services) =>
                {
                    var azureCreds = context.Configuration.GetSection(AzureAdCreds.SectionName).Get<AzureAdCreds>();
                    services.AddSingleton(azureCreds);
                });
            var host = builder.Build();
            using (host)
            {
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
                await host.StartAsync();
                await jobHost.CallAsync("Process");
                await host.StopAsync();
            }
        }
    }
}
