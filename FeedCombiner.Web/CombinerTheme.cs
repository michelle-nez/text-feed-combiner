using MudBlazor;

namespace FeedCombiner.Web;

/// <summary>
/// This app's own look: deep violet, generous whitespace, soft rounded surfaces
/// and a single centred column. It is a one-job utility rather than a data
/// screen, so it gets an airier shape than the other apps in the portfolio.
/// </summary>
public static class CombinerTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6d28d9",
            Secondary = "#a78bfa",
            Tertiary = "#8b5cf6",
            Info = "#6d28d9",
            Success = "#059669",
            Warning = "#d97706",
            Error = "#dc2626",
            Background = "#faf9fd",
            Surface = "#ffffff",
            AppbarBackground = "#faf9fd",
            AppbarText = "#4c1d95",
            TextPrimary = "#2b2440",
            TextSecondary = "#6b6484",
            Divider = "#e8e4f2",
            ActionDefault = "#6b6484"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Manrope", "Segoe UI", "sans-serif"], FontSize = "0.9rem" },
            H4 = new H4Typography { FontSize = "1.9rem", FontWeight = "700", LetterSpacing = "-.02em" },
            H5 = new H5Typography { FontSize = "1.4rem", FontWeight = "700", LetterSpacing = "-.01em" },
            H6 = new H6Typography { FontSize = "1.05rem", FontWeight = "700" },
            Button = new ButtonTypography { TextTransform = "none", FontWeight = "600" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "14px"
        }
    };
}
