using FileTransferService.Core.Interfaces;
using FileTransferService.Core.Models;
using System.Collections.Concurrent;

namespace FileTransferService.Services;

/// <summary>
/// Thread-safe FIFO queue implementation for transfer jobs.
/// </summary>
public sealed class TransferQueue : ITransferQueue
{
    private readonly ConcurrentQueue<TransferJob> _queue;
    private readonly ILogger<TransferQueue> _logger;

    public TransferQueue(ILogger<TransferQueue> logger)
    {
        _queue = new ConcurrentQueue<TransferJob>();
        _logger = logger;
    }

    /// <summary>
    /// Gets the current number of jobs in the queue.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Gets a value indicating whether the queue is empty.
    /// </summary>
    public bool IsEmpty => _queue.IsEmpty;

    /// <summary>
    /// Adds a transfer job to the queue.
    /// </summary>
    /// <param name="job">The transfer job to enqueue.</param>
    public void Enqueue(TransferJob job)
    {
        if (job == null)
        {
            _logger.LogWarning("Attempted to enqueue null job");
            return;
        }

        _queue.Enqueue(job);
        _logger.LogInformation(
            "Job enqueued. JobId: {JobId}, TransferDefinition: {TransferDefinition}, QueueCount: {QueueCount}",
            job.Id,
            job.TransferDefinitionName,
            _queue.Count);
    }

    /// <summary>
    /// Attempts to remove and return the next job from the queue.
    /// </summary>
    /// <param name="job">The dequeued job, or null if queue is empty.</param>
    /// <returns>True if a job was dequeued; otherwise false.</returns>
    public bool TryDequeue(out TransferJob? job)
    {
        var success = _queue.TryDequeue(out job);

        if (success && job != null)
        {
            _logger.LogInformation(
                "Job dequeued. JobId: {JobId}, TransferDefinition: {TransferDefinition}, RemainingCount: {RemainingCount}",
                job.Id,
                job.TransferDefinitionName,
                _queue.Count);
        }

        return success;
    }
}
