using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger
{
    [FriendlyName("GitHubActions")]
    [ExtensionUri("logger://tyrrrz/ghactions/v1")]
    public class TestLogger : ITestLogger
    {
        private (string? filePath, int? line) GetSourceLocationInfo(TestResult testResult)
        {
            if (!string.IsNullOrWhiteSpace(testResult.TestCase.CodeFilePath))
                return (testResult.TestCase.CodeFilePath, testResult.TestCase.LineNumber);

            var frame = StackTraceParser.Parse(testResult.ErrorStackTrace).LastOrDefault();
            return (frame?.FilePath, frame?.Line);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            events.TestResult += (sender, args) =>
            {
                if (args.Result.Outcome <= TestOutcome.Passed)
                    return;

                var (filePath, line) = GetSourceLocationInfo(args.Result);

                var message = args.Result.Outcome == TestOutcome.Failed
                    ? $"{args.Result.DisplayName}: {args.Result.ErrorMessage}"
                    : args.Result.DisplayName;

                var logLevel = args.Result.Outcome == TestOutcome.Failed
                    ? LogLevel.Error
                    : LogLevel.Warning;

                GitHubActions.WriteOutput(logLevel, message, filePath, line);
            };
        }
    }
}