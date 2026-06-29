using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Resolves file name collisions by generating unique filenames.
/// Strategy: file.txt -> file_2.txt -> file_3.txt -> ...
/// </summary>
public sealed class CollisionResolver : ICollisionResolver
{
    private readonly ILogger<CollisionResolver> _logger;

    public CollisionResolver(ILogger<CollisionResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks if a file path already exists.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the file exists; otherwise false.</returns>
    public bool FileExists(string filePath)
    {
        var exists = File.Exists(filePath);
        _logger.LogDebug("File existence check: {FilePath} - Exists: {Exists}", filePath, exists);
        return exists;
    }

    /// <summary>
    /// Resolves a filename collision by generating a new unique filename.
    /// Uses the pattern: filename.ext -> filename_2.ext -> filename_3.ext
    /// </summary>
    /// <param name="destinationPath">The original destination path where collision occurred.</param>
    /// <returns>A new unique destination path that does not exist.</returns>
    public string ResolveCollision(string destinationPath)
    {
        if (!FileExists(destinationPath))
        {
            _logger.LogDebug("No collision detected for path: {DestinationPath}", destinationPath);
            return destinationPath;
        }

        var directory = Path.GetDirectoryName(destinationPath);
        var filename = Path.GetFileNameWithoutExtension(destinationPath);
        var extension = Path.GetExtension(destinationPath);

        if (string.IsNullOrEmpty(directory))
        {
            _logger.LogError("Invalid destination path: {DestinationPath}", destinationPath);
            throw new ArgumentException($"Invalid destination path: {destinationPath}", nameof(destinationPath));
        }

        var counter = 2;
        var maxAttempts = 10000; // Prevent infinite loops

        while (counter <= maxAttempts)
        {
            var newFilename = $"{filename}_{counter}{extension}";
            var newPath = Path.Combine(directory, newFilename);

            if (!FileExists(newPath))
            {
                _logger.LogInformation(
                    "Collision resolved. Original: {OriginalPath} -> New: {NewPath}",
                    destinationPath,
                    newPath);
                return newPath;
            }

            counter++;
        }

        _logger.LogError(
            "Could not resolve collision after {MaxAttempts} attempts for: {DestinationPath}",
            maxAttempts,
            destinationPath);
        throw new InvalidOperationException(
            $"Could not resolve collision for file: {destinationPath} after {maxAttempts} attempts");
    }
}
