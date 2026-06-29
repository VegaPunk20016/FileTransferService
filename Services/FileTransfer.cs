using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Performs the actual file transfer operation.
/// Uses File.Move to transfer files from source to destination.
/// </summary>
public sealed class FileTransfer : IFileTransfer
{
    private readonly ILogger<FileTransfer> _logger;

    public FileTransfer(ILogger<FileTransfer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Transfers a file from source to destination.
    /// </summary>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="destinationFilePath">The destination file path.</param>
    /// <param name="cancellationToken">Token to cancel the transfer.</param>
    /// <returns>True if transfer was successful; otherwise false.</returns>
    public async Task<bool> TransferAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
    {
        // Validate inputs
        if (!ValidateInputs(sourceFilePath, destinationFilePath))
        {
            return false;
        }

        // Check cancellation before starting transfer
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("File transfer cancelled before start. Source: {Source}", sourceFilePath);
            return false;
        }

        try
        {
            _logger.LogInformation(
                "Starting file transfer. Source: {Source}, Destination: {Destination}",
                sourceFilePath,
                destinationFilePath);

            var sourceFileInfo = new FileInfo(sourceFilePath);
            var startTime = DateTime.UtcNow;

            // Perform the transfer using File.Move
            // This is an atomic operation that moves the file (or copies on some file systems)
            await Task.Run(() => 
            {
                File.Move(sourceFilePath, destinationFilePath, overwrite: false);
            }, cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "File transfer completed successfully. Source: {Source}, Destination: {Destination}, " +
                "FileSize: {FileSize} bytes, Duration: {Duration}ms",
                sourceFilePath,
                destinationFilePath,
                sourceFileInfo.Length,
                duration.TotalMilliseconds);

            return true;
        }
        catch (OperationCanceledException ocEx)
        {
            _logger.LogWarning(
                "File transfer cancelled during operation. Source: {Source}, Destination: {Destination}, Exception: {Exception}",
                sourceFilePath,
                destinationFilePath,
                ocEx.Message);
            return false;
        }
        catch (FileNotFoundException fnfEx)
        {
            _logger.LogError(
                "Source file not found during transfer. Source: {Source}, Exception: {Exception}",
                sourceFilePath,
                fnfEx.Message);
            return false;
        }
        catch (DirectoryNotFoundException dnfEx)
        {
            _logger.LogError(
                "Destination directory not found during transfer. Destination: {Destination}, Exception: {Exception}",
                destinationFilePath,
                dnfEx.Message);
            return false;
        }
        catch (UnauthorizedAccessException uaEx)
        {
            _logger.LogError(
                "Access denied during file transfer. Source: {Source}, Destination: {Destination}, Exception: {Exception}",
                sourceFilePath,
                destinationFilePath,
                uaEx.Message);
            return false;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(
                "IO error during file transfer. Source: {Source}, Destination: {Destination}, Exception: {Exception}",
                sourceFilePath,
                destinationFilePath,
                ioEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Unexpected error during file transfer. Source: {Source}, Destination: {Destination}, Exception: {Exception}",
                sourceFilePath,
                destinationFilePath,
                ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Validates that input paths are not null or whitespace.
    /// </summary>
    private bool ValidateInputs(string sourceFilePath, string destinationFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            _logger.LogWarning("Source file path is null or whitespace");
            return false;
        }

        if (string.IsNullOrWhiteSpace(destinationFilePath))
        {
            _logger.LogWarning("Destination file path is null or whitespace");
            return false;
        }

        return true;
    }
}
