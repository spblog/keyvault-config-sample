using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using System.Threading.Tasks;

namespace KeyVault.PnP.Job
{
    public class Functions
    {
        private readonly AzureAdCreds _azureCreds;
        public Functions(AzureAdCreds creds)
        {
            _azureCreds = creds;
        }

        [NoAutomaticTrigger]
        public async Task Process(ILogger logger)
        {
            var authManager = new AuthenticationManager(_azureCreds.ClientId, _azureCreds.Certificate, _azureCreds.TenantId);
            var clientContext = await authManager.GetContextAsync("https://mastaq.sharepoint.com/sites/PnPDroneDemo");
            clientContext.Load(clientContext.Web);
            await clientContext.ExecuteQueryRetryAsync();

            logger.LogInformation($"Web title: {clientContext.Web.Title}");
        }
    }
}
