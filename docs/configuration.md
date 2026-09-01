# Configuration

**There is nothing you have to configure to run this app.** No connection string, no
User Secrets, no API keys, no environment variables beyond the one Visual Studio sets
for you.

This document is short on purpose. It exists so the absence is documented rather than
looking like an omission, and so the values that *are* fixed in code are findable.

## appsettings.json

The committed file, in full:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Setting | Meaning |
|---|---|
| `Logging:LogLevel:Default` | `Information` — application log level |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` — quiets per-request framework noise |
| `AllowedHosts` | `*`, the framework default. Worth narrowing to real hostnames if ever deployed |

**Note what is not here: no `ConnectionStrings` section at all.** The other apps in
this portfolio carry an empty connection string key to document its shape. This one has
no database, so there is nothing to document.

`appsettings.Development.json` overrides the base file with identical logging values,
so it currently changes nothing. It is the stock template file, left in place.

## No User Secrets

`FeedCombiner.Web.csproj` has **no `UserSecretsId`**, because there is no secret to
store. There is nothing in this app that a credential would protect.

## Environments

`ASPNETCORE_ENVIRONMENT` is the only environment variable this app reads, and both
launch profiles set it to `Development`.

| | Development | Production |
|---|---|---|
| Unhandled exceptions | Developer exception page, full stack trace | `/Error` page, no detail |
| HSTS | off | on |

Everything else is registered unconditionally. There is no
`appsettings.Production.json`.

## Launch profiles

From `FeedCombiner.Web/Properties/launchSettings.json`:

| Profile | URLs | Environment |
|---|---|---|
| `http` | http://localhost:5081 | Development |
| `https` | https://localhost:7202 and http://localhost:5081 | Development |

`launchSettings.json` is a **local development file only** — not read when the app runs
outside Visual Studio or `dotnet run`.

## The settings that are in code

These are the values someone would go looking for in a config file. **All of them are
constants**, deliberately.

### Upload limits — `Combine.razor`

```csharp
private const int MaxFiles = 20;
private const long MaxFileBytes = 5L * 1024 * 1024;    // 5 MB
private const long MaxTotalBytes = 20L * 1024 * 1024;  // 20 MB
```

These bound how much memory one combine can consume. They are constants rather than
settings because raising them is a decision about the host's memory, not a per-
environment preference — see
[getting-started.md](getting-started.md#optional-future-deployment).

### Download lifetime — `CombinedFileStore.cs`

```csharp
private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);
```

How long a finished file stays available for download before its link 404s.

### Marketplace rules — `MarketplaceDetector.cs`

```csharp
public static IReadOnlyList<IMarketplaceRule> Default() => new List<IMarketplaceRule>
{
    new KeywordMarketplaceRule("Amazon", "AMAZON"),
    new KeywordMarketplaceRule("Shopify", "SHOPIFY"),
    new KeywordMarketplaceRule("eBay", "EBAY"),
    new KeywordMarketplaceRule("Walmart", "WALMART")
};
```

Adding a marketplace is one line here. **This is the part most worth moving into
configuration one day** — it is genuinely a list of values rather than a rule, and
`MarketplaceDetector` already accepts a custom list through its constructor, so the
seam exists. Listed under
[architecture.md → Recommendations](architecture.md#recommendations).

### Default combine options — `CombineOptions.cs`

| Option | Default |
|---|---|
| `RemoveDuplicates` | `true` |
| `FirstLineIsHeader` | `false` |
| `SkipBlankLines` | `true` |
| `TrimLineEnds` | `true` |

These are the defaults the form starts with; the user can change all four per combine.
They are not persisted — every visit starts from these values again.

## What is not configured

- **No database, no connection string, no `ConnectionStrings` section**
- **No authentication or authorization**
- **No external services** — no API keys, no email, no storage
- **No Swagger/OpenAPI** — the one endpoint is described in
  [architecture.md](architecture.md#the-http-api)
- **No feature flags, health checks, or CORS policy**
- **No custom logging providers** — console only

The complete list of things that must be supplied to run this app is: **nothing.**

## Recommendations

| Recommendation | Why |
|---|---|
| Move the marketplace rules into configuration | The seam already exists; it is the one list that is data rather than logic |
| Narrow `AllowedHosts` if deployed | `*` accepts any Host header |
| Make the upload limits configurable if deployed | They bound memory use, which is host-specific |
