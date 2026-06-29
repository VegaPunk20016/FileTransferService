namespace FileTransferService.Core.Models;

/// <summary>
/// Represents a file transfer job in progress.
/// </summary>
public sealed class TransferJob
{
    /// <summary>
    /// Gets the unique identifier for this transfer job.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name of the transfer definition this job belongs to.
    /// </summary>
    public required string TransferDefinitionName { get; init; }

    /// <summary>
    /// Gets the full path to the source file to be transferred.
    /// </summary>
    public required string SourceFilePath { get; init; }

    /// <summary>
    /// Gets the destination file path where the file will be transferred to.
    /// </summary>
    public required string DestinationFilePath { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Gets the date and time when this job was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time when this job was started.
    /// </summary>
    public DateTime? StartedAt { get; init; }

    /// <summary>
    /// Gets the current status of the transfer job.
    /// </summary>
    public required TransferJobStatus Status { get; init; }
}
