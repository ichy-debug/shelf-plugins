namespace Shelf.Plugin.Abstractions;

public interface IShelfPlugin
{
    PluginManifest Manifest { get; }
}

public interface IImportPlugin : IShelfPlugin
{
    bool CanImport(ImportSource source);

    Task<ImportPreview> PreviewAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ShelfItemCandidate> ImportAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record PluginManifest(
    string Id,
    string Name,
    string Version,
    string ShelfApiVersion,
    string Description,
    IReadOnlyList<PluginCapability> Capabilities,
    IReadOnlyList<PluginPermission> Permissions);

public enum PluginCapability
{
    Import,
    Metadata,
    Covers,
    Pricing
}

public enum PluginPermission
{
    ReadSelectedFiles,
    ReadSelectedFolders,
    NetworkAccess,
    CredentialAccess
}

public sealed record ImportSource(
    string DisplayName,
    string FileName,
    string? MediaType = null);

public sealed record ImportRequest(
    ImportSource Source,
    Stream Content,
    IReadOnlyDictionary<string, string>? Settings = null);

public sealed record ImportPreview(
    int CandidateCount,
    IReadOnlyList<ShelfItemCandidate> Samples,
    IReadOnlyList<PluginMessage> Messages);

public sealed record PluginMessage(
    PluginMessageSeverity Severity,
    string Code,
    string Message,
    string? SourceReference = null);

public enum PluginMessageSeverity
{
    Information,
    Warning,
    Error
}

