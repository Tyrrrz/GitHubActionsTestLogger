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
                if (args.Result.Outcome == TestOutcome.Failed)
                {
                    var frame = StackTraceParser.Parse(args.Result.ErrorStackTrace).LastOrDefault();

                    GitHubActions.WriteError(
                        $"[FAIL] {args.Result.DisplayName}: {args.Result.ErrorMessage}",
                        frame?.FilePath,
                        frame?.Line);
                }
                else if (args.Result.Outcome == TestOutcome.Skipped)
                {
                    var frame = StackTraceParser.Parse(args.Result.ErrorStackTrace).LastOrDefault();

                    GitHubActions.WriteWarning(
                        $"[SKIP] {args.Result.DisplayName}",
                        frame?.FilePath,
                        frame?.Line);
                }
                else if (args.Result.Outcome == TestOutcome.NotFound)
                {
                    var frame = StackTraceParser.Parse(args.Result.ErrorStackTrace).LastOrDefault();

                    GitHubActions.WriteWarning(
                        $"[MISS] {args.Result.DisplayName}",
                        frame?.FilePath,
                        frame?.Line);
                }
            };
        }
    }
}