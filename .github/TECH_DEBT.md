# CI tech debt

## `azure-functions-core-tools` pinned to 4.12.1 in `workflows/test.yml`

`npm install -g azure-functions-core-tools@4` resolves to the latest npm-published version
(4.13.2 at time of writing), but that version's binary zip 404s from Microsoft's CDN
(`cdn.functions.azure.com`) — confirmed broken for 4.13.0/4.13.1/4.13.2 alike, blocking the
entire merge queue (`e2e-api-tests` fails at the "Install Azure Functions Core Tools" step
before any test runs). Pinned to 4.12.1, the newest version confirmed to actually download.

Un-pin (back to floating `@4`, or bump the pin) once a newer published version is confirmed to
download successfully from the CDN.
