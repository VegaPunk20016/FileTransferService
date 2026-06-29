using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Background worker service for continuous file transfer operations.
/// Monitors source directory and automatically transfers files.
/// </summary>
public class FileTransferWorker : BackgroundService
{
    private readonly ILogger<FileTransferWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileWatcher _fileWatcher;

    // Configuration
    private readonly string _sourceDirectory;
    private readonly string _destinationDirectory;
    private readonly bool _includeSubdirectories;
    private readonly int _delayMilliseconds;

    public FileTransferWorker(
        ILogger<FileTransferWorker> logger,
        IServiceProvider serviceProvider,
        IFileWatcher fileWatcher)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _fileWatcher = fileWatcher;

        // Load configuration from environment variables
        _sourceDirectory = Environment.GetEnvironmentVariable("SOURCE_DIRECTORY") ?? @"C:\Source";
        _destinationDirectory = Environment.GetEnvironmentVariable("DESTINATION_DIRECTORY") ?? @"C:\Destination";
        _includeSubdirectories = Environment.GetEnvironmentVariable("INCLUDE_SUBDIRECTORIES")?.ToLower() == "true";
        _delayMilliseconds = int.TryParse(Environment.GetEnvironmentVariable("TRANSFER_DELAY_MS"), out var delay) ? delay : 5000;
    }

    /// <summary>
    /// Executes the background worker service.
    /// Monitors source directory and performs transfers on file events.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "File Transfer Worker starting. Source: {Source}, Destination: {Destination}",
            _sourceDirectory,
            _destinationDirectory);

        try
        {
            // Validate directories
            if (!ValidateDirectories())
            {
                _logger.LogError("Directory validation failed. Worker stopping.");
                return;
            }

            // Start file watcher
            var watcherStarted = _fileWatcher.StartMonitoring(
                _sourceDirectory,
                filter: "*.*",
                includeSubdirectories: _includeSubdirectories);

            if (!watcherStarted)
            {
                _logger.LogError("Failed to start file watcher. Worker stopping.");
                return;
            }

            // Subscribe to file watcher events
            _fileWatcher.FileCreated += OnFileCreated;
            _fileWatcher.FileRenamed += OnFileRenamed;

            _logger.LogInformation("File watcher events subscribed. Worker ready.");

            // Keep worker alive until cancellation
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("File Transfer Worker cancellation requested.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error in File Transfer Worker. Exception: {Exception}", ex.Message);
        }
        finally
        {
            StopWorker();
        }
    }

    private void OnFileCreated(object? sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("File created event detected. FilePath: {FilePath}", e.FullPath);

        try
        {
            Task.Delay(_delayMilliseconds).Wait();
            ExecuteTransferForFile(e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error handling file created event. Exception: {Exception}", ex.Message);
        }
    }

    private void OnFileRenamed(object? sender, RenamedEventArgs e)
    {
        _logger.LogInformation("File renamed event detected. OldPath: {OldPath}, NewPath: {NewPath}", e.OldFullPath, e.FullPath);

        try
        {
            Task.Delay(_delayMilliseconds).Wait();
            ExecuteTransferForFile(e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error handling file renamed event. Exception: {Exception}", ex.Message);
        }
    }

    private void ExecuteTransferForFile(string sourceFilePath)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var destinationResolver = scope.ServiceProvider.GetRequiredService<IDestinationResolver>();
            var fileTransfer = scope.ServiceProvider.GetRequiredService<IFileTransfer>();

            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    _logger.LogWarning("File no longer exists. FilePath: {FilePath}", sourceFilePath);
                    return;
                }

                var destinationPath = destinationResolver.ResolveDestinationPath(sourceFilePath, _destinationDirectory);

                _logger.LogInformation(
                    "Transferring file. Source: {Source}, Destination: {Destination}",
                    sourceFilePath,
                    destinationPath);

                var task = fileTransfer.TransferAsync(sourceFilePath, destinationPath, CancellationToken.None);
                var success = task.Result;

                if (success)
                {
                    _logger.LogInformation(
                        "File transferred successfully. Source: {Source}, Destination: {Destination}",
                        sourceFilePath,
                        destinationPath);
                }
                else
                {
                    _logger.LogWarning(
                        "File transfer failed. Source: {Source}, Destination: {Destination}",
                        sourceFilePath,
                        destinationPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error transferring file. Source: {Source}, Exception: {Exception}",
                    sourceFilePath,
                    ex.Message);
            }
        }
    }

    private bool ValidateDirectories()
    {
        if (string.IsNullOrWhiteSpace(_sourceDirectory))
        {
            _logger.LogError("Source directory is not configured.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_destinationDirectory))
        {
            _logger.LogError("Destination directory is not configured.");
            return false;
        }

        if (!Directory.Exists(_sourceDirectory))
        {
            _logger.LogError("Source directory does not exist. Directory: {Directory}", _sourceDirectory);
            return false;
        }

        if (!Directory.Exists(_destinationDirectory))
        {
            _logger.LogInformation(
                "Destination directory does not exist. Creating it. Directory: {Directory}",
                _destinationDirectory);
            try
            {
                Directory.CreateDirectory(_destinationDirectory);
                _logger.LogInformation("Destination directory created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to create destination directory. Directory: {Directory}, Exception: {Exception}",
                    _destinationDirectory,
                    ex.Message);
                return false;
            }
        }

        return true;
    }

    private void StopWorker()
    {
        try
        {
            _logger.LogInformation("Stopping File Transfer Worker.");

            _fileWatcher.FileCreated -= OnFileCreated;
            _fileWatcher.FileRenamed -= OnFileRenamed;

            _fileWatcher.StopMonitoring();

            _logger.LogInformation("File Transfer Worker stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error stopping File Transfer Worker. Exception: {Exception}", ex.Message);
        }
    }
}
