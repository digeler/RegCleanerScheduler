using Azure.Identity;
using Microsoft.Azure.Cosmos;
using RegCleanerScheduler;

namespace RegCleanerScheduler ;

    public class CosmosClients : ICosmosClient
    {
        private readonly ILogger<CosmosClients> _logger;

        public CosmosClients(ILogger<CosmosClients> logger) => _logger = logger;

        public async Task<CosmosClient> GetCosmosClientAsync() =>
            await Task.Run(
                () => new CosmosClient($"https://{GlobalSettings.CosmosDbName}.{GlobalSettings.CosmosSuffix}:443/", new ManagedIdentityCredential()));
    }