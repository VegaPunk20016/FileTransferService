using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Service for resolving destination paths for files.
/// </summary>
public class DestinationResolver : IDestinationResolver
{
    private readonly ILogger<DestinationResolver> _logger;

    public DestinationResolver(ILogger<DestinationResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Resolves the destination path for a file.
    /// </summary>
    public string ResolveDestinationPath(string sourceFilePath, string destinationDirectory)
    {
        try
        {
            var fileName = Path.GetFileName(sourceFilePath);
            var destinationPath = Path.Combine(destinationDirectory, fileName);

            _logger.LogDebug(
                "Destination path resolved. Source: {Source}, Destination: {Destination}",
                sourceFilePath,
                destinationPath);

            return destinationPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Error resolving destination path. Source: {Source}, Exception: {Exception}",
                sourceFilePath,
                ex.Message);
            throw;
        }
    }
}
