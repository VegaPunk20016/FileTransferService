using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Service for validating files before transfer.
/// </summary>
public class FileValidator : IFileValidator
{
    private readonly ILogger<FileValidator> _logger;

    // Configuration
    private const long MaxFileSizeBytes = 5L * 1024 * 1024 * 1024; // 5 GB

    public FileValidator(ILogger<FileValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a file asynchronously.
    /// </summary>
    public async Task<bool> ValidateAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File does not exist. FilePath: {FilePath}", filePath);
                    return false;
                }

                // Check file size
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    _logger.LogWarning(
                        "File exceeds maximum size. FilePath: {FilePath}, Size: {Size}MB",
                        filePath,
                        fileInfo.Length / (1024 * 1024));
                    return false;
                }

                // Check file accessibility
                try
                {
                    using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        // File is accessible
                    }
                }
                catch (IOException)
                {
                    _logger.LogWarning("File is not accessible. FilePath: {FilePath}", filePath);
                    return false;
                }

                _logger.LogDebug("File validation passed. FilePath: {FilePath}", filePath);
                return true;
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File validation was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error validating file. FilePath: {FilePath}, Exception: {Exception}", filePath, ex.Message);
            return false;
        }
    }
}
