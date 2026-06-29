using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Service for monitoring file system changes.
/// </summary>
public class FileWatcher : IFileWatcher
{
    private readonly ILogger<FileWatcher> _logger;
    private FileSystemWatcher? _watcher;

    public event EventHandler<FileSystemEventArgs>? FileCreated;
    public event EventHandler<RenamedEventArgs>? FileRenamed;

    public FileWatcher(ILogger<FileWatcher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts monitoring a directory for file changes.
    /// </summary>
    public bool StartMonitoring(string directory, string filter, bool includeSubdirectories)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogError("Directory does not exist. Directory: {Directory}", directory);
                return false;
            }

            _watcher = new FileSystemWatcher(directory)
            {
                Filter = filter,
                IncludeSubdirectories = includeSubdirectories,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _watcher.Created += OnFileCreated;
            _watcher.Renamed += OnFileRenamed;

            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation(
                "File watcher started. Directory: {Directory}, Filter: {Filter}, IncludeSubdirectories: {IncludeSubdirectories}",
                directory,
                filter,
                includeSubdirectories);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Error starting file watcher. Directory: {Directory}, Exception: {Exception}",
                directory,
                ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Stops monitoring for file changes.
    /// </summary>
    public void StopMonitoring()
    {
        try
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileCreated;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Dispose();
                _watcher = null;

                _logger.LogInformation("File watcher stopped.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error stopping file watcher. Exception: {Exception}", ex.Message);
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("OnFileCreated event raised. Path: {Path}", e.FullPath);
        FileCreated?.Invoke(this, e);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogDebug("OnFileRenamed event raised. OldPath: {OldPath}, NewPath: {NewPath}", e.OldFullPath, e.FullPath);
        FileRenamed?.Invoke(this, e);
    }
}
