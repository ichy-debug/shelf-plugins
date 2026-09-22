# Building your first plugin

The current SDK is experimental and uses .NET 8.

## 1. Create a project

Create a class library and reference `Shelf.Plugin.Abstractions` while the SDK
is still source-based:

```xml
<ItemGroup>
  <ProjectReference Include="../../src/Shelf.Plugin.Abstractions/Shelf.Plugin.Abstractions.csproj" />
</ItemGroup>
```

The abstractions are intended to become a versioned package before the first
supported third-party plugin release.

## 2. Implement a contract

An import plugin implements `IImportPlugin`. Keep `CanImport` fast, return a
small representative preview, and stream complete results with
`IAsyncEnumerable<ShelfItemCandidate>`.

```csharp
public sealed class MyImporter : IImportPlugin
{
    public PluginManifest Manifest { get; } = new(
        "com.example.my-importer",
        "My Importer",
        "0.1.0",
        "0.1",
        "Imports my collection format.",
        [PluginCapability.Import],
        [PluginPermission.ReadSelectedFiles]);

    // Implement CanImport, PreviewAsync, and ImportAsync.
}
```

## 3. Add a manifest

Copy `samples/Shelf.Plugin.ClzExport/plugin.json`, change its identity and
capabilities, and validate it against `schemas/plugin-manifest.schema.json`.

Use a reverse-domain ID that you control. Declare only permissions the plugin
actually needs.

## 4. Preserve source meaning

- Do not invent missing metadata.
- Keep unknown fields in `AdditionalFields` where practical.
- Preserve provider identifiers and provenance.
- Treat covers as references unless the source grants storage rights.
- Label asking prices, sold prices, retail prices, and estimates differently.

## 5. Build and test

```bash
dotnet build Shelf.Plugins.sln
```

Test malformed input, empty exports, duplicates, cancellation, unusual Unicode,
large collections, missing covers, and date/number formats from other locales.

