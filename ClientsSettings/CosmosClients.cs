using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Azure.Core;

namespace RegCleanerScheduler ;

    public class CosmosClients : ICosmosClient
    {
        private readonly ILogger <CosmosClients> _logger;
        private readonly HttpClient _httpclient;

        public CosmosClients(ILogger<CosmosClients>logger)
        {
            _logger = logger;
            _httpclient = new HttpClient();
        }

        public async Task<CosmosClient> GetCosmosClientAsyncForTestingOnly(
            string azureManagmentSuffix,
            string subscriptionid,
            string resourcegroup,
            string cosmosdbaccountName,
            string cosmosSuffix,
            CancellationToken cancellationToken)
        {
            var creds = new DefaultAzureCredential();
            var accessToken = await creds.GetTokenAsync(
                new TokenRequestContext(
                    new[]
                    {
                        $"https://management.{azureManagmentSuffix}/"
                    }),
                cancellationToken);
            var endpoint = $"https://management.{azureManagmentSuffix}/subscriptions/{subscriptionid}/resourceGroups/{resourcegroup}/providers/Microsoft.DocumentDB/databaseAccounts/{cosmosdbaccountName}/listKeys?api-version=2019-12-12";
            _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            var result = await _httpclient.PostAsync(endpoint, new StringContent(""), cancellationToken);
            var keys = await result.Content.ReadFromJsonAsync<CosmosKeys>(cancellationToken: cancellationToken);
            try
            {
                return await Task.Run(
                    () =>
                        new CosmosClient(
                            $"https://{cosmosdbaccountName}.{cosmosSuffix}:443/",
                            keys.primaryMasterKey,
                            new CosmosClientOptions
                            {
                                ConnectionMode = ConnectionMode.Gateway,
                                AllowBulkExecution = false
                            }),
                    cancellationToken);
            }
            catch (CosmosException cx)
            {
                _logger.LogCritical($"{cx.Diagnostics}\n {cx.Message} {cx.StackTrace} {cx.StackTrace} {cx.StatusCode} {cx.GetBaseException()}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }
            return null;
        }

        public async Task<CosmosClient> GetCosmosClientWithKeysAsync(CancellationToken cancellationToken)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync($"https://management.{GlobalSettings.AzureManagmentSuffix}/", cancellationToken: cancellationToken);
                var endpoint = $"https://management.{GlobalSettings.AzureManagmentSuffix}/subscriptions/{GlobalSettings.SubscriptionId}/resourceGroups/{GlobalSettings.ResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{GlobalSettings.CosmosDbAccountName}/listKeys?api-version=2019-12-12";
                _httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var result = await _httpclient.PostAsync(endpoint, new StringContent(""), cancellationToken);
                var keys = await result.Content.ReadFromJsonAsync<CosmosKeys>(cancellationToken: cancellationToken);

                return await Task.Run(
                    () =>
                        new CosmosClient(
                            $"https://{GlobalSettings.CosmosDbAccountName}.{GlobalSettings.CosmosSuffix}:443/",
                            keys.primaryMasterKey,
                            new CosmosClientOptions
                            {
                                ConnectionMode = ConnectionMode.Gateway,
                                AllowBulkExecution = false
                            }),
                    cancellationToken);
            }
            catch (CosmosException cx)
            {
                _logger.LogCritical($"{cx.Diagnostics}\n {cx.Message} {cx.StackTrace} {cx.StackTrace} {cx.StatusCode} {cx.GetBaseException()}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"{ex.Message} {ex.InnerException} {ex.StackTrace} {ex.Source}");
            }
            return null;
        }
    }