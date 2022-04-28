using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest.Serialization;
using RegCleanerScheduler;
using RegCleanerScheduler.Operations;
using Xunit;
using Xunit.Abstractions;

namespace Tests ;

    public class DumpToCosmosAsync_Test
    {

    
        private readonly ILogger<CosmosClients> _loggercosmos;
        private readonly ILogger<RegistryClient> _loggerregistry;
        private readonly ILogger<DumpAzureContainerRegistryToCosmosAsync> _DumpAzureContainerRegistryToCosmosAsynclogger;

    public DumpToCosmosAsync_Test(ITestOutputHelper testOutputHelper)
        {

        _loggercosmos = new NullLogger<CosmosClients>();
        _loggerregistry = new NullLogger<RegistryClient>();
        _DumpAzureContainerRegistryToCosmosAsynclogger = new NullLogger<DumpAzureContainerRegistryToCosmosAsync>();
    }

        [Fact]
        public async Task DumpToCosmosAsync_Should_Return_Ok()
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
                var regc = await rl.GetContainerRegistryClientAsync(CancellationToken.None, TestSettings.RegEndpoint, "Public");
                var ds = new DumpAzureContainerRegistryToCosmosAsync(_DumpAzureContainerRegistryToCosmosAsynclogger, rl, cl);
                await Task.Run(
                    () =>
                        ds.DumpToCosmosAsync(
                            CancellationToken.None,
                            regc,
                            true));
                
            }
            catch (Exception ex)
            {
                Assert.Empty(ex.GetBaseException().HResult.ToString());
            }
        }
    }