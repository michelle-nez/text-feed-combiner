# Testing

`FeedCombiner.Tests` holds **20 xUnit tests** over `FeedCombiner.Core`. All 20 pass;
the suite runs in well under a second.

This document exists in place of a database doc — this app has no database, and the
test suite is the thing worth explaining instead.

## Running them

```bash
dotnet test
```

Or Test Explorer in Visual Studio. Expected output:

```
Passed!  - Failed: 0, Passed: 20, Skipped: 0, Total: 20
```

No setup, no database, no fixtures, no mocking framework. The tests construct plain
objects and assert on the result.

## Why the tests are possible at all

**This is the whole argument of the rewrite.** The original MicroCombiner put every
decision inside a WinForms drag-and-drop handler, so exercising the dedupe logic meant
launching a form and dropping real files onto it. There was no seam to test through.

`FeedCombiner.Core` references **nothing** — not ASP.NET, not Windows Forms, not
`System.IO`. `FeedFile` holds a name and a list of lines, not a path. So a test can do
this:

```csharp
var files = new[]
{
    new FeedFile("AMAZON-feed.txt", new[] { "a", "b" }),
    new FeedFile("AMAZON-feed2.txt", new[] { "b", "c" })
};

var result = service.Combine(files);

Assert.Equal(3, result.LinesWritten);
Assert.Equal(1, result.DuplicatesRemoved);
```

No disk, no browser, no cleanup. **Test count is a consequence of the design, not an
effort put in afterwards.**

## Project setup

| | |
|---|---|
| Framework | xUnit |
| Also referenced | `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector` |
| References | `FeedCombiner.Core` only — **not** `FeedCombiner.Web` |

The test project deliberately does not reference the web project. If a future test
needs to reach into `Combine.razor` or `CombinedFileStore`, that is a second reference
and a deliberate decision, not something to add casually.

## What the two files cover

### `FeedCombinerServiceTests.cs` — the combine behavior

| Test | Proves |
|---|---|
| `Combines_files_in_order` | Output preserves file and line order |
| `Removes_duplicate_lines_across_files` | Dedupe works across files, not just within one |
| `Keeps_duplicates_when_asked_to` | `RemoveDuplicates = false` is honored |
| `Keeps_only_the_first_header_when_header_option_is_on` | One header survives from the first file |
| `Header_row_survives_even_when_it_looks_like_a_duplicate_of_data` | The header is not eaten by the dedupe set |
| `Skips_blank_lines_and_counts_them` | Blanks are dropped **and** reported |
| `Trailing_whitespace_does_not_create_a_false_unique_line` | `value` and `value   ` are the same row |
| `Empty_input_is_rejected_rather_than_silently_producing_a_file` | No input throws instead of writing an empty file |
| `Reports_every_line_as_a_duplicate_when_all_files_match` | Counts are right in the degenerate case |

### `NamingTests.cs` — detection and output naming

| Test | Proves |
|---|---|
| `Detects_the_marketplace_from_the_file_name` | The rule list matches |
| `Marketplace_match_is_case_insensitive` | `amazon` and `AMAZON` both match |
| `Unknown_marketplace_falls_back_to_the_default_binder` | `Binder.txt` when nothing matches |
| `Binder_is_a_text_file_not_an_xls` | **Guards a fixed bug** — see below |
| `Output_name_keeps_the_first_two_segments_of_the_source_name` | The original's `Split('-')` behavior is preserved |
| `Output_name_uses_the_whole_base_name_when_there_is_no_hyphen` | The other branch of that rule |
| `Feed_file_requires_a_name` | `FeedFile` rejects a blank name |
| `Base_name_strips_only_the_final_extension` | `a.b.txt` → `a.b`, not `a` |

## The tests that are doing real work

Three of them are worth pointing at, because they encode decisions rather than just
exercising code.

**`Binder_is_a_text_file_not_an_xls`** guards a bug carried over from the original,
which named its output `.xls` while writing tab-delimited text — so Excel warned on
every open. The test fails if anyone "restores" the old extension.

**`Header_row_survives_even_when_it_looks_like_a_duplicate_of_data`** covers the one
genuinely subtle interaction in the combine loop. The header is added to the same
`seen` set the dedupe uses, so a data row identical to the header is dropped — but the
header itself must not be. Ordering the checks wrongly breaks this, and nothing else
would catch it.

**`Empty_input_is_rejected_rather_than_silently_producing_a_file`** pins a deliberate
choice: `Combine` throws `ArgumentException` on no input rather than returning an
empty result. Silently producing an empty file is the behavior that wastes someone's
afternoon.

## What is not tested

Stated plainly, because a test count means little without its boundaries:

- **`Combine.razor`** — the upload limits (20 files, 5 MB each, 20 MB total), the
  per-file error messages, and the preview rendering have no tests. This is the largest
  gap.
- **`CombinedFileStore`** — the put/get round trip and the ten-minute expiry are
  untested. Expiry in particular would need a clock abstraction to test properly.
- **The `/download/{id}` endpoint** — no integration test. Its 404 path was verified by
  hand against the running app.
- **`MarketplaceDetector` with a custom rule list** — the constructor accepts one, but
  only the defaults are covered.

## If you add to the domain

Two habits worth keeping:

1. **Add the test in the same commit.** The suite is fast and has no setup cost, so
   there is no friction excuse.
2. **Keep `FeedCombiner.Core` free of references.** The moment it takes a dependency on
   ASP.NET or `System.IO`, the tests need fixtures and the argument this app is making
   stops being true.
