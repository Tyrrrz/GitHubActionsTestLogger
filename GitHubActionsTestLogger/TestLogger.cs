using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v2")]
public class TestLogger : ITestLoggerWithParameters
{
    public TestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        var context = new TestLoggerContext(GitHubWorkflow.Default, options);

        events.TestRunStart += (_, args) => context.HandleTestRunStart(args);
        events.TestResult += (_, args) => context.HandleTestResult(args);
        events.TestRunComplete += (_, args) => context.HandleTestRunComplete(args);

        Context = context;
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestLoggerOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) =>
        Initialize(events, TestLoggerOptions.Resolve(parameters));
}
