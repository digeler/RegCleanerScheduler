namespace RegCleanerScheduler ;

    public sealed class ContainerRegistryModel
    {
        public string id { get; set; } = string.Empty;
        public string LastUpdated { get; set; }
        public string RepositoryName { get; set; } = String.Empty;
        public string Tag { get; set; } = String.Empty;
}