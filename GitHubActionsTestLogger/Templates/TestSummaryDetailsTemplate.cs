namespace GitHubActionsTestLogger.Templates;

// Workaround for:
// https://github.com/ltrzesniewski/RazorBlade/issues/10
internal partial class TestSummaryDetailsTemplate
{
    public TestSummaryDetailsTemplate(TestSummaryContext context)
        : base(context)
    {
    }
}