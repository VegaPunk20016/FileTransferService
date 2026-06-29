using FileTransferService.Configuration;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext())
    .ConfigureServices((context, services) =>
    {
        services.Configure<TransferSettings>(context.Configuration.GetSection("TransferSettings"));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
