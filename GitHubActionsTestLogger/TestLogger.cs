using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger
{
    [FriendlyName("GitHubActions")]
    [ExtensionUri("logger://tyrrrz/ghactions/v1")]
    public class TestLogger : ITestLogger
    {
        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            events.TestResult += (sender, args) =>
            {
                if (args.Result.Outcome <= TestOutcome.Passed)
                    return;

                var lastStackFrame = !string.IsNullOrWhiteSpace(args.Result.ErrorStackTrace)
                    ? StackTraceParser.Parse(args.Result.ErrorStackTrace).LastOrDefault()
                    : null;

                var filePath = !string.IsNullOrWhiteSpace(args.Result.TestCase.CodeFilePath)
                    ? args.Result.TestCase.CodeFilePath
                    : lastStackFrame?.FilePath;

                var line = args.Result.TestCase.LineNumber > 0
                    ? args.Result.TestCase.LineNumber
                    : lastStackFrame?.Line;

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