using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using Azure.Containers.ContainerRegistry;
using Coravel.Invocable;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.VisualBasic;
using RegCleanerScheduler;

namespace RegCleanerScheduler.Operations ;

    public class DumpAzureContainerRegistryToCosmosAsync : IInvocable
    {
        private readonly ILogger <DumpAzureContainerRegistryToCosmosAsync> _logger;
        private readonly IRegistryClient _regclient;
        private readonly ICosmosClient _cosmosclient;
        private readonly ContainerRegistryModel _registrymodel;
        private int _items = 0;
        private int _interval = 0;

        private DatabaseResponse _result;

        private ContainerResponse _comoscontainer;

        public DumpAzureContainerRegistryToCosmosAsync(
            ILogger <DumpAzureContainerRegistryToCosmosAsync> logger,
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
            _logger.LogInformation($"invoked with {nameof(DumpAzureContainerRegistryAsync)}");
            _logger.LogInformation("Starting Wait 30 sec for debugging");
            await Task.Delay(30000);

            try
            {
                await DumpAzureContainerRegistryAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"{ex.Message} \n {ex.StackTrace} \n {ex.InnerException} \n");
            }
        }

        public async Task DumpAzureContainerRegistryAsync(CancellationToken cancellationToken)
        {
            await CreateCosmosDbIfNotExistAsync(cancellationToken);
            await CreateCosmosContainerIfNotExistAsync(cancellationToken);
            await DumpToCosmosAsync(cancellationToken);
        }

        public async Task CreateCosmosDbIfNotExistAsync(
            CancellationToken cancellationToken,
            CosmosClient cosmostestClient = null,
            string cosmostestsuffix = null,
            string cosmosdbtestaccount = null,
            string cosmosdbtstname = null,
            bool istest = false
            )
        {
            if (!istest)
            {
                _ = new GlobalSettings();
                try
                {
                    _logger.LogInformation($"Started operation {nameof(CreateCosmosDbIfNotExistAsync)}");
                    _logger.LogInformation($"Using endpoint https://{GlobalSettings.CosmosDbAccountName}.{GlobalSettings.CosmosSuffix}:443/");
                    var cclient = await _cosmosclient.GetCosmosClientWithKeysAsync(cancellationToken);
                    var db = cclient.GetDatabase(GlobalSettings.CosmosDbName);
                    if (db.Id != null)
                    {
                        _logger.LogInformation($"DbName already exists{db.Id}");
                    }
                    else
                    {
                        var result = await cclient.CreateDatabaseIfNotExistsAsync(
                            GlobalSettings.CosmosDbName,
                            GlobalSettings.CosmosThroughPut,
                            cancellationToken: cancellationToken);
                        _result = result;

                        if (result.StatusCode == HttpStatusCode.Created)
                        {
                            _logger.LogInformation($"{HttpStatusCode.Created}");
                        }
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            _logger.LogInformation($"{HttpStatusCode.OK}");
                        }

                        else
                        {
                            throw new AuthenticationException("Problem connecting to Cosmos");
                        }
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
            if (istest)
            {
                var result = await cosmostestClient.CreateDatabaseIfNotExistsAsync(
                    cosmosdbtstname,
                    1000,
                    cancellationToken: cancellationToken);
                _result = result;
            }
        }

        public async Task CreateCosmosContainerIfNotExistAsync(
            CancellationToken cancellationToken,
            CosmosClient testClient = null,
            string cosmosdbtstname = null,
            string cosmostestcontainername = null,
            string cosmosdbtestpartitionkey = null,
            string cosmosdbaccountname = null,
            bool isTest = false)
        {
            if (!isTest)
            {
                _ = new GlobalSettings();
                try
                {
                    _logger.LogInformation($"Started operation {nameof(CreateCosmosContainerIfNotExistAsync)}");
                    var cosmos = await _cosmosclient.GetCosmosClientWithKeysAsync(cancellationToken);
                    var cosmoscontainer = await cosmos.GetDatabase(GlobalSettings.CosmosDbName).
                        CreateContainerIfNotExistsAsync(
                            new ContainerProperties(
                                GlobalSettings.CosmosContainerName,
                                GlobalSettings.CosmosPartitionKey),
                            cancellationToken: cancellationToken);

                    if (cosmoscontainer.StatusCode == HttpStatusCode.Created)
                    {
                        _logger.LogInformation($"{HttpStatusCode.Created}");
                    }
                    if (cosmoscontainer.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"{HttpStatusCode.OK}");
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

                _logger.LogInformation($"Finished {nameof(CreateCosmosContainerIfNotExistAsync)} DbName: {GlobalSettings.CosmosDbAccountName ?? cosmosdbaccountname} Container Name: {GlobalSettings.CosmosContainerName ?? cosmostestcontainername}");
            }
            if (isTest)
            {
                try
                {
                    await testClient.GetDatabase(cosmosdbtstname).
                        CreateContainerIfNotExistsAsync(
                            new ContainerProperties(
                                cosmostestcontainername,
                                cosmosdbtestpartitionkey),
                            cancellationToken: cancellationToken);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task DumpToCosmosAsync(CancellationToken cancellationToken, ContainerRegistryClient containertestclient = null, bool isTesting = false)
        {
            _logger.LogInformation($"Started operation {nameof(DumpToCosmosAsync)}");
            var tasks = new List<Task>();
            var _rclient = containertestclient ?? await _regclient.GetContainerRegistryClientAsync(cancellationToken);
            var acrrepolist = _rclient.GetRepositoryNamesAsync(cancellationToken);
            if (!isTesting)
            {
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
                            _registrymodel.LastUpdated = repoproperties.LastUpdatedOn.ToString("s");
                            _registrymodel.Tag = tag;
                            _registrymodel.RepositoryName = repoproperties.RepositoryName;
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
                                                _logger.LogInformation(t.Status.ToString());
                                            }
                                            else
                                            {
                                                _logger.LogError(t.Exception?.GetBaseException().ToString());
                                            }
                                        },
                                        cancellationToken));

                                _logger.LogInformation($"Dump completed with status code {task.Status}");
                            }
                            catch (CosmosException cx)
                            {
                                _logger.LogCritical($"{cx.Diagnostics}\n {cx.Message} {cx.StackTrace} {cx.StackTrace} {cx.StatusCode} {cx.GetBaseException()}");
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("Entity with the specified id already exists in the system"))
                                {
                                    _logger.LogInformation($"Conflict 409 for {_registrymodel.id}");
                                }
                                if (ex.Message.Contains("A task was canceled"))
                                {
                                    _logger.LogInformation("A task was cancelled");
                                }
                                else
                                {
                                    _logger.LogCritical($"{ex.Message} \n {ex.InnerException} \n {ex.StackTrace} {ex.GetBaseException()}");
                                }
                            }
                        }
                    }
                }
                await Task.WhenAll(tasks);
                _logger.LogInformation($"Total items that was dumped {_items}");
            }
            if (isTesting)
            {
                await foreach (var repo in acrrepolist)
                {
                    if (repo.Length > 0)
                    {
                        return;
                    }
                }
            }
        }
    }