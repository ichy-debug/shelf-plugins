# Shelf Plugins

Open plugin contracts, documentation, and reference plugins for **Shelf**, the premium collection and reading platform for books, comics, and magazines.

Shelf Server remains a commercial product. This repository makes it easy for users, data providers, and independent developers to connect their own collection files and services to Shelf.

## What plugins can do

- Import collections from applications such as CLZ or Calibre.
- Match local cover folders to physical collection items.
- Look up metadata and cover candidates.
- Retrieve retail, asking, sold, or estimated market prices.
- Add support for specialist or private data sources without changing Shelf Server.

Plugins return normalized candidates. Shelf remains responsible for validation, matching, conflict resolution, user confirmation, persistence, and rollback. Plugins never write directly to the Shelf database.

## Repository status

The SDK is currently an **experimental 0.x contract**. Breaking changes may occur before version 1.0.

## Repository layout

```text
src/Shelf.Plugin.Abstractions/       Shared .NET contracts
samples/Shelf.Plugin.ClzExport/      Reference collection importer
samples/Shelf.Plugin.Gutenberg/      Project Gutenberg search provider
schemas/plugin-manifest.schema.json  Plugin manifest schema
docs/                                Architecture and developer guide
```

## Build

The current reference implementations require the .NET 8 SDK.

```bash
dotnet build Shelf.Plugins.sln
```

Users can follow [Installing plugins in Shelf](docs/installing-plugins.md). Developers should start with [Building your first plugin](docs/getting-started.md), then read the [architecture](docs/architecture.md).

## Licensing

The SDK, documentation, and reference plugins in this repository are licensed under the [Apache License 2.0](LICENSE). Plugins built with the SDK may use their own compatible license and do not have to be open source.

Shelf and the Shelf name are separate from this source-code license. See [NOTICE](NOTICE).

## Official examples

The CLZ importer reads a user-provided export and returns an import preview. CLZ and Collectorz.com are trademarks of their respective owners; this project is not affiliated with or endorsed by Collectorz.com.

The Project Gutenberg provider searches Gutenberg's public OPDS catalog. It does not scrape pages or bypass access controls. Shelf remains responsible for downloading and validating the selected EPUB.
