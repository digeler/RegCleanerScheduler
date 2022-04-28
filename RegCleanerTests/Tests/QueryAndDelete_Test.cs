using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegCleanerScheduler;
using Xunit;
using Xunit.Abstractions;

namespace Tests ;

    public sealed class QueryAndDelete_Test
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<CosmosClients> _loggercosmos;
        private readonly ILogger<RegistryClient> _loggerregistry;
        private readonly ILogger<QueryImagesAndDeleteAsync> _queryimageanddeletelogger;

    public QueryAndDelete_Test(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _loggercosmos = new NullLogger<CosmosClients>();
            _loggerregistry = new NullLogger<RegistryClient>();
            _queryimageanddeletelogger =  new NullLogger<QueryImagesAndDeleteAsync>();
    }

        [Fact]
        public async Task QueryAndDelete_Should_Return_Ok()
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
                var qanddelete = new QueryImagesAndDeleteAsync( _queryimageanddeletelogger, cl, rl);
                await Task.Run(()=>
                 qanddelete.QueryAndDeleteAsync(
                    CancellationToken.None,
                     cosmosClient,
                    TestSettings.CosmosDbName,
                    TestSettings.CosmosContainerName,
                    TestSettings.MonthsBack,
                    true));
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(ex.Message);
                Assert.Empty(ex.GetBaseException().HResult.ToString());
            }
        }
    }