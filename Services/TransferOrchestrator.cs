using FileTransferService.Core.Interfaces;

namespace FileTransferService.Services;

/// <summary>
/// Orchestrates the complete file transfer workflow.
/// Coordinates FileScanner, FileValidator, and FileTransfer services.
/// </summary>
public sealed class TransferOrchestrator : ITransferOrchestrator
{
    private readonly ILogger<TransferOrchestrator> _logger;
    private readonly IFileScanner _fileScanner;
    private readonly IFileValidator _fileValidator;
    private readonly IFileTransfer _fileTransfer;
    private readonly IDestinationResolver _destinationResolver;

    public TransferOrchestrator(
        ILogger<TransferOrchestrator> logger,
        IFileScanner fileScanner,
        IFileValidator fileValidator,
        IFileTransfer fileTransfer,
        IDestinationResolver destinationResolver)
    {
        _logger = logger;
        _fileScanner = fileScanner;
        _fileValidator = fileValidator;
        _fileTransfer = fileTransfer;
        _destinationResolver = destinationResolver;
    }

    /// <summary>
    /// Orchestrates the complete transfer workflow for a directory.
    /// Scans → Validates → Transfers files.
    /// </summary>
    /// <param name="sourceDirectory">The source directory to scan and transfer from.</param>
    /// <param name="destinationDirectory">The destination directory to transfer files to.</param>
    /// <param name="includeSubdirectories">Whether to include files from subdirectories.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Transfer result with statistics and details.</returns>
    public async Task<TransferResult> ExecuteTransferAsync(
        string sourceDirectory,
        string destinationDirectory,
        bool includeSubdirectories,
        CancellationToken cancellationToken)
    {
        var result = new TransferResult();

        try
        {
            _logger.LogInformation(
                "Starting transfer orchestration. Source: {Source}, Destination: {Destination}, IncludeSubdirectories: {IncludeSubdirectories}",
                sourceDirectory,
                destinationDirectory,
                includeSubdirectories);

            var startTime = DateTime.UtcNow;

            // Step 1: Validate inputs
            if (!ValidateInputs(sourceDirectory, destinationDirectory, result))
            {
                return result;
            }

            // Step 2: Scan source directory
            var scannedFiles = await ScanSourceDirectory(sourceDirectory, includeSubdirectories, cancellationToken, result);
            if (scannedFiles.Count == 0)
            {
                result.Status = TransferStatus.Completed;
                result.Message = "No files found to transfer.";
                _logger.LogInformation("No files found in source directory. Source: {Source}", sourceDirectory);
                return result;
            }

            // Step 3: Validate files
            var validatedFiles = await ValidateFiles(scannedFiles, cancellationToken, result);
            if (validatedFiles.Count == 0)
            {
                result.Status = TransferStatus.Failed;
                result.Message = "No files passed validation.";
                _logger.LogWarning("All files failed validation. Source: {Source}", sourceDirectory);
                return result;
            }

            // Step 4: Transfer files
            await TransferFiles(validatedFiles, destinationDirectory, cancellationToken, result);

            var duration = DateTime.UtcNow - startTime;
            result.Duration = duration;
            result.Status = result.TransferredCount > 0 ? TransferStatus.Completed : TransferStatus.Failed;
            result.Message = $"Transfer completed. Transferred: {result.TransferredCount}/{result.ValidatedCount} files.";

            _logger.LogInformation(
                "Transfer orchestration completed. Transferred: {Transferred}/{Validated}, Failed: {Failed}, Duration: {Duration}ms",
                result.TransferredCount,
                result.ValidatedCount,
                result.FailedCount,
                duration.TotalMilliseconds);

            return result;
        }
        catch (OperationCanceledException ocEx)
        {
            result.Status = TransferStatus.Cancelled;
            result.Message = "Transfer operation was cancelled.";
            _logger.LogWarning("Transfer orchestration cancelled. Exception: {Exception}", ocEx.Message);
            return result;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Transfer orchestration failed: {ex.Message}";
            _logger.LogError("Unexpected error in transfer orchestration. Exception: {Exception}", ex.Message);
            return result;
        }
    }

    /// <summary>
    /// Validates input parameters.
    /// </summary>
    private bool ValidateInputs(string sourceDirectory, string destinationDirectory, TransferResult result)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            result.Status = TransferStatus.Failed;
            result.Message = "Source directory path is null or empty.";
            _logger.LogWarning("Source directory path is null or empty.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            result.Status = TransferStatus.Failed;
            result.Message = "Destination directory path is null or empty.";
            _logger.LogWarning("Destination directory path is null or empty.");
            return false;
        }

        if (!Directory.Exists(sourceDirectory))
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Source directory does not exist: {sourceDirectory}";
            _logger.LogWarning("Source directory does not exist. Directory: {Directory}", sourceDirectory);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Scans the source directory for files.
    /// </summary>
    private async Task<List<string>> ScanSourceDirectory(
        string sourceDirectory,
        bool includeSubdirectories,
        CancellationToken cancellationToken,
        TransferResult result)
    {
        try
        {
            _logger.LogInformation("Scanning source directory. Directory: {Directory}", sourceDirectory);
            var scannedFiles = await _fileScanner.ScanAsync(sourceDirectory, includeSubdirectories, cancellationToken);
            var fileList = scannedFiles.ToList();
            result.ScannedCount = fileList.Count;

            _logger.LogInformation(
                "Source directory scan completed. ScannedCount: {ScannedCount}",
                result.ScannedCount);

            return fileList;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Scan operation was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Failed to scan source directory: {ex.Message}";
            _logger.LogError("Error scanning source directory. Exception: {Exception}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Validates scanned files.
    /// </summary>
    private async Task<List<string>> ValidateFiles(
        List<string> scannedFiles,
        CancellationToken cancellationToken,
        TransferResult result)
    {
        var validatedFiles = new List<string>();

        try
        {
            _logger.LogInformation("Validating scanned files. Count: {Count}", scannedFiles.Count);

            foreach (var filePath in scannedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Validation cancelled.");
                    throw new OperationCanceledException();
                }

                var isValid = await _fileValidator.ValidateAsync(filePath, cancellationToken);
                if (isValid)
                {
                    validatedFiles.Add(filePath);
                    _logger.LogDebug("File validated successfully. FilePath: {FilePath}", filePath);
                }
                else
                {
                    result.FailedCount++;
                    _logger.LogWarning("File validation failed. FilePath: {FilePath}", filePath);
                }
            }

            result.ValidatedCount = validatedFiles.Count;

            _logger.LogInformation(
                "File validation completed. ValidatedCount: {ValidatedCount}, FailedCount: {FailedCount}",
                result.ValidatedCount,
                result.FailedCount);

            return validatedFiles;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Validation operation was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Error during file validation: {ex.Message}";
            _logger.LogError("Error validating files. Exception: {Exception}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Transfers validated files to destination.
    /// </summary>
    private async Task TransferFiles(
        List<string> validatedFiles,
        string destinationDirectory,
        CancellationToken cancellationToken,
        TransferResult result)
    {
        try
        {
            _logger.LogInformation(
                "Starting file transfer. DestinationDirectory: {Destination}, FileCount: {Count}",
                destinationDirectory,
                validatedFiles.Count);

            foreach (var sourceFilePath in validatedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Transfer cancelled.");
                    throw new OperationCanceledException();
                }

                try
                {
                    // Resolve destination path
                    var destinationPath = _destinationResolver.ResolveDestinationPath(sourceFilePath, destinationDirectory);

                    // Transfer file
                    var transferSuccess = await _fileTransfer.TransferAsync(sourceFilePath, destinationPath, cancellationToken);

                    if (transferSuccess)
                    {
                        result.TransferredCount++;
                        _logger.LogInformation(
                            "File transferred successfully. Source: {Source}, Destination: {Destination}",
                            sourceFilePath,
                            destinationPath);
                    }
                    else
                    {
                        result.FailedCount++;
                        _logger.LogWarning(
                            "File transfer failed. Source: {Source}, Destination: {Destination}",
                            sourceFilePath,
                            destinationPath);
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    _logger.LogError(
                        "Error transferring file. Source: {Source}, Exception: {Exception}",
                        sourceFilePath,
                        ex.Message);
                }
            }

            _logger.LogInformation(
                "File transfer completed. TransferredCount: {Transferred}, FailedCount: {Failed}",
                result.TransferredCount,
                result.FailedCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Transfer operation was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Error during file transfer: {ex.Message}";
            _logger.LogError("Error transferring files. Exception: {Exception}", ex.Message);
            throw;
        }
    }
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
