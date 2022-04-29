using Azure.Containers.ContainerRegistry;
using Azure.Identity;

namespace RegCleanerScheduler ;

    public sealed class RegistryClient : IRegistryClient
    {
        private readonly ILogger<RegistryClient> _logger;

        public RegistryClient(ILogger <RegistryClient> logger) => _logger = logger;

        public async Task<ContainerRegistryClient> GetContainerRegistryClientAsync(
            CancellationToken cancellationToken,
            Uri regtestinguri = null,
            string testAudience = null)
        {
            try
            {
                return await Task.Run(
                    () =>
                        new ContainerRegistryClient(
                            GlobalSettings.RegEndpoint,
                            new DefaultAzureCredential(),
                            new ContainerRegistryClientOptions
                            {
                                Audience = GetAudience(GlobalSettings.Audience ?? testAudience)
                            }),
                    cancellationToken);
            }

            catch (Exception cx)
            {
                _logger.LogCritical($" {cx.Message} {cx.StackTrace} {cx.StackTrace} {cx.GetBaseException()}");
                return await Task.Run(
                    () =>
                        new ContainerRegistryClient(
                            regtestinguri,
                            new DefaultAzureCredential(),
                            new ContainerRegistryClientOptions
                            {
                                Audience = GetAudience(testAudience)
                            }),
                    cancellationToken);
            }
        }

        private static ContainerRegistryAudience? GetAudience(string audience)
        {
            return audience switch
            {
                "Public" => ContainerRegistryAudience.AzureResourceManagerPublicCloud,
                "Goverment" =>
                    ContainerRegistryAudience.AzureResourceManagerGovernment,
                "China" =>
                    ContainerRegistryAudience.AzureResourceManagerChina,
                _ => null
                };
        }
    }