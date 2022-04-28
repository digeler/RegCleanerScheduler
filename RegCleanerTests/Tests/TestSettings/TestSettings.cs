using System;
using Azure.Containers.ContainerRegistry;

namespace Tests
{
    public class TestSettings
    {
        static TestSettings()
        {
            RegEndpoint = new Uri("https://polyengcr.azurecr.io");
            CosmosDbAccountName = "container-da";
            CosmosDbName = "acrdb";
            CosmosContainerName = "testcontainer";
            SubscriptionId = "A9F4E502-9188-4E9C-857F-532DD66F5D0C";
            ResourceGroup = "PolyTstUseGc";
        }
        public static string CosmosDbAccountName { get; set; }
        public static string CosmosDbName { get; set; }
        public static Uri RegEndpoint { get; set; }
        public static string AzureManagmentSuffix { get; set; } = "azure.com";
        public static string Audience { get; set; } = ContainerRegistryAudience.AzureResourceManagerPublicCloud.ToString();
        public static string SubscriptionId { get; set; }
        public static string ResourceGroup { get; set; }
        public static string CosmosContainerName { get; set; } = Environment.GetEnvironmentVariable("CosmosContainerName") ?? "AcrContainer";
        public static int CosmosThroughPut = 1000;
        public static string CosmosSuffix { get; set; } = "documents.azure.com";
        public static string IsDryRun { get; set; } = "True";
        public static string MonthsBack { get; set; } = "-3";
        public static DateTimeOffset TimeAdjust { get; set; } = DateTime.Now.AddMonths(Convert.ToInt32(MonthsBack));
        public static string CosmosPartitionKey { get; set; } = "/LastUpdated";
    }
}