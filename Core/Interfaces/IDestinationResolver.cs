namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Interface for resolving destination paths for files.
/// </summary>
public interface IDestinationResolver
{
    /// <summary>
    /// Resolves the destination path for a file.
    /// </summary>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="destinationDirectory">The destination directory.</param>
    /// <returns>The full destination file path.</returns>
    string ResolveDestinationPath(string sourceFilePath, string destinationDirectory);
}
