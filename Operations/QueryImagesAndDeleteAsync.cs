using System.Diagnostics;
using Azure.Containers.ContainerRegistry;
using Coravel.Invocable;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace RegCleanerScheduler ;

    public sealed class QueryImagesAndDeleteAsync : IInvocable
    {
        private readonly ILogger<QueryImagesAndDeleteAsync> _logger;
        private readonly ICosmosClient _cosmosclient;
        private readonly IRegistryClient _registryclient;
        private int _item = 0;
        private Task<ContainerRegistryClient> _regclient;

        public QueryImagesAndDeleteAsync(
            ILogger<QueryImagesAndDeleteAsync> logger,
            ICosmosClient cosmosClient,
            IRegistryClient registryClient)
        {
            _logger = logger;
            _cosmosclient = cosmosClient;
            _registryclient = registryClient;
        }

        public async Task QueryAndDeleteAsync(
            CancellationToken cancellationToken,
            CosmosClient cosmosTestClient = null,
            string cosmosdbtstname = null,
            string cosmostestcontainername = null,
            string monthsBack = null,
            bool IsTesting = false
            )
        {
            _regclient = _registryclient.GetContainerRegistryClientAsync(cancellationToken);

            try
            {
                if (!IsTesting)
                {
                    _ = new GlobalSettings();
                    var csclient = await _cosmosclient.GetCosmosClientWithKeysAsync(cancellationToken);
                    var cosmoscontainer = csclient.GetDatabase(GlobalSettings.CosmosDbName).
                        GetContainer(GlobalSettings.CosmosContainerName);

                    _logger.LogInformation($"Started operation {nameof(QueryAndDeleteAsync)} With Date {GlobalSettings.TimeAdjust} flag is {Convert.ToBoolean(GlobalSettings.IsDryRun)}");
                    _logger.LogInformation($"SELECT * FROM c where c.LastUpdated < DateTimeAdd('month', {GlobalSettings.MonthsBack}, GetCurrentDateTime()) order by c.LastUpdated desc");
                    using (var results = cosmoscontainer.GetItemQueryIterator<ContainerRegistryModel>(
                        $"SELECT * FROM c where c.LastUpdated < DateTimeAdd('month', {GlobalSettings.MonthsBack}, GetCurrentDateTime()) order by c.LastUpdated desc",
                        requestOptions: new QueryRequestOptions()
                        {
                            MaxConcurrency = 2,
                            MaxItemCount = 10
                        }))
                    {
                        while (results.HasMoreResults)
                        {
                            var response = await results.ReadNextAsync(cancellationToken);
                            foreach (var iteModel in response)
                            {
                                {
                                    if (Convert.ToBoolean(GlobalSettings.IsDryRun))
                                    {
                                        _logger.LogInformation($"IsDryRun flag set to {GlobalSettings.IsDryRun} will not delete");
                                        _logger.LogInformation(
                                            $"Dry run will delete the following images image id: {iteModel.id}" +
                                            $"Last updated at {iteModel.LastUpdated}");
                                        _logger.LogInformation($"current items will be deleted {_item}");
                                        _item++;
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"IsDryRun flag set to {GlobalSettings.IsDryRun} will delete");
                                        _logger.LogInformation($"Image id  to be deleted {iteModel.id} LastUpdatedOn {iteModel.LastUpdated}");
                                        await cosmoscontainer.
                                            DeleteItemAsync<ContainerRegistryModel>(
                                                iteModel.id,
                                                new PartitionKey(iteModel.LastUpdated),
                                                cancellationToken: cancellationToken);

                                        var regclientt = await _regclient;
                                        var regoperation = regclientt.GetArtifact(iteModel.RepositoryName, iteModel.Tag).DeleteAsync(cancellationToken);
                                        _logger.LogInformation($"Deleted Image {iteModel.id} with {iteModel.LastUpdated} status: {regoperation.Status}");
                                    }
                                }
                            }
                        }
                        _logger.LogInformation($"Total items deleted {_item}");
                    }
                }
                if (IsTesting)
                {
                    var csclient = cosmosTestClient;
                    var cosmoscontainer = csclient.GetDatabase(cosmosdbtstname).
                        GetContainer(cosmostestcontainername);
                    using (var results = cosmoscontainer.GetItemQueryIterator<ContainerRegistryModel>(
                        $"SELECT * FROM c where c.LastUpdated < DateTimeAdd('month', {monthsBack}, GetCurrentDateTime()) order by c.LastUpdated desc",
                        requestOptions: new QueryRequestOptions()
                        {
                            MaxConcurrency = 1,
                            MaxItemCount = 1
                        }))
                    {
                        while (results.HasMoreResults)
                        {
                            var response = await results.ReadNextAsync(CancellationToken.None);
                            foreach (var iteModel in response)
                            {
                                if (iteModel.LastUpdated.Length > 0)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch
                (CosmosException cx)
            {
                _logger.LogCritical($"{cx.Diagnostics}\n {cx.Message} {cx.StackTrace} {cx.StackTrace} {cx.StatusCode} {cx.GetBaseException()}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"{ex.Message} \n {ex.StackTrace} \n {ex.InnerException}");
            }
        }

        public async Task Invoke()
        {
            _logger.LogInformation("Awaiting 30 sec for debugging");
            await Task.Delay(30000);
            _logger.LogInformation($"{nameof(QueryAndDeleteAsync)} invoked");

            try
            {
                await QueryAndDeleteAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"{ex.Message} \n {ex.StackTrace} \n {ex.InnerException}");
            }
        }
    }