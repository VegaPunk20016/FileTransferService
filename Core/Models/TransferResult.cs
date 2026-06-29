namespace FileTransferService.Core.Models;

/// <summary>
/// Represents the result of a completed file transfer operation.
/// </summary>
public sealed class TransferResult
{
    /// <summary>
    /// Gets the unique identifier of the transfer job associated with this result.
    /// </summary>
    public required string TransferJobId { get; init; }

    /// <summary>
    /// Gets the name of the transfer definition this result belongs to.
    /// </summary>
    public required string TransferDefinitionName { get; init; }

    /// <summary>
    /// Gets the source file path.
    /// </summary>
    public required string SourceFilePath { get; init; }

    /// <summary>
    /// Gets the destination file path.
    /// </summary>
    public required string DestinationFilePath { get; init; }

    /// <summary>
    /// Gets a value indicating whether the transfer was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the error message if the transfer failed; otherwise null.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the exception details if an error occurred; otherwise null.
    /// </summary>
    public string? ExceptionDetails { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Gets the date and time when the transfer started.
    /// </summary>
    public required DateTime StartedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the transfer completed.
    /// </summary>
    public required DateTime CompletedAt { get; init; }
}
