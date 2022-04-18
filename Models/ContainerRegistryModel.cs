namespace RegCleanerScheduler ;

    public class ContainerRegistryModel
    {
        public string Id { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public DateTimeOffset LastUpdated { get; set; }
    }