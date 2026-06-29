namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Interface for transferring files.
/// </summary>
public interface IFileTransfer
{
    /// <summary>
    /// Transfers a file from source to destination asynchronously.
    /// </summary>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="destinationFilePath">The destination file path.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if transfer succeeded, false otherwise.</returns>
    Task<bool> TransferAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken);
}
