namespace GitHubActionsTestLogger.Templates;

// Workaround for:
// https://github.com/ltrzesniewski/RazorBlade/issues/10
internal partial class TestSummaryTemplate
{
    public TestSummaryTemplate(TestSummaryContext context)
        : base(context)
    {
    }
}