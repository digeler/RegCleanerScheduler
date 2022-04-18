using Azure.Containers.ContainerRegistry;

namespace RegCleanerScheduler ;

    public interface IRegistryClient
    {
        public Task<ContainerRegistryClient> GetContainerRegistryClientAsync(CancellationToken cancellationToken);
    }