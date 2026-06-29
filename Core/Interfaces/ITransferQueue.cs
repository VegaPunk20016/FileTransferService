using FileTransferService.Core.Models;

namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Manages a FIFO queue of transfer jobs to be processed.
/// </summary>
public interface ITransferQueue
{
    /// <summary>
    /// Adds a transfer job to the queue.
    /// </summary>
    /// <param name="job">The transfer job to enqueue.</param>
    void Enqueue(TransferJob job);

    /// <summary>
    /// Attempts to remove and return the next job from the queue.
    /// </summary>
    /// <param name="job">The dequeued job, or null if queue is empty.</param>
    /// <returns>True if a job was dequeued; otherwise false.</returns>
    bool TryDequeue(out TransferJob? job);

    /// <summary>
    /// Gets the current number of jobs in the queue.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets a value indicating whether the queue is empty.
    /// </summary>
    bool IsEmpty { get; }
}
