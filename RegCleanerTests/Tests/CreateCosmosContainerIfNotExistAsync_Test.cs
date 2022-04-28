using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegCleanerScheduler;
using RegCleanerScheduler.Operations;
using Xunit;

namespace Tests ;

    public class CreateCosmosContainerIfNotExistAsync_Test
    {
    
        private readonly ILogger<CosmosClients> _loggercosmos;
        private readonly ILogger<RegistryClient> _loggerregistry;
        private readonly ILogger<DumpAzureContainerRegistryToCosmosAsync> _dumpAzureContainerRegistryToCosmosAsynclogger;

        public CreateCosmosContainerIfNotExistAsync_Test()
        {
            _loggercosmos = new NullLogger<CosmosClients>();
            _loggerregistry = new NullLogger<RegistryClient>();
            _dumpAzureContainerRegistryToCosmosAsynclogger= new NullLogger<DumpAzureContainerRegistryToCosmosAsync>();
    }

        [Fact]
        public async Task CreateCosmosContainerIfNotExistAsync_Should_Return_Ok()
        {
            var ts = new TestSettings();
            try
            {
                var cl = new CosmosClients(_loggercosmos);
                var cosmosClient = await cl.GetCosmosClientAsyncForTestingOnly(
                    TestSettings.AzureManagmentSuffix,
                    TestSettings.SubscriptionId,
                    TestSettings.ResourceGroup,
                    TestSettings.CosmosDbAccountName,
                    TestSettings.CosmosSuffix,
                    CancellationToken.None);

                var rl = new RegistryClient(_loggerregistry);
                await rl.GetContainerRegistryClientAsync(CancellationToken.None, TestSettings.RegEndpoint, "Public");
                var ds = new DumpAzureContainerRegistryToCosmosAsync(_dumpAzureContainerRegistryToCosmosAsynclogger, rl, cl);
                await Task.Run(
                    async () => await ds.CreateCosmosContainerIfNotExistAsync(
                        CancellationToken.None,
                        cosmosClient,
                        TestSettings.CosmosDbName,
                        TestSettings.CosmosContainerName,
                        TestSettings.CosmosPartitionKey,
                        TestSettings.CosmosDbAccountName,
                        true));
            }
            catch (Exception ex)
            {
               Assert.Empty(ex.Message); 
            }
        }
    }