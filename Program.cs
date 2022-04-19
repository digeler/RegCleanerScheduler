
using Coravel;
using RegCleanerScheduler;
using RegCleanerScheduler.Operations;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddScheduler();
        services.AddLogging(
            options =>
            {
                options.AddSimpleConsole(
                    c =>
                    {
                        c.TimestampFormat = "[dd-MM-yyyy HH:mm:ss]";
                        c.UseUtcTimestamp = true;
                    });
            });
        services.AddSingleton<IRegistryClient, RegistryClient>();
        services.AddSingleton<ICosmosClient,CosmosClients>();
        services.AddTransient<DumpAzureContainerRegistryToCosmosAsync>();
    })
    .Build();
    host.Services.UseScheduler(
        scheduler =>
            scheduler.OnWorker("DumpAzureContainerRegistryToCosmosAsync")
                .Schedule<DumpAzureContainerRegistryToCosmosAsync>().
                EveryTenSeconds().PreventOverlapping("DumpAzureContainerRegistry").RunOnceAtStart());
await host.RunAsync();
