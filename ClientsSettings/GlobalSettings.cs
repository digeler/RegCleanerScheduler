using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;

namespace RegCleanerScheduler ;

    public static class GlobalSettings
    {
        public static string CosmosDbName { get; } = Environment.GetEnvironmentVariable("CosmosDbName") ?? throw new Exception("CosmosDb Env Missing");
        public static Uri RegEndpoint { get; } = new(Environment.GetEnvironmentVariable("RegEndpoint") ?? throw new Exception("RegEndpoint Env Missing"));
        public static string Audience { get; } = Environment.GetEnvironmentVariable("Audience") ?? "Public";

        public static string CosmosContainerName { get; } = Environment.GetEnvironmentVariable("CosmosContainerName") ?? "AcrContainer";
        public static int CosmosThroughPut = Convert.ToInt32(Environment.GetEnvironmentVariable("CosmosThroughPut") ?? 400.ToString());
        public static string CosmosSuffix { get; } = new(Environment.GetEnvironmentVariable("CosmosSuffix") ?? "documents.azure.com");
        public static string CosmosPartitionKey { get; } = "/LastUpdatedOn";
    }