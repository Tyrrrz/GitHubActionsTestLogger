using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger
{
    public partial class TestResultMessageFormat
    {
        public string Template { get; }

        public TestResultMessageFormat(string template) => Template = template;

        public string Apply(TestResult testResult)
        {
            var buffer = new StringBuilder(Template);

            var outcome = !string.IsNullOrWhiteSpace(testResult.ErrorMessage)
                ? testResult.ErrorMessage
                : testResult.Outcome.ToString();

            buffer
                .Replace(NameToken, testResult.TestCase.DisplayName)
                .Replace(OutcomeToken, outcome);

            return buffer.ToString();
        }
    }

    public partial class TestResultMessageFormat
    {
        private const string NameToken = "$test";
        private const string OutcomeToken = "$outcome";

        public static TestResultMessageFormat Default { get; } = new($"{NameToken}: {OutcomeToken}");
    }
}