namespace FileTransferService.Configuration;

/// <summary>
/// Defines the strategy for handling the directory structure during file transfers.
/// </summary>
public enum TransferStrategy
{
    /// <summary>
    /// Removes intermediate folders in the hierarchy and flattens the structure.
    /// Example: Site/Machine/File.txt becomes Site/File.txt
    /// </summary>
    RemoveIntermediateFolder,

    /// <summary>
    /// Preserves the complete relative directory structure from source to destination.
    /// Example: Source/Dir1/Dir2/File.txt becomes Destination/Dir1/Dir2/File.txt
    /// </summary>
    PreserveRelativeStructure
}
