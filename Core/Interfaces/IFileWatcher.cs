namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Monitors a directory for file system events and triggers callbacks.
/// </summary>
public interface IFileWatcher
{
    /// <summary>
    /// Starts monitoring the specified directory for file changes.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to monitor.</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories in monitoring.</param>
    /// <param name="cancellationToken">Token to cancel the monitoring operation.</param>
    Task StartAsync(string directoryPath, bool includeSubdirectories, CancellationToken cancellationToken);

    /// <summary>
    /// Stops monitoring the directory.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Event raised when a file is created.
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileCreated;

    /// <summary>
    /// Event raised when a file is renamed.
    /// </summary>
    event EventHandler<RenamedEventArgs>? FileRenamed;
}
