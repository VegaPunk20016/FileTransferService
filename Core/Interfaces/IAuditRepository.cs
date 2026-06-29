using FileTransferService.Core.Models;

namespace FileTransferService.Core.Interfaces;

/// <summary>
/// Persists transfer results for auditing purposes.
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Records a transfer result to the audit log.
    /// </summary>
    /// <param name="result">The transfer result to record.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task RecordResultAsync(TransferResult result, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all recorded transfer results.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of all recorded transfer results.</returns>
    Task<IEnumerable<TransferResult>> GetAllResultsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves transfer results for a specific transfer definition.
    /// </summary>
    /// <param name="transferDefinitionName">The name of the transfer definition.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of results matching the transfer definition.</returns>
    Task<IEnumerable<TransferResult>> GetResultsByDefinitionAsync(string transferDefinitionName, CancellationToken cancellationToken);
}
