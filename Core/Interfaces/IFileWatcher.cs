namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Interface for monitoring file system changes.
/// </summary>
public interface IFileWatcher
{
    /// <summary>
    /// Event raised when a file is created.
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileCreated;

    /// <summary>
    /// Event raised when a file is renamed.
    /// </summary>
    event EventHandler<RenamedEventArgs>? FileRenamed;

    /// <summary>
    /// Starts monitoring a directory for file changes.
    /// </summary>
    /// <param name="directory">The directory to monitor.</param>
    /// <param name="filter">File filter pattern (e.g., "*.*").</param>
    /// <param name="includeSubdirectories">Whether to monitor subdirectories.</param>
    /// <returns>True if monitoring started successfully, false otherwise.</returns>
    bool StartMonitoring(string directory, string filter, bool includeSubdirectories);

    /// <summary>
    /// Stops monitoring for file changes.
    /// </summary>
    void StopMonitoring();
}
