using Microsoft.Azure.Cosmos;

namespace RegCleanerScheduler ;

    public interface ICosmosClient
    {
        public Task<CosmosClient> GetCosmosClientAsync();
    }