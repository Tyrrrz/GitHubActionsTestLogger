using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v2")]
public class TestLogger : ITestLoggerWithParameters
{
    public VSTestTestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        var context = new VSTestTestLoggerContext(GitHubWorkflow.Default, options);

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

public class VSTestTestLoggerContext
{
    private readonly TestLoggerContext _testLoggerContext;

    public TestLoggerOptions Options { get; }

    public VSTestTestLoggerContext(GitHubWorkflow github, TestLoggerOptions options)
    {
        Options = options;
        _testLoggerContext = new TestLoggerContext(github, options);
    }

    public void HandleTestResult(TestResultEventArgs args)
    {
        _testLoggerContext.HandleTestResult(VSTestConverter.ConvertTestResult(args.Result));
    }

    public void HandleTestRunStart(TestRunStartEventArgs args)
    {
        _testLoggerContext.HandleTestRunStart(
            VSTestConverter.ConvertTestRunCriteria(args.TestRunCriteria)
        );
    }

    public void HandleTestRunComplete(TestRunCompleteEventArgs args)
    {
        _testLoggerContext.HandleTestRunComplete(VSTestConverter.ConvertTestRunComplete(args));
    }
}
