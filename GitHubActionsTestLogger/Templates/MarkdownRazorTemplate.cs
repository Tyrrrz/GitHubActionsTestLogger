using RazorBlade;

namespace GitHubActionsTestLogger.Templates;

internal abstract class MarkdownRazorTemplate<T> : PlainTextTemplate<T>
{
    protected MarkdownRazorTemplate(T model)
        : base(model)
    {
    }

    // In order to produce HTML that's also valid Markdown, we need to
    // remove some whitespace inside literals.
    public new void WriteLiteral(string? literal)
    {
        if (!string.IsNullOrEmpty(literal))
        {
            base.WriteLiteral(
                literal
                    // Remove indentation
                    .Replace("    ", "")
                    // Remove linebreaks
                    .Replace("\r", "").Replace("\n", "")
            );
        }
        else
        {
            base.WriteLiteral(literal);
        }
    }

    // Using params here to write multiple lines as a workaround
    // for the fact that Razor does not support raw string literals.
    protected void WriteMarkdown(params string?[] lines)
    {
        // Two line breaks are required to separate markdown from HTML
        base.WriteLiteral("\n\n");

        foreach (var line in lines)
        {
            base.WriteLiteral(line);
            base.WriteLiteral("\n");
        }
    }
}