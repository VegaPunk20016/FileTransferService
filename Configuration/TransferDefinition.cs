namespace FileTransferService.Configuration;

/// <summary>
/// Defines a single transfer job configuration.
/// </summary>
public sealed class TransferDefinition
{
    /// <summary>
    /// Gets the name of this transfer definition.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the source directory path where files are monitored for transfer.
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// Gets the destination directory path where files will be transferred to.
    /// </summary>
    public required string DestinationPath { get; init; }

    /// <summary>
    /// Gets the transfer strategy that determines how directory structures are handled.
    /// </summary>
    public required TransferStrategy Strategy { get; init; }

    /// <summary>
    /// Gets a value indicating whether subdirectories should be included in the transfer.
    /// </summary>
    public bool IncludeSubdirectories { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of concurrent transfers allowed for this definition.
    /// </summary>
    public int MaxConcurrentTransfers { get; init; } = 1;

    /// <summary>
    /// Gets the timeout in seconds to wait for a file to be released before attempting transfer.
    /// </summary>
    public int FileValidationTimeoutSeconds { get; init; } = 5;
}
