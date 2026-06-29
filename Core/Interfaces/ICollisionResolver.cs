namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Handles file name collision resolution when destination file already exists.
/// </summary>
public interface ICollisionResolver
{
    /// <summary>
    /// Resolves a filename collision by generating a new unique filename.
    /// </summary>
    /// <param name="destinationPath">The original destination path where collision occurred.</param>
    /// <returns>A new unique destination path that does not exist.</returns>
    string ResolveCollision(string destinationPath);

    /// <summary>
    /// Checks if a file path already exists.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the file exists; otherwise false.</returns>
    bool FileExists(string filePath);
}
