# Troubleshooting

Problems you can actually hit with this application, what causes each one, and how to
confirm the fix.

This app has **no database and no configuration**, so the whole class of setup
problems that dominates the other portfolio apps does not exist here. If it builds, it
runs.

## Setup and startup

### Missing NuGet packages after cloning

```bash
dotnet restore
dotnet build
```

If restore fails, the .NET 10 SDK may be missing — `dotnet --version` should report
10.x.

### The app will not start — "cannot run a class library"

`FeedCombiner.Core` or `FeedCombiner.Tests` has been set as the startup project.
Neither has an entry point. Right-click **`FeedCombiner.Web`** → **Set as Startup
Project**.

### Port already in use

The launch profiles bind 5081 (http) and 7202 (https). Check for a stray `dotnet`
process, or edit `FeedCombiner.Web/Properties/launchSettings.json`.

### HTTPS certificate warnings on first run

```bash
dotnet dev-certs https --trust
```

### I am looking for the connection string and cannot find it

There isn't one. This app has no database. `appsettings.json` has no
`ConnectionStrings` section and the web project has no `UserSecretsId`. See
[configuration.md](configuration.md).

## Uploading and combining

### A file was rejected as too large

The limits are 20 files, 5 MB each, 20 MB total. They are constants in
`Combine.razor`, not settings:

```csharp
private const int MaxFiles = 20;
private const long MaxFileBytes = 5L * 1024 * 1024;
private const long MaxTotalBytes = 20L * 1024 * 1024;
```

Raising them raises how much memory one combine can consume — everything is held in
memory, nothing is written to disk.

### "Give me at least one file to combine."

`FeedCombinerService.Combine` throws `ArgumentException` on empty input, deliberately,
rather than returning an empty result. Silently producing an empty file is the
behavior that wastes an afternoon. There is a test pinning this
(`Empty_input_is_rejected_rather_than_silently_producing_a_file`).

### Duplicate rows survived the combine

Three things to check, in order:

1. **Is "Remove duplicates" on?** It defaults to on but can be switched off.
2. **Are the rows actually identical?** Dedupe is **line-based and ordinal** — a
   different column order, a different delimiter, or one differing character makes two
   rows genuinely distinct. There is no column awareness.
3. **Leading whitespace.** `TrimLineEnds` trims the **end** of a line only. `  value`
   and `value` are still two different lines.

### The header appeared more than once

Turn on **First line is a header**. Without it, the first line of each file is treated
as data — and if the headers are identical, dedupe removes the extras anyway, so you
usually only see this when the headers differ slightly between files.

### A data row identical to the header disappeared

Working as designed. The header is added to the same `seen` set the dedupe uses, so a
data row matching it exactly is treated as a duplicate. The header itself always
survives — there is a test for precisely this interaction
(`Header_row_survives_even_when_it_looks_like_a_duplicate_of_data`).

## Naming and detection

### The output is called Binder.txt instead of Binder_AMAZON.txt

No marketplace rule matched. Detection is a **case-insensitive substring match on the
first file's name only**, against `AMAZON`, `SHOPIFY`, `EBAY` and `WALMART`.

- A file named `az-feed.txt` does not match — the keyword is `AMAZON`, not `az`.
- Only the **first** file is inspected. A folder where the Amazon file happens to be
  sorted second detects nothing.

The rule list is in `MarketplaceDetector.Default()`.

### The output name looks wrong

The rule, carried over from the original app: **the first two hyphen-separated segments
of the first file's base name**, plus the binder name.

| First file | Output |
|---|---|
| `WP-2026-batch.txt` | `WP-2026-Binder.txt` |
| `AMAZON-feed-1.txt` | `AMAZON-feed-Binder_AMAZON.txt` |
| `feed.txt` | `feed-Binder.txt` (no hyphen — whole base name used) |

### The output is .txt and I expected .xls

**Deliberate, and a fixed bug.** The original named its output `.xls` while writing
tab-delimited text, so Excel warned on every open. A test guards the new behavior
(`Binder_is_a_text_file_not_an_xls`). Excel opens the `.txt` without complaint.

## Downloading

### "That download has expired. Combine the files again."

The link is older than ten minutes, or the app restarted. Results live in
`CombinedFileStore`, backed by `IMemoryCache`, and expire on purpose so nothing
accumulates in server memory.

Combine again to get a fresh link. There is no way to recover the old one — the result
was never written to disk.

### Downloads 404 intermittently after deploying to more than one instance

`CombinedFileStore` is a **singleton over in-process `IMemoryCache`**, so a result is
only known to the instance that produced it. A download request load-balanced to a
different instance sees an unknown id and returns the expired-link 404.

Sticky sessions or a shared cache would be needed. Noted in
[getting-started.md](getting-started.md#optional-future-deployment).

### The download link does nothing

It is an ordinary `<a href="/download/{id}">` — there is **no JavaScript** behind it.
If it does nothing, the id is missing from the markup, which means the combine did not
complete. Check for an error message above the button.

## Tests

### `dotnet test` finds no tests

Run it from the solution folder, not from inside a project folder. `FeedCombiner.Tests`
is the only test project.

### A naming or dedupe test fails after a change

That is the suite doing its job — several tests pin decisions rather than just
exercising code. Before "fixing" the test, check [testing.md](testing.md) for what the
test is protecting. Three of them guard deliberate behavior that looks like a bug if
you do not know the history.

## UI and runtime

### The page heading is hidden under the app bar

A `pt-*` class has been added to `MudMainContent`, overriding the padding MudBlazor
uses to clear the fixed app bar. Put spacing on the inner `MudContainer` instead.

### MudBlazor components render unstyled, or dialogs never appear

One of the required pieces is missing:

- `builder.Services.AddMudServices()` in `Program.cs`
- `@using MudBlazor` in `Components/_Imports.razor`
- `MudBlazor.min.css` and `MudBlazor.min.js` linked in `App.razor`
- `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider` and
  `MudSnackbarProvider` at the top of `MainLayout.razor`

### The result preview lost its dark panel

The `.preview` block lives in `wwwroot/app.css`, not in MudBlazor. **`app.css` is live
code in this app** — it holds `.preview` and the `#blazor-error-ui` styling, and
nothing else. Do not empty it out the way the stock template stylesheet was emptied in
the other portfolio apps.

### "Rejoining the server..." keeps appearing

The Blazor Server circuit dropped. Normal after a restart during debugging — reload. If
it happens while idle, something is interrupting the WebSocket: a proxy, a VPN, or a
firewall.

**Note:** if the circuit drops mid-combine, the uploaded content is gone — it lived in
that circuit's memory. Upload and combine again.

## Things people look for that are not here

- **Swagger / OpenAPI** — none. There is one minimal-API endpoint, documented in
  [architecture.md](architecture.md#the-http-api). `/swagger` will 404.
- **A database** — none by design. Apps 1–3 in this portfolio prove SQL; this one
  proves file handling and class design.
- **A login page** — no authentication. Every page is public.
- **Saved history of past combines** — nothing is persisted. A combine is one
  operation with a result you take away.
- **Custom JavaScript** — deliberately none. The download is a plain HTTP GET.

## Still stuck?

Work through the seven checks at the end of
[getting-started.md](getting-started.md#4-check-it-actually-works). They use two small
files you create by hand and exercise dedupe, header handling, marketplace detection,
the fallback name and the download link.
