using FileTransferService.Core.Models;

namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Coordinates the entire file transfer process.
/// </summary>
public interface ITransferEngine
{
    /// <summary>
    /// Starts the transfer engine.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel engine operation.</param>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the transfer engine gracefully.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Processes a single transfer job.
    /// </summary>
    /// <param name="job">The transfer job to process.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result of the transfer operation.</returns>
    Task<TransferResult> ProcessJobAsync(TransferJob job, CancellationToken cancellationToken);
}
