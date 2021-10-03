using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using ProxmoxJumpProxy.Daemon;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        services.Configure<DaemonOptions>(context.Configuration);
        services.AddHostedService<HostListener>();
        services.AddSingleton(CreateHubConnection);
    })
    .Build();

await host.RunAsync();

static HubConnection CreateHubConnection(IServiceProvider services)
{
    var options = services.GetRequiredService<IOptions<DaemonOptions>>();
    return new HubConnectionBuilder()
        .WithUrl(options.Value.SignalRUrl, options => {

        })
        .ConfigureLogging(builder => {
            builder.AddConsole();
            builder.AddDebug();
        })
        .Build();
}
