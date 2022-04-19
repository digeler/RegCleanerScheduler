using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using Coravel.Invocable;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualBasic;
using RegCleanerScheduler;

namespace RegCleanerScheduler.Operations ;

    public sealed class DumpAzureContainerRegistryToCosmosAsync : IInvocable
    {
        private readonly ILogger<DumpAzureContainerRegistryToCosmosAsync> _logger;
        private readonly IRegistryClient _regclient;
        private readonly ICosmosClient _cosmosclient;
        private readonly ContainerRegistryModel _registrymodel;
        private int _items = 0;
        private int _interval = 0;

        private DatabaseResponse _result;

        private ContainerResponse _comoscontainer;

        public DumpAzureContainerRegistryToCosmosAsync(
            ILogger<DumpAzureContainerRegistryToCosmosAsync> logger,
            IRegistryClient regClient,
            ICosmosClient cosmosClient)
        {
            _logger = logger;
            _regclient = regClient;
            _cosmosclient = cosmosClient;
            _registrymodel = new ContainerRegistryModel();
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"Coravel was invoked with {nameof(DumpAzureContainerRegistryAsync)}");
            _logger.LogInformation("Starting Wait 30 sec for debugging");
            await Task.Delay(30000);

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
            _logger.LogInformation($"Using endpoint https://{GlobalSettings.CosmosDbAccountName}.{GlobalSettings.CosmosSuffix}:443/");

            try
            {
                var result = await _cosmosclient.GetCosmosClientWithKeysAsync(cancellationToken).Result.CreateDatabaseIfNotExistsAsync(
                    GlobalSettings.CosmosDbName,
                    GlobalSettings.CosmosThroughPut,
                    cancellationToken: cancellationToken);
                _result = result;

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    throw new AuthenticationException("Problem connecting to Cosmos");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Name or service not known"))
                {
                    _logger.LogCritical("Please create cosmos account");
                }
                _logger.LogError($"{ex.Message}\n {ex.StackTrace} \n {ex.StackTrace} \n {ex.InnerException} \n");
                _logger.LogInformation($"Diagnostics: {_result.Diagnostics}");
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
                var cosmoscontainer = await _cosmosclient.GetCosmosClientWithKeysAsync(cancellationToken).
                    Result.GetDatabase(GlobalSettings.CosmosDbName).
                    CreateContainerIfNotExistsAsync(
                        new ContainerProperties(
                            GlobalSettings.CosmosContainerName,
                            GlobalSettings.CosmosPartitionKey),
                        cancellationToken: cancellationToken);
                if (cosmoscontainer.StatusCode != HttpStatusCode.OK)
                {
                    throw new ArgumentException("Could not create Container");
                }
                _comoscontainer = cosmoscontainer;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Resource Not Found"))
                {
                    _logger.LogError($"Resource not found will try 5 times in 5 sec intervals");
                    while (_interval <= 5)
                    {
                        await CreateCosmosContainerIfNotExistAsync(cancellationToken);
                        await Task.Delay(5000, cancellationToken);
                        _interval++;
                        _logger.LogInformation($"Itration {_interval}");
                        _logger.LogError($"{ex.Message}\n {ex.StackTrace} \n {ex.StackTrace} \n {ex.InnerException} \n");
                    }
                }
                else
                {
                    _logger.LogError($"{ex.Message} \n {ex.InnerException} \n {ex.StackTrace} \n");
                }
            }

            _logger.LogInformation($"Finished {nameof(CreateCosmosContainerIfNotExistAsync)} DbName: {GlobalSettings.CosmosDbAccountName} Container Name: {GlobalSettings.CosmosContainerName}");
        }

        private async Task DumpToCosmosAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started operation {nameof(DumpToCosmosAsync)}");
            var tasks = new List<Task>();
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
                        _registrymodel.id = $"{repoproperties.RepositoryName}:{tag}";
                        _registrymodel.LastUpdated = repoproperties.LastUpdatedOn;
                        _items++;

                        var task = _comoscontainer.Container.CreateItemAsync(_registrymodel, cancellationToken: cancellationToken);
                        try
                        {
                            tasks.Add(
                                task.ContinueWith(
                                    t =>
                                    {
                                        if (t.Status == TaskStatus.RanToCompletion)
                                        {
                                            _logger.LogInformation(t.Result.StatusCode.ToString());
                                        }
                                        else
                                        {
                                            _logger.LogError(t.Exception.Message);
                                        }
                                    },
                                    cancellationToken));

                            _logger.LogInformation($"Dump completed with status code {task.Result.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("A task was canceled"))
                            {
                                _logger.LogInformation("A task was cancelled");
                            }
                            else
                            {
                                if (ex.Message.Contains("Conflict (409)"))
                                {
                                    _logger.LogInformation($"Conflict 409 for {_registrymodel.id}");
                                }
                                else
                                {
                                    _logger.LogCritical($"{ex.Message} \n {ex.InnerException} \n {ex.StackTrace}");
                                }
                            }
                        }
                    }
                }
            }
            await Task.WhenAll(tasks);
            _logger.LogInformation($"Total items that was dumped {_items}");
        }
    }