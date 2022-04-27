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

    public string Apply(TestResult testResult)
    {
        var buffer = new StringBuilder(Template);

        // Name token
        buffer.Replace(NameToken, testResult.TestCase.DisplayName);

        // Outcome token
        buffer.Replace(OutcomeToken, !string.IsNullOrWhiteSpace(testResult.ErrorMessage)
            ? testResult.ErrorMessage
            : testResult.Outcome.ToString()
        );

        // Traits tokens
        foreach (var trait in testResult.Traits.Concat(testResult.TestCase.Traits).Distinct())
        {
            var traitToken = $"{TraitsToken}.{trait.Name}";

            buffer.Replace(traitToken, trait.Value);
        }

        return buffer.ToString();
    }
}

public partial class TestResultMessageFormat
{
    public static TestResultMessageFormat Default { get; } = new($"{OutcomeToken}");
}