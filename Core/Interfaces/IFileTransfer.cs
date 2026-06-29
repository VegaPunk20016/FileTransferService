namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Performs the actual file transfer operation.
/// </summary>
public interface IFileTransfer
{
    /// <summary>
    /// Transfers a file from source to destination.
    /// </summary>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="destinationFilePath">The destination file path.</param>
    /// <param name="cancellationToken">Token to cancel the transfer.</param>
    /// <returns>True if transfer was successful; otherwise false.</returns>
    Task<bool> TransferAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken);
}
