# Getting started

The easiest setup of the four portfolio apps: **there is no database and nothing to
configure.** Clone, open, run.

## Prerequisites

| Requirement | Notes |
|---|---|
| Visual Studio 2026 | With the **ASP.NET and web development** workload |
| .NET 10 SDK | `dotnet --version` should report 10.x |

No SQL Server, no LocalDB, no connection string, no User Secrets. This app holds
everything in memory for the length of one operation.

## 1. Clone and open

```bash
git clone https://github.com/michelle-nez/text-feed-combiner.git
cd text-feed-combiner
```

Open `TextFeedCombiner.sln`. `FeedCombiner.Web` is already the startup project.

## 2. Run it

Press **F5**, or:

```bash
dotnet run --project FeedCombiner.Web
```

| Profile | URL |
|---|---|
| `http` | http://localhost:5081 |
| `https` | https://localhost:7202 (also serves 5081) |

Both set `ASPNETCORE_ENVIRONMENT=Development`. On first run over HTTPS:

```bash
dotnet dev-certs https --trust
```

That is the whole setup. Open `/combine`.

## 3. Run the tests

```bash
dotnet test
```

Expect **20 passed, 0 failed**, in well under a second. Detail in
[testing.md](testing.md).

## 4. Check it actually works

You need a couple of text files. Create them anywhere — the app never touches your
disk, it reads what you upload into memory.

Make **`AMAZON-feed-1.txt`**:

```
sku	title	price
WP-1G-BLK	1-Gang Blank Wall Plate	2.19
CBL-CAT6-25	Cat6 Patch Cable 25ft	9.40
```

And **`AMAZON-feed-2.txt`**, deliberately repeating one row:

```
sku	title	price
CBL-CAT6-25	Cat6 Patch Cable 25ft	9.40
ADP-USBC	USB-C to HDMI Adapter	18.99
```

Then:

1. Open `/combine` and upload both files.
2. Turn **First line is a header** on — both files start with the same `sku title
   price` row.
3. Click **Combine**. You should see **4 lines written**, **2 duplicates removed** —
   one being the repeated header, one the repeated Cat6 row — and the marketplace
   detected as **Amazon**.
4. The output name should be **`AMAZON-feed-Binder_AMAZON.txt`**: the first two
   hyphen-separated segments of the first file's name, plus the binder name. Note the
   **`.txt`** — the original app wrote `.xls` for tab-delimited text and Excel
   complained every time.
5. Click the download link. It is a plain `<a href>` to `/download/{id}` — no
   JavaScript involved.
6. **Rename a file to `feed-1.txt`** with no marketplace keyword and combine again.
   The output falls back to **`Binder.txt`**.
7. Wait ten minutes and click an old download link. It returns *"That download has
   expired. Combine the files again."* — the store's entries expire on purpose so
   nothing accumulates in memory.

If all seven behave, the domain, the upload path and the download endpoint are all
wired up correctly.

## Where the pieces live

| Project | What it is |
|---|---|
| `FeedCombiner.Core` | The domain — references nothing, holds all the logic |
| `FeedCombiner.Web` | Blazor Server UI, the download endpoint, the file store |
| `FeedCombiner.Tests` | 20 xUnit tests over Core |

## Current deployment state

**This application is not deployed anywhere, and has no deployment configuration.**

Verified in the repository: no publish profile (`.pubxml`), no `Dockerfile`, no
`.github/workflows`, no `appsettings.Production.json`.

## Optional future deployment

**Nothing here is implemented.**

This app would be the easiest of the four to deploy — there is no database to
provision and no connection string to supply. It still needs a real .NET host holding
a **live SignalR connection**, because it is Blazor Server; static hosts cannot run it.

Two things specific to this app would matter on a real host:

- **Memory.** Uploads and results are held in memory, and the store keeps each result
  for ten minutes. Twenty concurrent users at the 20 MB limit is 400 MB of transient
  load. The limits in `Combine.razor` are the only thing bounding this.
- **`CombinedFileStore` is a singleton over `IMemoryCache`**, so it is per-instance. On
  more than one instance, a download request routed to a different server than the one
  that produced the file returns the expired-link 404. Sticky sessions or a shared
  cache would be needed.

## Where to go next

| Document | Covers |
|---|---|
| [architecture.md](architecture.md) | The three projects, the rewrite, the download endpoint |
| [testing.md](testing.md) | What the 20 tests cover and what they do not |
| [configuration.md](configuration.md) | The (very short) list of settings |
| [troubleshooting.md](troubleshooting.md) | Problems specific to this app |
