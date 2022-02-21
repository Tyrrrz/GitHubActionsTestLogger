using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

public partial class TestResultMessageFormat
{
    private const string NameToken = "$test";
    private const string OutcomeToken = "$outcome";
    private const string TraitsToken = "$traits";

    public string Template { get; }

    public TestResultMessageFormat(string template) => Template = template;

    private static void ReplaceNameTokens(StringBuilder buffer, TestResult testResult)
    {
        buffer.Replace(NameToken, testResult.TestCase.DisplayName);
    }

    private static void ReplaceOutcomeTokens(StringBuilder buffer, TestResult testResult)
    {
        var outcome = !string.IsNullOrWhiteSpace(testResult.ErrorMessage)
            ? testResult.ErrorMessage
            : testResult.Outcome.ToString();

        buffer.Replace(OutcomeToken, outcome);
    }

    private static void ReplaceTraitsTokens(StringBuilder buffer, TestResult testResult)
    {
        var traits = testResult.Traits.Concat(testResult.TestCase.Traits).Distinct();

        foreach (var trait in traits)
        {
            var traitToken = $"{TraitsToken}.{trait.Name}";

            buffer.Replace(traitToken, trait.Value);
        }
    }

    public string Apply(TestResult testResult)
    {
        var buffer = new StringBuilder(Template);

        ReplaceNameTokens(buffer, testResult);
        ReplaceOutcomeTokens(buffer, testResult);
        ReplaceTraitsTokens(buffer, testResult);

        return buffer.ToString();
    }
}

public partial class TestResultMessageFormat
{
    public static TestResultMessageFormat Default { get; } = new($"{OutcomeToken}");
}