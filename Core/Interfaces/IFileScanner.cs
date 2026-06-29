namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Scans a directory recursively and discovers all files matching criteria.
/// </summary>
public interface IFileScanner
{
    /// <summary>
    /// Scans the specified directory and returns all files found.
    /// </summary>
    /// <param name="directoryPath">The root directory path to scan.</param>
    /// <param name="includeSubdirectories">Whether to include files in subdirectories.</param>
    /// <param name="cancellationToken">Token to cancel the scan operation.</param>
    /// <returns>A collection of file paths found during the scan.</returns>
    Task<IEnumerable<string>> ScanAsync(string directoryPath, bool includeSubdirectories, CancellationToken cancellationToken);
}
