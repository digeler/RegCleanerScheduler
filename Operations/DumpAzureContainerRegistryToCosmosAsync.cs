using System.Security.Authentication;
using Coravel.Invocable;
using Microsoft.Azure.Cosmos;
using RegCleanerScheduler;

namespace RegCleanerScheduler.Operations ;

    public class DumpAzureContainerRegistryToCosmosAsync : IInvocable
    {
        private readonly ILogger<DumpAzureContainerRegistryToCosmosAsync> _logger;
        private readonly IRegistryClient _regclient;
        private readonly ICosmosClient _cosmosclient;

        public DumpAzureContainerRegistryToCosmosAsync(
            ILogger<DumpAzureContainerRegistryToCosmosAsync> logger,
            IRegistryClient regClient,
            ICosmosClient cosmosClient)
        {
            _logger = logger;
            _regclient = regClient;
            _cosmosclient = cosmosClient;
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"Coravel was invoked with {nameof(DumpAzureContainerRegistryAsync)}");
            _logger.LogInformation("Starting Wait 20 sec for debugging");
            await Task.Delay(20000);

            await DumpAzureContainerRegistryAsync(CancellationToken.None);
        }

        public async Task DumpAzureContainerRegistryAsync(CancellationToken cancellationToken)
        {
            await CreateCosmosDbIfNotExistAsync(cancellationToken);
            await CreateCosmosContainerIfNotExistAsync(cancellationToken);
            await DumpToCosmosAsync(cancellationToken);
        }

        private async Task CreateCosmosDbIfNotExistAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started operation {nameof(CreateCosmosDbIfNotExistAsync)}");
            try
            {
                var cosmosclient = await _cosmosclient.GetCosmosClientAsync().Result.CreateDatabaseIfNotExistsAsync(
                    GlobalSettings.CosmosDbName,
                    GlobalSettings.CosmosThroughPut,
                    cancellationToken: cancellationToken);
                if (cosmosclient.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new AuthenticationException("Problem connecting to Cosmos");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n {ex.StackTrace} \n {ex.StackTrace} \n {ex.InnerException} \n");
                await Task.Delay(10000);
                _logger.LogInformation("Retrying connect to cosmos");
                await DumpAzureContainerRegistryAsync(cancellationToken);
            }
            _logger.LogInformation($"Finished {nameof(CreateCosmosDbIfNotExistAsync)} DbName: {GlobalSettings.CosmosDbName}");
        }

        private async Task CreateCosmosContainerIfNotExistAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started operation {nameof(CreateCosmosContainerIfNotExistAsync)}");
            try
            {
                var cosmoscontainer = await _cosmosclient.GetCosmosClientAsync().
                    Result.GetDatabase(GlobalSettings.CosmosDbName).
                    CreateContainerIfNotExistsAsync(
                        new ContainerProperties(
                            GlobalSettings.CosmosContainerName,
                            GlobalSettings.CosmosPartitionKey),
                        cancellationToken: cancellationToken);
                if (cosmoscontainer.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ArgumentException("Could not create Container");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} \n {ex.InnerException} \n {ex.StackTrace} \n");
            }
            _logger.LogInformation($"Finished {nameof(CreateCosmosContainerIfNotExistAsync)} DbName: {GlobalSettings.CosmosDbName} Container Name: {GlobalSettings.CosmosContainerName}");
    }

        private async Task DumpToCosmosAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started operation {nameof(DumpToCosmosAsync)}");
            try
            {
                var _rclient = await _regclient.GetContainerRegistryClientAsync(cancellationToken);
                var acrrepolist = _rclient.GetRepositoryNamesAsync(cancellationToken);

                await foreach (var repo in acrrepolist)
                {
                    await foreach (var repoproperties in _rclient.GetRepository(repo).GetManifestPropertiesCollectionAsync())
                    {
                        foreach (var tag in repoproperties.Tags)
                        {
                            _logger.LogInformation(
                                $"Repo Name: {repoproperties.RepositoryName} \n " +
                                $"Tag: {tag} \n " +
                                $"Last Updated: {repoproperties.LastUpdatedOn} \n ");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n {ex.InnerException} \n {ex.StackTrace}\n");
            }
        }
    }