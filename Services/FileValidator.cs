using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Validates file readiness before transfer.
/// Checks if file is accessible and not locked by other processes.
/// </summary>
public sealed class FileValidator : IFileValidator
{
    private readonly ILogger<FileValidator> _logger;
    private const int RetryDelayMilliseconds = 100;

    public FileValidator(ILogger<FileValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates that a file is ready for transfer (not locked, accessible, etc.).
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="timeoutSeconds">Maximum seconds to wait for file to be ready.</param>
    /// <param name="cancellationToken">Token to cancel the validation.</param>
    /// <returns>True if file is valid and ready; otherwise false.</returns>
    public async Task<bool> IsFileReadyAsync(string filePath, int timeoutSeconds, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("File path is null or whitespace");
            return false;
        }

        if (timeoutSeconds < 0)
        {
            _logger.LogWarning("Timeout seconds cannot be negative: {TimeoutSeconds}", timeoutSeconds);
            return false;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var timeoutMilliseconds = timeoutSeconds * 1000;

        while (stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("File validation cancelled for: {FilePath}", filePath);
                return false;
            }

            if (IsFileFree(filePath))
            {
                _logger.LogInformation(
                    "File is ready for transfer. FilePath: {FilePath}, ElapsedTime: {ElapsedMs}ms",
                    filePath,
                    stopwatch.ElapsedMilliseconds);
                return true;
            }

            // Wait before retrying
            try
            {
                await Task.Delay(RetryDelayMilliseconds, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("File validation cancelled for: {FilePath}", filePath);
                return false;
            }
        }

        stopwatch.Stop();
        _logger.LogWarning(
            "File did not become ready within timeout. FilePath: {FilePath}, TimeoutSeconds: {TimeoutSeconds}",
            filePath,
            timeoutSeconds);
        return false;
    }

    /// <summary>
    /// Checks if a file is not locked and is accessible.
    /// Attempts to open the file with exclusive write access.
    /// </summary>
    private bool IsFileFree(string filePath)
    {
        try
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                _logger.LogDebug("File does not exist: {FilePath}", filePath);
                return false;
            }

            // Try to open the file with exclusive write access
            // If it fails, the file is locked by another process
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                fileStream.Close();
            }

            _logger.LogDebug("File is free (not locked): {FilePath}", filePath);
            return true;
        }
        catch (IOException ioEx)
        {
            _logger.LogDebug(
                "File is locked or inaccessible. FilePath: {FilePath}, Exception: {Exception}",
                filePath,
                ioEx.Message);
            return false;
        }
        catch (UnauthorizedAccessException uaEx)
        {
            _logger.LogDebug(
                "Access denied to file. FilePath: {FilePath}, Exception: {Exception}",
                filePath,
                uaEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                "Unexpected error checking file status. FilePath: {FilePath}, Exception: {Exception}",
                filePath,
                ex.Message);
            return false;
        }
    }
}
