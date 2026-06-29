namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Interface for validating files before transfer.
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Validates a file asynchronously.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if file is valid, false otherwise.</returns>
    Task<bool> ValidateAsync(string filePath, CancellationToken cancellationToken);
}
