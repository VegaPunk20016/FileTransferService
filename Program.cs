using FileTransferService.Core.Interfaces;
using FileTransferService.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Add logging configuration
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddConsole();
            configure.AddDebug();
            configure.SetMinimumLevel(LogLevel.Information);
        });

        // Register core services
        services.AddScoped<IFileScanner, FileScanner>();
        services.AddScoped<IFileValidator, FileValidator>();
        services.AddScoped<IFileTransfer, FileTransfer>();
        services.AddScoped<IDestinationResolver, DestinationResolver>();
        services.AddScoped<IFileWatcher, FileWatcher>();
        services.AddScoped<ITransferOrchestrator, TransferOrchestrator>();

        // Register the worker service
        services.AddHostedService<FileTransferWorker>();
    });

var host = builder.Build();
await host.RunAsync();
