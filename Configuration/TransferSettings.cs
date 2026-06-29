namespace FileTransferService.Configuration;

/// <summary>
/// Root configuration class for all transfer settings.
/// Loaded from appsettings.json under the "TransferSettings" section.
/// </summary>
public sealed class TransferSettings
{
    /// <summary>
    /// Gets the collection of transfer definitions configured for this service.
    /// </summary>
    public IList<TransferDefinition> Transfers { get; init; } = new List<TransferDefinition>();
}
