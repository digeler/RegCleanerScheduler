using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;

namespace RegCleanerScheduler ;

    public  class GlobalSettings
    {
        public  static string CosmosDbAccountName { get; } = Environment.GetEnvironmentVariable("CosmosDbAccountName") ?? throw new Exception("CosmosDbAccountName Env Missing");
        public static string CosmosDbName { get; } = Environment.GetEnvironmentVariable("CosmosDbName") ?? throw new Exception("CosmosDbName Env Missing");
        public static Uri RegEndpoint { get; } = new(Environment.GetEnvironmentVariable("RegEndpoint") ?? throw new Exception("RegEndpoint Env Missing"));
        public static string AzureManagmentSuffix { get; } = Environment.GetEnvironmentVariable("AzureManagmentSuffix") ?? "azure.com";
        public static string Audience { get; } = Environment.GetEnvironmentVariable("Audience") ?? "Public";
        public static string SubscriptionId { get; } = Environment.GetEnvironmentVariable("SubscriptionId") ?? throw new Exception("Subid Env Missing");
        public static string ResourceGroup { get; } = Environment.GetEnvironmentVariable("ResourceGroup") ?? throw new Exception("ResourceGroup Env Missing");
        public static string CosmosContainerName { get; } = Environment.GetEnvironmentVariable("CosmosContainerName") ?? "AcrContainer";
        public static int CosmosThroughPut = Convert.ToInt32(Environment.GetEnvironmentVariable("CosmosThroughPut") ?? 1000.ToString());
        public static string CosmosSuffix { get; } = new(Environment.GetEnvironmentVariable("CosmosSuffix") ?? "documents.azure.com");
        public static string IsDryRun { get; } = Environment.GetEnvironmentVariable("IsDryRun") ?? "True";
        public static string MonthsBack { get; } = Environment.GetEnvironmentVariable("MonthsBack") ?? "36";
        public static DateTimeOffset TimeAdjust { get; set; } = DateTime.Now.AddMonths(Convert.ToInt32(MonthsBack));
        public static string CosmosPartitionKey { get; } = "/LastUpdated";

        static GlobalSettings()
        {
            
        }
    }