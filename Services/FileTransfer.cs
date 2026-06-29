using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Service for transferring files.
/// </summary>
public class FileTransfer : IFileTransfer
{
    private readonly ILogger<FileTransfer> _logger;
    private const int BufferSize = 81920; // 80 KB

    public FileTransfer(ILogger<FileTransfer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Transfers a file from source to destination asynchronously.
    /// </summary>
    public async Task<bool> TransferAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Starting file transfer. Source: {Source}, Destination: {Destination}",
                sourceFilePath,
                destinationFilePath);

            // Ensure destination directory exists
            var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
                _logger.LogInformation("Created destination directory: {Directory}", destinationDirectory);
            }

            // Copy file asynchronously
            await Task.Run(() =>
            {
                using (var sourceStream = new FileStream(
                    sourceFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    BufferSize,
                    useAsync: true))
                using (var destinationStream = new FileStream(
                    destinationFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    BufferSize,
                    useAsync: true))
                {
                    sourceStream.CopyTo(destinationStream, BufferSize);
                }
            }, cancellationToken);

            _logger.LogInformation(
                "File transfer completed successfully. Source: {Source}, Destination: {Destination}",
                sourceFilePath,
                destinationFilePath);

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File transfer was cancelled. Source: {Source}", sourceFilePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Error transferring file. Source: {Source}, Destination: {Destination}, Exception: {Exception}",
                sourceFilePath,
                destinationFilePath,
                ex.Message);
            return false;
        }
    }
}
