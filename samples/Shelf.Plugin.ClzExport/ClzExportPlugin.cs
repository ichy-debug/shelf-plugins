using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Shelf.Plugin.Abstractions;

namespace Shelf.Plugin.ClzExport;

public sealed class ClzExportPlugin : IImportPlugin
{
    public PluginManifest Manifest { get; } = new(
        Id: "com.shelf.reference.clz-export",
        Name: "CLZ Export Importer",
        Version: "0.1.0",
        ShelfApiVersion: "0.1",
        Description: "Imports a collection from a user-provided CLZ XML export.",
        Capabilities: [PluginCapability.Import, PluginCapability.Metadata, PluginCapability.Covers],
        Permissions: [PluginPermission.ReadSelectedFiles]);

    public bool CanImport(ImportSource source) =>
        string.Equals(Path.GetExtension(source.FileName), ".xml", StringComparison.OrdinalIgnoreCase);

    public async Task<ImportPreview> PreviewAsync(
        ImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var candidates = new List<ShelfItemCandidate>();
        var messages = new List<PluginMessage>();

        await foreach (var candidate in ImportAsync(request, cancellationToken))
        {
            if (candidates.Count < 10)
            {
                candidates.Add(candidate);
            }
        }

        if (candidates.Count == 0)
        {
            messages.Add(new PluginMessage(
                PluginMessageSeverity.Warning,
                "CLZ_NO_ITEMS",
                "No recognizable collection items were found in the selected XML file."));
        }

        Rewind(request.Content);
        var document = await XDocument.LoadAsync(request.Content, LoadOptions.None, cancellationToken);
        var count = FindItems(document).Count();
        Rewind(request.Content);

        return new ImportPreview(count, candidates, messages);
    }

    public async IAsyncEnumerable<ShelfItemCandidate> ImportAsync(
        ImportRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Rewind(request.Content);
        var document = await XDocument.LoadAsync(request.Content, LoadOptions.None, cancellationToken);

        var index = 0;
        foreach (var element in FindItems(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            index++;

            var title = Value(element, "title", "name");
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var id = Value(element, "id", "index", "collectionid") ?? index.ToString(CultureInfo.InvariantCulture);
            var series = Value(element, "series");
            var number = Value(element, "issue", "issuenumber", "number");
            var type = ResolveMediaType(request.Source.MediaType, number, Value(element, "format", "type"));

            yield return new ShelfItemCandidate
            {
                SourceItemId = id,
                MediaType = type,
                Title = title,
                Series = series,
                Number = number,
                Isbn = Value(element, "isbn", "isbn13"),
                Barcode = Value(element, "barcode", "upc"),
                Publisher = Value(element, "publisher"),
                PublicationDate = Date(Value(element, "publicationdate", "releasedate", "date")),
                Description = Value(element, "description", "plot", "notes"),
                Language = Value(element, "language"),
                Condition = Value(element, "condition", "grade"),
                EstimatedValue = Decimal(Value(element, "value", "estimatedvalue")),
                Currency = Value(element, "currency"),
                Location = Value(element, "location", "storagedevice", "shelf"),
                Creators = Values(element, "creator", "author", "writer", "artist"),
                Covers = Covers(element),
                Provenance = new SourceProvenance(
                    "CLZ export",
                    id,
                    DateTimeOffset.UtcNow),
                AdditionalFields = PreserveUnmappedFields(element)
            };
        }
    }

    private static IEnumerable<XElement> FindItems(XDocument document) =>
        document.Descendants().Where(element =>
            element.Name.LocalName.Equals("book", StringComparison.OrdinalIgnoreCase) ||
            element.Name.LocalName.Equals("comic", StringComparison.OrdinalIgnoreCase) ||
            element.Name.LocalName.Equals("item", StringComparison.OrdinalIgnoreCase));

    private static string? Value(XElement element, params string[] names) =>
        element.DescendantsAndSelf()
            .FirstOrDefault(candidate => names.Contains(candidate.Name.LocalName, StringComparer.OrdinalIgnoreCase))
            ?.Value.Trim() is { Length: > 0 } value ? value : null;

    private static IReadOnlyList<string> Values(XElement element, params string[] names) =>
        element.Descendants()
            .Where(candidate => names.Contains(candidate.Name.LocalName, StringComparer.OrdinalIgnoreCase))
            .Select(candidate => candidate.Value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static ShelfMediaType ResolveMediaType(string? requested, string? issueNumber, string? format)
    {
        var value = requested ?? format;
        if (Enum.TryParse<ShelfMediaType>(value, true, out var parsed))
        {
            return parsed;
        }

        return string.IsNullOrWhiteSpace(issueNumber) ? ShelfMediaType.Book : ShelfMediaType.Comic;
    }

    private static DateOnly? Date(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? result
            : null;

    private static decimal? Decimal(string? value) =>
        decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    private static IReadOnlyList<CoverCandidate> Covers(XElement element)
    {
        var reference = Value(element, "cover", "coverfile", "image");
        return reference is null
            ? []
            : [new CoverCandidate(reference, CoverReferenceKind.SelectedLocalFile, "Exported cover")];
    }

    private static IReadOnlyDictionary<string, string> PreserveUnmappedFields(XElement element)
    {
        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "index", "collectionid", "title", "name", "series", "issue", "issuenumber", "number",
            "isbn", "isbn13", "barcode", "upc", "publisher", "publicationdate", "releasedate", "date",
            "description", "plot", "notes", "language", "condition", "grade", "value", "estimatedvalue",
            "currency", "location", "storagedevice", "shelf", "creator", "author", "writer", "artist",
            "cover", "coverfile", "image", "format", "type"
        };

        return element.Elements()
            .Where(child => !child.HasElements && !known.Contains(child.Name.LocalName))
            .GroupBy(child => child.Name.LocalName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => string.Join("; ", group.Select(child => child.Value.Trim())));
    }

    private static void Rewind(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new InvalidOperationException("The experimental host contract requires a seekable import stream.");
        }

        stream.Position = 0;
    }
}

