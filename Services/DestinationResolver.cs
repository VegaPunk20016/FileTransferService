using FileTransferService.Configuration;
using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Resolves destination paths based on transfer strategy.
/// Supports two strategies:
/// 1. RemoveIntermediateFolder: Flattens hierarchy (Site/Machine/File.txt -> Site/File.txt)
/// 2. PreserveRelativeStructure: Maintains full hierarchy (Source/Dir1/Dir2/File -> Dest/Dir1/Dir2/File)
/// </summary>
public sealed class DestinationResolver : IDestinationResolver
{
    private readonly ILogger<DestinationResolver> _logger;

    public DestinationResolver(ILogger<DestinationResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Resolves the destination file path based on source path and transfer strategy.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="sourceRootPath">The source root directory path.</param>
    /// <param name="destinationRootPath">The destination root directory path.</param>
    /// <param name="strategy">The transfer strategy to apply.</param>
    /// <returns>The resolved destination file path.</returns>
    public string ResolveDestinationPath(
        string sourcePath,
        string sourceRootPath,
        string destinationRootPath,
        TransferStrategy strategy)
    {
        ValidateInputs(sourcePath, sourceRootPath, destinationRootPath);

        var destinationPath = strategy switch
        {
            TransferStrategy.RemoveIntermediateFolder => ResolveRemoveIntermediateFolder(sourcePath, sourceRootPath, destinationRootPath),
            TransferStrategy.PreserveRelativeStructure => ResolvePreserveRelativeStructure(sourcePath, sourceRootPath, destinationRootPath),
            _ => throw new ArgumentException($"Unknown transfer strategy: {strategy}", nameof(strategy))
        };

        _logger.LogInformation(
            "Destination path resolved. Strategy: {Strategy}, Source: {Source}, Destination: {Destination}",
            strategy,
            sourcePath,
            destinationPath);

        return destinationPath;
    }

    /// <summary>
    /// RemoveIntermediateFolder Strategy:
    /// Removes all intermediate directories and places file directly in destination root.
    /// Example: C:\Source\Site\Machine\SubDir\file.txt -> C:\Dest\file.txt
    /// </summary>
    private string ResolveRemoveIntermediateFolder(string sourcePath, string sourceRootPath, string destinationRootPath)
    {
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.Combine(destinationRootPath, fileName);

        _logger.LogDebug(
            "RemoveIntermediateFolder strategy applied. Source: {Source} -> Destination: {Destination}",
            sourcePath,
            destinationPath);

        return destinationPath;
    }

    /// <summary>
    /// PreserveRelativeStructure Strategy:
    /// Maintains the complete relative directory structure from source to destination.
    /// Example: C:\Source\Dir1\Dir2\file.txt -> C:\Dest\Dir1\Dir2\file.txt
    /// </summary>
    private string ResolvePreserveRelativeStructure(string sourcePath, string sourceRootPath, string destinationRootPath)
    {
        // Normalize paths to ensure consistent comparison
        var normalizedSourceRoot = NormalizePath(sourceRootPath);
        var normalizedSourcePath = NormalizePath(sourcePath);

        // Get relative path from source root
        if (!normalizedSourcePath.StartsWith(normalizedSourceRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Source path is not within source root. Source: {Source}, Root: {Root}",
                sourcePath,
                sourceRootPath);
            throw new ArgumentException(
                $"Source path '{sourcePath}' is not within source root '{sourceRootPath}'");
        }

        var relativePath = normalizedSourcePath
            .Substring(normalizedSourceRoot.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var destinationPath = Path.Combine(destinationRootPath, relativePath);

        // Ensure destination directory exists
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        _logger.LogDebug(
            "PreserveRelativeStructure strategy applied. Source: {Source} -> Destination: {Destination}",
            sourcePath,
            destinationPath);

        return destinationPath;
    }

    /// <summary>
    /// Normalizes path separators and removes trailing slashes for consistent comparison.
    /// </summary>
    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Validates that all input paths are not null or whitespace.
    /// </summary>
    private void ValidateInputs(string sourcePath, string sourceRootPath, string destinationRootPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            _logger.LogError("Source path is null or whitespace");
            throw new ArgumentException("Source path cannot be null or whitespace", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(sourceRootPath))
        {
            _logger.LogError("Source root path is null or whitespace");
            throw new ArgumentException("Source root path cannot be null or whitespace", nameof(sourceRootPath));
        }

        if (string.IsNullOrWhiteSpace(destinationRootPath))
        {
            _logger.LogError("Destination root path is null or whitespace");
            throw new ArgumentException("Destination root path cannot be null or whitespace", nameof(destinationRootPath));
        }
    }
}
