using Stormancer.Bots.Agent.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options=>
    {
        options.ServiceName = "Stormancer Remote control agent";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
