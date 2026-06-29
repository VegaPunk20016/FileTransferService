using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Monitors a directory for file system changes.
/// Detects file creation and rename events.
/// </summary>
public sealed class FileWatcher : IFileWatcher
{
    private readonly ILogger<FileWatcher> _logger;
    private FileSystemWatcher? _fileSystemWatcher;
    private readonly object _lockObject = new object();

    public event EventHandler<FileSystemEventArgs>? FileCreated;
    public event EventHandler<RenamedEventArgs>? FileRenamed;

    public FileWatcher(ILogger<FileWatcher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts monitoring the specified directory for file changes.
    /// </summary>
    /// <param name="directoryPath">The directory path to monitor.</param>
    /// <param name="filter">Optional file filter pattern (e.g., "*.txt").</param>
    /// <param name="includeSubdirectories">Whether to monitor subdirectories.</param>
    /// <returns>True if monitoring started successfully; otherwise false.</returns>
    public bool StartMonitoring(string directoryPath, string filter = "*.*", bool includeSubdirectories = false)
    {
        if (!ValidateDirectoryPath(directoryPath))
        {
            return false;
        }

        lock (_lockObject)
        {
            try
            {
                // Stop existing watcher if any
                if (_fileSystemWatcher != null)
                {
                    _logger.LogInformation("Stopping existing file watcher");
                    StopMonitoring();
                }

                // Create new watcher
                _fileSystemWatcher = new FileSystemWatcher(directoryPath)
                {
                    Filter = filter,
                    IncludeSubdirectories = includeSubdirectories,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                // Subscribe to events
                _fileSystemWatcher.Created += OnFileCreated;
                _fileSystemWatcher.Renamed += OnFileRenamed;
                _fileSystemWatcher.Error += OnWatcherError;

                // Enable monitoring
                _fileSystemWatcher.EnableRaisingEvents = true;

                _logger.LogInformation(
                    "File watcher started. Directory: {Directory}, Filter: {Filter}, IncludeSubdirectories: {IncludeSubdirectories}",
                    directoryPath,
                    filter,
                    includeSubdirectories);

                return true;
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(
                    "Invalid argument for file watcher. Directory: {Directory}, Exception: {Exception}",
                    directoryPath,
                    argEx.Message);
                return false;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                _logger.LogError(
                    "Access denied when starting file watcher. Directory: {Directory}, Exception: {Exception}",
                    directoryPath,
                    uaEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Unexpected error when starting file watcher. Directory: {Directory}, Exception: {Exception}",
                    directoryPath,
                    ex.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// Stops monitoring the directory for file changes.
    /// </summary>
    public void StopMonitoring()
    {
        lock (_lockObject)
        {
            try
            {
                if (_fileSystemWatcher != null)
                {
                    _fileSystemWatcher.EnableRaisingEvents = false;
                    _fileSystemWatcher.Created -= OnFileCreated;
                    _fileSystemWatcher.Renamed -= OnFileRenamed;
                    _fileSystemWatcher.Error -= OnWatcherError;
                    _fileSystemWatcher.Dispose();
                    _fileSystemWatcher = null;

                    _logger.LogInformation("File watcher stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error stopping file watcher. Exception: {Exception}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Handles file created events.
    /// </summary>
    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            _logger.LogInformation(
                "File created event detected. FilePath: {FilePath}, ChangeType: {ChangeType}",
                e.FullPath,
                e.ChangeType);

            // Raise the FileCreated event to subscribers
            FileCreated?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Error handling file created event. FilePath: {FilePath}, Exception: {Exception}",
                e.FullPath,
                ex.Message);
        }
    }

    /// <summary>
    /// Handles file renamed events.
    /// </summary>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            _logger.LogInformation(
                "File renamed event detected. OldPath: {OldPath}, NewPath: {NewPath}, ChangeType: {ChangeType}",
                e.OldFullPath,
                e.FullPath,
                e.ChangeType);

            // Raise the FileRenamed event to subscribers
            FileRenamed?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Error handling file renamed event. OldPath: {OldPath}, NewPath: {NewPath}, Exception: {Exception}",
                e.OldFullPath,
                e.FullPath,
                ex.Message);
        }
    }

    /// <summary>
    /// Handles file watcher errors.
    /// </summary>
    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var exception = e.GetException();
        _logger.LogError(
            "File watcher error occurred. Exception: {Exception}",
            exception?.Message ?? "Unknown error");
    }

    /// <summary>
    /// Validates that the directory path exists and is accessible.
    /// </summary>
    private bool ValidateDirectoryPath(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            _logger.LogWarning("Directory path is null or whitespace");
            return false;
        }

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("Directory does not exist. Directory: {Directory}", directoryPath);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Releases resources used by the file watcher.
    /// </summary>
    public void Dispose()
    {
        StopMonitoring();
    }
}
