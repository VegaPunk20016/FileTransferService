namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Interface for orchestrating the complete file transfer workflow.
/// </summary>
public interface ITransferOrchestrator
{
    /// <summary>
    /// Executes the complete transfer workflow.
    /// </summary>
    /// <param name="sourceDirectory">The source directory to scan and transfer from.</param>
    /// <param name="destinationDirectory">The destination directory to transfer to.</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result of the transfer operation.</returns>
    Task<TransferResult> ExecuteTransferAsync(string sourceDirectory, string destinationDirectory, bool includeSubdirectories, CancellationToken cancellationToken);
}

/// <summary>
/// Represents the result of a transfer operation.
/// </summary>
public class TransferResult
{
    public TransferStatus Status { get; set; } = TransferStatus.InProgress;
    public string Message { get; set; } = "Transfer in progress...";
    public int ScannedCount { get; set; }
    public int ValidatedCount { get; set; }
    public int TransferredCount { get; set; }
    public int FailedCount { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Enumeration for transfer operation status.
/// </summary>
public enum TransferStatus
{
    InProgress,
    Completed,
    Failed,
    Cancelled
}
