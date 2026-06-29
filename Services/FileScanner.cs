using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Scans a directory recursively and discovers all files matching criteria.
/// </summary>
public sealed class FileScanner : IFileScanner
{
    private readonly ILogger<FileScanner> _logger;

    public FileScanner(ILogger<FileScanner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Scans the specified directory and returns all files found.
    /// </summary>
    /// <param name="directoryPath">The root directory path to scan.</param>
    /// <param name="includeSubdirectories">Whether to include files in subdirectories.</param>
    /// <param name="cancellationToken">Token to cancel the scan operation.</param>
    /// <returns>A collection of file paths found during the scan.</returns>
    public async Task<IEnumerable<string>> ScanAsync(string directoryPath, bool includeSubdirectories, CancellationToken cancellationToken)
    {
        // Validate input
        if (!ValidateDirectoryPath(directoryPath))
        {
            return Enumerable.Empty<string>();
        }

        // Check cancellation before starting
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Directory scan cancelled before start. Directory: {Directory}", directoryPath);
            return Enumerable.Empty<string>();
        }

        var files = new List<string>();

        try
        {
            _logger.LogInformation(
                "Starting directory scan. Directory: {Directory}, IncludeSubdirectories: {IncludeSubdirectories}",
                directoryPath,
                includeSubdirectories);

            var startTime = DateTime.UtcNow;

            // Perform the scan
            await Task.Run(() =>
            {
                ScanDirectoryRecursive(directoryPath, files, includeSubdirectories, cancellationToken);
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Directory scan completed. Directory: {Directory}, FilesFound: {FilesFound}, Duration: {Duration}ms",
                directoryPath,
                files.Count,
                duration.TotalMilliseconds);

            return files;
        }
        catch (OperationCanceledException ocEx)
        {
            _logger.LogWarning(
                "Directory scan cancelled during operation. Directory: {Directory}, FilesFound: {FilesFound}, Exception: {Exception}",
                directoryPath,
                files.Count,
                ocEx.Message);
            return files; // Return partially scanned files
        }
        catch (DirectoryNotFoundException dnfEx)
        {
            _logger.LogError(
                "Directory not found during scan. Directory: {Directory}, Exception: {Exception}",
                directoryPath,
                dnfEx.Message);
            return Enumerable.Empty<string>();
        }
        catch (UnauthorizedAccessException uaEx)
        {
            _logger.LogError(
                "Access denied during directory scan. Directory: {Directory}, Exception: {Exception}",
                directoryPath,
                uaEx.Message);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Unexpected error during directory scan. Directory: {Directory}, Exception: {Exception}",
                directoryPath,
                ex.Message);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Recursively scans directory and its subdirectories for files.
    /// </summary>
    private void ScanDirectoryRecursive(string directoryPath, List<string> files, bool includeSubdirectories, CancellationToken cancellationToken)
    {
        try
        {
            // Check cancellation at each directory level
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var directory = new DirectoryInfo(directoryPath);

            // Get all files in current directory
            var filesInDirectory = directory.GetFiles();
            foreach (var file in filesInDirectory)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                files.Add(file.FullPath);
                _logger.LogDebug("File discovered: {FilePath}", file.FullPath);
            }

            // Recursively scan subdirectories if requested
            if (includeSubdirectories)
            {
                var subdirectories = directory.GetDirectories();
                foreach (var subdirectory in subdirectories)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        ScanDirectoryRecursive(subdirectory.FullPath, files, includeSubdirectories, cancellationToken);
                    }
                    catch (UnauthorizedAccessException uaEx)
                    {
                        _logger.LogWarning(
                            "Access denied to subdirectory. Directory: {Directory}, Exception: {Exception}",
                            subdirectory.FullPath,
                            uaEx.Message);
                        // Continue scanning other directories
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            "Error scanning subdirectory. Directory: {Directory}, Exception: {Exception}",
                            subdirectory.FullPath,
                            ex.Message);
                        // Continue scanning other directories
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Error accessing directory. Directory: {Directory}, Exception: {Exception}",
                directoryPath,
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Validates that the directory path is not null, whitespace, and exists.
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
}
