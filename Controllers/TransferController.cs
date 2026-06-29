using FileTransferService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileTransferService.Controllers;

/// <summary>
/// API controller for file transfer operations.
/// Provides endpoints to orchestrate file scanning, validation, and transfer.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransferController : ControllerBase
{
    private readonly ILogger<TransferController> _logger;
    private readonly ITransferOrchestrator _transferOrchestrator;

    public TransferController(
        ILogger<TransferController> logger,
        ITransferOrchestrator transferOrchestrator)
    {
        _logger = logger;
        _transferOrchestrator = transferOrchestrator;
    }

    /// <summary>
    /// Executes a complete file transfer operation.
    /// Scans source directory, validates files, and transfers to destination.
    /// </summary>
    /// <param name="request">Transfer request containing source and destination paths.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Transfer result with statistics.</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(TransferResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExecuteTransfer(
        [FromBody] ExecuteTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Transfer request received. Source: {Source}, Destination: {Destination}",
                request.SourceDirectory,
                request.DestinationDirectory);

            if (string.IsNullOrWhiteSpace(request.SourceDirectory) ||
                string.IsNullOrWhiteSpace(request.DestinationDirectory))
            {
                _logger.LogWarning("Invalid request: missing required fields.");
                return BadRequest(new { message = "Source and destination directories are required." });
            }

            var result = await _transferOrchestrator.ExecuteTransferAsync(
                request.SourceDirectory,
                request.DestinationDirectory,
                request.IncludeSubdirectories,
                cancellationToken);

            var dto = new TransferResultDto
            {
                Status = result.Status.ToString(),
                Message = result.Message,
                ScannedCount = result.ScannedCount,
                ValidatedCount = result.ValidatedCount,
                TransferredCount = result.TransferredCount,
                FailedCount = result.FailedCount,
                DurationMilliseconds = result.Duration.TotalMilliseconds
            };

            _logger.LogInformation(
                "Transfer completed. Status: {Status}, Transferred: {Transferred}/{Validated}",
                result.Status,
                result.TransferredCount,
                result.ValidatedCount);

            return Ok(dto);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Transfer operation was cancelled.");
            return Ok(new TransferResultDto
            {
                Status = "Cancelled",
                Message = "Transfer operation was cancelled."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error executing transfer. Exception: {Exception}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred during transfer operation.", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for executing a transfer operation.
/// </summary>
public class ExecuteTransferRequest
{
    public string SourceDirectory { get; set; } = string.Empty;
    public string DestinationDirectory { get; set; } = string.Empty;
    public bool IncludeSubdirectories { get; set; } = false;
}

/// <summary>
/// Response model for transfer operation results.
/// </summary>
public class TransferResultDto
{
    public string Status { get; set; } = "InProgress";
    public string Message { get; set; } = string.Empty;
    public int ScannedCount { get; set; }
    public int ValidatedCount { get; set; }
    public int TransferredCount { get; set; }
    public int FailedCount { get; set; }
    public double DurationMilliseconds { get; set; }
}
