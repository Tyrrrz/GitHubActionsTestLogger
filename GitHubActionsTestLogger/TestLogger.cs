using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger;

/// <summary>
/// VSTest test logger extension.
/// </summary>
[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v2")]
public class TestLogger : ITestLoggerWithParameters
{
    public TestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        var context = new TestLoggerContext(GitHubWorkflow.Default, options);

        events.TestRunStart += (_, args) =>
            context.HandleTestRunStart(
                VSTestConverter.ConvertTestRunCriteria(args.TestRunCriteria)
            );
        events.TestResult += (_, args) =>
            context.HandleTestResult(VSTestConverter.ConvertTestResult(args.Result));
        events.TestRunComplete += (_, args) =>
            context.HandleTestRunComplete(VSTestConverter.ConvertTestRunComplete(args));

        Context = context;
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestLoggerOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) =>
        Initialize(events, TestLoggerOptions.Resolve(parameters));
}
