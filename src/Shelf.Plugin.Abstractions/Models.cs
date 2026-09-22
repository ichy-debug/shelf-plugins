namespace Shelf.Plugin.Abstractions;

public sealed record ShelfItemCandidate
{
    public required string SourceItemId { get; init; }
    public required ShelfMediaType MediaType { get; init; }
    public required string Title { get; init; }
    public string? Series { get; init; }
    public string? Number { get; init; }
    public string? Volume { get; init; }
    public string? Variant { get; init; }
    public string? Isbn { get; init; }
    public string? Issn { get; init; }
    public string? Barcode { get; init; }
    public string? Publisher { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public string? Description { get; init; }
    public string? Language { get; init; }
    public string? Condition { get; init; }
    public decimal? EstimatedValue { get; init; }
    public string? Currency { get; init; }
    public string? Location { get; init; }
    public IReadOnlyList<string> Creators { get; init; } = [];
    public IReadOnlyList<string> Characters { get; init; } = [];
    public IReadOnlyList<CoverCandidate> Covers { get; init; } = [];
    public IReadOnlyDictionary<string, string> ExternalIdentifiers { get; init; }
        = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> AdditionalFields { get; init; }
        = new Dictionary<string, string>();
    public required SourceProvenance Provenance { get; init; }
    public double Confidence { get; init; } = 1.0;
}

public enum ShelfMediaType
{
    Book,
    Comic,
    Magazine,
    Unknown
}

public sealed record CoverCandidate(
    string Reference,
    CoverReferenceKind Kind,
    string? Label = null);

public enum CoverReferenceKind
{
    SelectedLocalFile,
    RemoteUrl,
    EmbeddedData
}

public sealed record SourceProvenance(
    string Provider,
    string? ExternalId,
    DateTimeOffset RetrievedAt,
    string? SourceUrl = null);

