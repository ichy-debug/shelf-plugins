# Installing plugins in Shelf

Shelf plugins extend import, metadata, cover, and pricing capabilities without changing Shelf Server itself.

## Before you install

Only install a plugin from a publisher you trust. A plugin package can contain executable code for a future isolated Shelf plugin worker. Review:

- the publisher and download location;
- the plugin ID and version;
- the requested capabilities;
- the requested permissions;
- the source code or signature when available.

A customer-owned API key does not override a data provider's license or terms.

## Install through Shelf

1. Open **Shelf → Settings**.
2. Scroll to **Plugins**.
3. Download the plugin's `.shelf-plugin` or `.zip` package.
4. Select **Install plugin** and choose the downloaded package.
5. Shelf checks the package structure, manifest, API compatibility, permissions, duplicate IDs, unsafe paths, and size limits.
6. Review the installed plugin in the list.

Installation does not automatically run the plugin, access your collection, or grant credentials. Shelf keeps database writes, matching, previews, conflict handling, and final confirmation under its own control.

A plugin marked **Ready** passed structural validation. This does not mean Shelf endorses the publisher or guarantees the external service.

A plugin marked **Needs attention** contains an invalid or incompatible manifest. Its validation messages explain what must be corrected.

## Package format

A plugin package is a ZIP archive with `plugin.json` at its root. It may use either the `.zip` or `.shelf-plugin` extension.

```text
my-plugin.shelf-plugin
├── plugin.json
├── My.Plugin.dll
└── assets/
    └── icon.svg
```

The manifest must conform to [the public schema](../schemas/plugin-manifest.schema.json).

Shelf currently accepts packages up to 100 MB and at most 250 MB after expansion. Packages containing path traversal entries or symbolic links are rejected.

## Permissions

Plugins declare only the permissions they need:

| Permission | Meaning |
|---|---|
| `read-selected-files` | Read files explicitly selected by the user |
| `read-selected-folders` | Read a folder explicitly selected by the user |
| `network-access` | Contact external services |
| `credential-access` | Use credentials assigned specifically to that plugin |

Plugins do not receive direct Shelf database access.

## Current status

The plugin API is experimental (`0.x`). Shelf can install, discover, and validate packages. Shelf 0.6.22 can run previews with the official CLZ reference plugin in a separate worker process. Community plugin execution remains disabled; Shelf does not load arbitrary community assemblies into the main server process.

Package upgrades, uninstall controls, publisher signatures, and a plugin directory are planned follow-ups.
