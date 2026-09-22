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

The SDK is currently an **experimental 0.x contract**. It defines the intended boundary and allows reference plugins to be developed in public while integration with Shelf Server is being completed. Breaking changes may occur before version 1.0.

## Repository layout

```text
src/Shelf.Plugin.Abstractions/       Shared .NET contracts
samples/Shelf.Plugin.ClzExport/      Reference collection importer
schemas/plugin-manifest.schema.json  Plugin manifest schema
docs/                                Architecture and developer guide
```

## Build

The current reference implementation requires the .NET 8 SDK.

```bash
dotnet build Shelf.Plugins.sln
```

Start with [Building your first plugin](docs/getting-started.md), then read the [architecture](docs/architecture.md).

## Licensing

The SDK, documentation, and reference plugins in this repository are licensed under the [Apache License 2.0](LICENSE). Plugins built with the SDK may use their own compatible license and do not have to be open source.

Shelf and the Shelf name are separate from this source-code license. See [NOTICE](NOTICE).

## CLZ reference importer

The sample importer demonstrates how a plugin can read a user-provided collection export, normalize common fields, report warnings, and return an import preview.

CLZ and Collectorz.com are trademarks of their respective owners. The reference importer is not affiliated with or endorsed by Collectorz.com and does not log in to or access a CLZ account.

