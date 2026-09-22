using System.Text.Json;
using Shelf.Plugin.Abstractions;
using Shelf.Plugin.ClzExport;

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
try
{
    var request = await JsonSerializer.DeserializeAsync<WorkerRequest>(Console.OpenStandardInput(), options)
        ?? throw new InvalidDataException("The worker request is empty.");
    if (request.Protocol != "shelf-import-v1")
        throw new InvalidDataException("Unsupported worker protocol.");
    if (request.Operation != "preview")
        throw new InvalidDataException("This worker currently supports preview only.");
    if (string.IsNullOrWhiteSpace(request.SourcePath) || !File.Exists(request.SourcePath))
        throw new FileNotFoundException("The selected import file is unavailable.");

    var plugin = new ClzExportPlugin();
    await using var stream = File.OpenRead(request.SourcePath);
    var source = new ImportSource(
        request.SourceFileName ?? Path.GetFileName(request.SourcePath),
        request.SourceFileName ?? Path.GetFileName(request.SourcePath),
        request.MediaType);
    if (!plugin.CanImport(source))
        throw new InvalidDataException("The CLZ reference plugin currently accepts XML exports.");

    var preview = await plugin.PreviewAsync(new ImportRequest(source, stream));
    await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), new WorkerResponse(true, preview, null), options);
}
catch (Exception error)
{
    await JsonSerializer.SerializeAsync(
        Console.OpenStandardOutput(),
        new WorkerResponse(false, null, error.Message),
        options);
    Environment.ExitCode = 1;
}

internal sealed record WorkerRequest(
    string Protocol,
    string Operation,
    string SourcePath,
    string? SourceFileName,
    string? MediaType);

internal sealed record WorkerResponse(
    bool Success,
    ImportPreview? Preview,
    string? Error);
