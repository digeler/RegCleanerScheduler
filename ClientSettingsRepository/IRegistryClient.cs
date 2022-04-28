using Azure.Containers.ContainerRegistry;

namespace RegCleanerScheduler ;

    public interface IRegistryClient
    {
        public Task<ContainerRegistryClient> GetContainerRegistryClientAsync(
            CancellationToken cancellationToken,
            Uri regtestinguri = null,
            string testAudience = null);

    }