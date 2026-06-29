namespace FileTransferService.Core.Models;

/// <summary>
/// Represents the status of a transfer job.
/// </summary>
public enum TransferJobStatus
{
    /// <summary>
    /// The job has been queued but not yet processed.
    /// </summary>
    Queued,

    /// <summary>
    /// The job is currently being validated before transfer.
    /// </summary>
    Validating,

    /// <summary>
    /// The job is currently being transferred.
    /// </summary>
    Transferring,

    /// <summary>
    /// The job has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The job failed during transfer.
    /// </summary>
    Failed,

    /// <summary>
    /// The job was cancelled before completion.
    /// </summary>
    Cancelled
}
