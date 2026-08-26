using FeedCombiner.Core;

namespace FeedCombiner.Tests;

/// <summary>
/// These tests are the point of the rewrite. None of them could exist against the
/// original, where the same logic lived inside a WinForms drag-drop handler.
/// </summary>
public class FeedCombinerServiceTests
{
    private static FeedCombinerService NewService()
    {
        var detector = new MarketplaceDetector();
        return new FeedCombinerService(detector, new OutputNameBuilder(detector));
    }

    private static FeedFile File(string name, params string[] lines) =>
        new(name, lines);

    [Fact]
    public void Combines_files_in_order()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "one", "two"),
            File("b.txt", "three")
        }, new CombineOptions { RemoveDuplicates = false });

        Assert.Equal(new[] { "one", "two", "three" }, result.Lines);
        Assert.Equal(2, result.FilesProcessed);
        Assert.Equal(3, result.LinesRead);
    }

    [Fact]
    public void Removes_duplicate_lines_across_files()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "keep", "dupe"),
            File("b.txt", "dupe", "also")
        });

        Assert.Equal(new[] { "keep", "dupe", "also" }, result.Lines);
        Assert.Equal(1, result.DuplicatesRemoved);
        Assert.Equal(4, result.LinesRead);
        Assert.Equal(3, result.LinesWritten);
    }

    [Fact]
    public void Keeps_duplicates_when_asked_to()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "same"),
            File("b.txt", "same")
        }, new CombineOptions { RemoveDuplicates = false });

        Assert.Equal(2, result.LinesWritten);
        Assert.Equal(0, result.DuplicatesRemoved);
    }

    [Fact]
    public void Keeps_only_the_first_header_when_header_option_is_on()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "sku\ttitle", "A\tWidget"),
            File("b.txt", "sku\ttitle", "B\tGadget")
        }, new CombineOptions { FirstLineIsHeader = true });

        Assert.Equal(new[] { "sku\ttitle", "A\tWidget", "B\tGadget" }, result.Lines);
        Assert.Equal(1, result.DuplicatesRemoved);
    }

    [Fact]
    public void Header_row_survives_even_when_it_looks_like_a_duplicate_of_data()
    {
        // A header is kept because it is the first line, not because it is unique.
        var result = NewService().Combine(new[]
        {
            File("a.txt", "repeat", "repeat")
        }, new CombineOptions { FirstLineIsHeader = true });

        Assert.Equal(new[] { "repeat" }, result.Lines);
    }

    [Fact]
    public void Skips_blank_lines_and_counts_them()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "one", "", "   ", "two")
        });

        Assert.Equal(new[] { "one", "two" }, result.Lines);
        Assert.Equal(2, result.BlanksSkipped);
    }

    [Fact]
    public void Trailing_whitespace_does_not_create_a_false_unique_line()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "value", "value   ")
        });

        Assert.Single(result.Lines);
        Assert.Equal(1, result.DuplicatesRemoved);
    }

    [Fact]
    public void Empty_input_is_rejected_rather_than_silently_producing_a_file()
    {
        Assert.Throws<ArgumentException>(() => NewService().Combine(Array.Empty<FeedFile>()));
    }

    [Fact]
    public void Reports_every_line_as_a_duplicate_when_all_files_match()
    {
        var result = NewService().Combine(new[]
        {
            File("a.txt", "x"),
            File("b.txt", "x"),
            File("c.txt", "x")
        });

        Assert.Single(result.Lines);
        Assert.Equal(2, result.DuplicatesRemoved);
        Assert.Equal(3, result.FilesProcessed);
    }
}
