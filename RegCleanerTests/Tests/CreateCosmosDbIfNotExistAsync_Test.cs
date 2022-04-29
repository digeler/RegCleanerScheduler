using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.VisualBasic;
using Moq;
using RegCleanerScheduler;
using RegCleanerScheduler.Operations;
using Xunit;

namespace Tests ;

    public class CreateCosmosDbIfNotExistAsync_Test
    {
        private readonly ILogger<CosmosClients> _loggercosmos;
        private readonly ILogger<RegistryClient> _loggerregistry;
        private readonly ILogger<DumpAzureContainerRegistryToCosmosAsync> _dumpAzureContainerRegistryToCosmosAsynclogger;

        public CreateCosmosDbIfNotExistAsync_Test()
        {
            _loggercosmos = new NullLogger<CosmosClients>();
            _loggerregistry = new NullLogger<RegistryClient>();
            _dumpAzureContainerRegistryToCosmosAsynclogger = new NullLogger<DumpAzureContainerRegistryToCosmosAsync>();
    }

        [Fact]
        public async Task CreateCosmosDbIfNotExistAsync_ShouldReturnOk()
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
                    async () => await ds.CreateCosmosDbIfNotExistAsync(
                        CancellationToken.None,
                        cosmosClient,
                        TestSettings.CosmosDbName,
                        true));
            }
            catch (Exception ex)
            {
                Assert.Empty(ex.Message);
            }
        }
    }