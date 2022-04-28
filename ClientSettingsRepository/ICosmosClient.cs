using Microsoft.Azure.Cosmos;

namespace RegCleanerScheduler ;

    public interface ICosmosClient
    {
        public Task<CosmosClient> GetCosmosClientAsyncForTestingOnly(
            string azureManagmentSuffix,
            string subscriptionid,
            string resourcegroup,
            string cosmosdbaccountName,
            string cosmosSuffix,
            CancellationToken cancellationToken);

        public Task<CosmosClient> GetCosmosClientWithKeysAsync(CancellationToken cancellationToken);
    }