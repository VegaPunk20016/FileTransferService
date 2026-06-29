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
                "Starting transfer orchestration. Source: {Source}, Destination: {Destination}",
                sourceDirectory,
                destinationDirectory);

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
                return result;
            }

            // Step 3: Validate files
            var validatedFiles = await ValidateFiles(scannedFiles, cancellationToken, result);
            if (validatedFiles.Count == 0)
            {
                result.Status = TransferStatus.Failed;
                result.Message = "No files passed validation.";
                return result;
            }

            // Step 4: Transfer files
            await TransferFiles(validatedFiles, destinationDirectory, cancellationToken, result);

            var duration = DateTime.UtcNow - startTime;
            result.Duration = duration;
            result.Status = result.TransferredCount > 0 ? TransferStatus.Completed : TransferStatus.Failed;
            result.Message = $"Transfer completed. Transferred: {result.TransferredCount}/{result.ValidatedCount} files.";

            _logger.LogInformation(
                "Transfer orchestration completed. Transferred: {Transferred}/{Validated}",
                result.TransferredCount,
                result.ValidatedCount);

            return result;
        }
        catch (OperationCanceledException ocEx)
        {
            result.Status = TransferStatus.Cancelled;
            result.Message = "Transfer operation was cancelled.";
            _logger.LogWarning("Transfer orchestration cancelled.");
            return result;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Transfer orchestration failed: {ex.Message}";
            _logger.LogError("Error in transfer orchestration. Exception: {Exception}", ex.Message);
            return result;
        }
    }

    private bool ValidateInputs(string sourceDirectory, string destinationDirectory, TransferResult result)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            result.Status = TransferStatus.Failed;
            result.Message = "Source directory path is null or empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            result.Status = TransferStatus.Failed;
            result.Message = "Destination directory path is null or empty.";
            return false;
        }

        if (!Directory.Exists(sourceDirectory))
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Source directory does not exist: {sourceDirectory}";
            return false;
        }

        return true;
    }

    private async Task<List<string>> ScanSourceDirectory(
        string sourceDirectory,
        bool includeSubdirectories,
        CancellationToken cancellationToken,
        TransferResult result)
    {
        try
        {
            var scannedFiles = await _fileScanner.ScanAsync(sourceDirectory, includeSubdirectories, cancellationToken);
            var fileList = scannedFiles.ToList();
            result.ScannedCount = fileList.Count;
            return fileList;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Failed to scan source directory: {ex.Message}";
            throw;
        }
    }

    private async Task<List<string>> ValidateFiles(
        List<string> scannedFiles,
        CancellationToken cancellationToken,
        TransferResult result)
    {
        var validatedFiles = new List<string>();

        try
        {
            foreach (var filePath in scannedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var isValid = await _fileValidator.ValidateAsync(filePath, cancellationToken);
                if (isValid)
                {
                    validatedFiles.Add(filePath);
                }
                else
                {
                    result.FailedCount++;
                }
            }

            result.ValidatedCount = validatedFiles.Count;
            return validatedFiles;
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Error during file validation: {ex.Message}";
            throw;
        }
    }

    private async Task TransferFiles(
        List<string> validatedFiles,
        string destinationDirectory,
        CancellationToken cancellationToken,
        TransferResult result)
    {
        try
        {
            foreach (var sourceFilePath in validatedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                try
                {
                    var destinationPath = _destinationResolver.ResolveDestinationPath(sourceFilePath, destinationDirectory);
                    var transferSuccess = await _fileTransfer.TransferAsync(sourceFilePath, destinationPath, cancellationToken);

                    if (transferSuccess)
                    {
                        result.TransferredCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    _logger.LogError("Error transferring file. Source: {Source}, Exception: {Exception}", sourceFilePath, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            result.Status = TransferStatus.Failed;
            result.Message = $"Error during file transfer: {ex.Message}";
            throw;
        }
    }
}
