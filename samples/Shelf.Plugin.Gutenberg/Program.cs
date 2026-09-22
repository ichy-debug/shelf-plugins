using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

const string protocol = "shelf-provider-v1";
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
try
{
    var request = await JsonSerializer.DeserializeAsync<WorkerRequest>(Console.OpenStandardInput(), options)
        ?? throw new InvalidDataException("The worker request is empty.");
    if (request.Protocol != protocol) throw new InvalidDataException("Unsupported worker protocol.");
    if (request.Operation != "search") throw new InvalidDataException("This provider currently supports search only.");

    var query = (request.Query ?? string.Empty).Trim();
    if (query.Length > 200) throw new InvalidDataException("Keep the search query under 200 characters.");
    var page = Math.Max(1, request.Page);
    var start = (page - 1) * 25 + 1;
    var catalog = new Uri("https://www.gutenberg.org/ebooks/search.opds/");
    var url = new Uri(catalog, $"?query={Uri.EscapeDataString(query)}&start_index={start}");

    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    using var message = new HttpRequestMessage(HttpMethod.Get, url);
    message.Headers.UserAgent.ParseAdd("Shelf-Gutenberg-Plugin/0.1 (+https://github.com/ichy-debug/shelf-plugins)");
    using var response = await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    await using var stream = await response.Content.ReadAsStreamAsync();
    var document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

    XNamespace atom = "http://www.w3.org/2005/Atom";
    XNamespace openSearch = "http://a9.com/-/spec/opensearch/1.1/";
    var books = document.Root?.Elements(atom + "entry").Select(entry =>
    {
        var detail = entry.Elements(atom + "link")
            .Select(link => (string?)link.Attribute("href"))
            .FirstOrDefault(href => !string.IsNullOrWhiteSpace(href)
                && Regex.IsMatch(href, @"^/ebooks/\d+\.opds(?:\?.*)?$", RegexOptions.IgnoreCase));
        var match = Regex.Match(detail ?? string.Empty, @"/ebooks/(\d+)\.opds", RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        var id = match.Groups[1].Value;
        return new GutenbergBook(
            id,
            ((string?)entry.Element(atom + "title") ?? "Untitled").Trim(),
            ((string?)entry.Element(atom + "content") ?? string.Empty).Trim(),
            string.Empty,
            null,
            null,
            $"https://www.gutenberg.org/cache/epub/{id}/pg{id}.cover.medium.jpg",
            $"https://www.gutenberg.org/ebooks/{id}.epub3.images",
            []);
    }).Where(book => book is not null).Cast<GutenbergBook>().ToList() ?? [];

    var hasNext = document.Root?.Elements(atom + "link").Any(link =>
        string.Equals((string?)link.Attribute("rel"), "next", StringComparison.OrdinalIgnoreCase)) == true;
    var total = int.TryParse((string?)document.Root?.Element(openSearch + "totalResults"), out var parsed)
        ? parsed : start - 1 + books.Count + (hasNext ? 1 : 0);
    var result = new GutenbergSearchResponse(query, page, total, books);
    await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), new WorkerResponse(true, result, null), options);
}
catch (Exception error)
{
    await JsonSerializer.SerializeAsync(
        Console.OpenStandardOutput(),
        new WorkerResponse(false, null, error is HttpRequestException ? "Project Gutenberg could not be reached." : error.Message),
        options);
    Environment.ExitCode = 1;
}

sealed record WorkerRequest(string Protocol, string Operation, string? Query, int Page);
sealed record WorkerResponse(bool Success, GutenbergSearchResponse? Result, string? Error);
sealed record GutenbergSearchResponse(string Query, int Page, int TotalResults, IReadOnlyList<GutenbergBook> Items);
sealed record GutenbergBook(string Id, string Title, string Author, string Summary, string? Language, int? Year, string? CoverUrl, string? DownloadUrl, IReadOnlyList<string> Subjects);
