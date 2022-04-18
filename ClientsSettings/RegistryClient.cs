using Azure.Containers.ContainerRegistry;
using Azure.Identity;
using RegCleanerScheduler;

namespace RegCleanerScheduler ;

    public class RegistryClient : IRegistryClient
    {
        private readonly ILogger<RegistryClient> _logger;

        public RegistryClient(ILogger<RegistryClient> logger) => _logger = logger;

        public async Task<ContainerRegistryClient> GetContainerRegistryClientAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(
                () =>
                    new ContainerRegistryClient(
                        GlobalSettings.RegEndpoint,
                        new DefaultAzureCredential(),
                        new ContainerRegistryClientOptions
                        {
                            Audience = GetAudience(GlobalSettings.Audience)
                        }),
                cancellationToken);
        }

        private ContainerRegistryAudience? GetAudience(string audience)
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