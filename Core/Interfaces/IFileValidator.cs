namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Validates file readiness before transfer.
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Validates that a file is ready for transfer (not locked, accessible, etc.).
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="timeoutSeconds">Maximum seconds to wait for file to be ready.</param>
    /// <param name="cancellationToken">Token to cancel the validation.</param>
    /// <returns>True if file is valid and ready; otherwise false.</returns>
    Task<bool> IsFileReadyAsync(string filePath, int timeoutSeconds, CancellationToken cancellationToken);
}
