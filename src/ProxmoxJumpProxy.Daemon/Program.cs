using ProxmoxJumpProxy.Daemon;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<HostListener>();
    })
    .Build();

await host.RunAsync();
