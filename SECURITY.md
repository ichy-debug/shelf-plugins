# Security policy

Do not disclose vulnerabilities in a public issue. Report them privately to the
maintainers through GitHub's private vulnerability reporting feature.

## Plugin security model

The intended Shelf host runs third-party plugins outside the Shelf Server
process. A plugin receives only explicitly granted files, settings, credentials,
and network capabilities. Plugins do not receive direct database access.

Community plugins should be treated as third-party software. Review the source,
publisher, signature, and requested permissions before installing one.

