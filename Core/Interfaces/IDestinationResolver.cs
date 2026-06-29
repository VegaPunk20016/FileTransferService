using FileTransferService.Configuration;

namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Resolves the destination path for a file based on transfer strategy.
/// </summary>
public interface IDestinationResolver
{
    /// <summary>
    /// Resolves the destination file path based on source path and transfer strategy.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="sourceRootPath">The source root directory path.</param>
    /// <param name="destinationRootPath">The destination root directory path.</param>
    /// <param name="strategy">The transfer strategy to apply.</param>
    /// <returns>The resolved destination file path.</returns>
    string ResolveDestinationPath(
        string sourcePath,
        string sourceRootPath,
        string destinationRootPath,
        TransferStrategy strategy);
}
