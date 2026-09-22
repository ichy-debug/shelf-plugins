# Contributing

Thank you for helping make Shelf useful to more collectors and data providers.

## Principles

- Plugins return candidates; Shelf owns persistence and user confirmation.
- Preserve source provenance for every imported or retrieved value.
- Request the smallest possible permission set.
- Never collect credentials that are not required by the plugin.
- Do not redistribute cover art or provider data without the necessary rights.
- Prefer streaming imports so large collections do not need to fit in memory.

## Development

1. Install the .NET 8 SDK.
2. Fork this repository.
3. Build with `dotnet build Shelf.Plugins.sln`.
4. Add tests for mapping, malformed input, cancellation, and large exports.
5. Open a focused pull request describing the source format and permissions.

The SDK is pre-1.0. Discuss substantial contract changes in an issue before
building against assumptions that are not documented here.

