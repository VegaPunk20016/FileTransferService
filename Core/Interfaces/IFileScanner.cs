namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Interface for scanning directories to find files.
/// </summary>
public interface IFileScanner
{
    /// <summary>
    /// Scans a directory for files asynchronously.
    /// </summary>
    /// <param name="directory">The directory path to scan.</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An enumerable collection of file paths found.</returns>
    Task<IEnumerable<string>> ScanAsync(string directory, bool includeSubdirectories, CancellationToken cancellationToken);
}
