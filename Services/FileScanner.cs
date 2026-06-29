using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Service for scanning directories to find files.
/// </summary>
public class FileScanner : IFileScanner
{
    private readonly ILogger<FileScanner> _logger;

    public FileScanner(ILogger<FileScanner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Scans a directory for files asynchronously.
    /// </summary>
    public async Task<IEnumerable<string>> ScanAsync(string directory, bool includeSubdirectories, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Starting directory scan. Directory: {Directory}, IncludeSubdirectories: {IncludeSubdirectories}",
                directory,
                includeSubdirectories);

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Directory.GetFiles(directory, "*.*", searchOption).ToList();
            }, cancellationToken);

            _logger.LogInformation(
                "Directory scan completed. Directory: {Directory}, FilesFound: {Count}",
                directory,
                files.Count);

            return files;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Directory scan was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error scanning directory. Exception: {Exception}", ex.Message);
            throw;
        }
    }
}
