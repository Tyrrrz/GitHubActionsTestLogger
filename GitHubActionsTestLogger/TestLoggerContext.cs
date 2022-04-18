using System.Collections.Generic;
using System.IO;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

public class TestLoggerContext
{
    private readonly object _lock = new();
    private readonly GitHubWorkflow _github;
    private readonly List<TestResult> _handledTestResults = new();

    public TextWriter Output { get; }

    public TestLoggerOptions Options { get; }

    public TestLoggerContext(TextWriter output, TestLoggerOptions options)
    {
        Output = output;
        Options = options;

        _github = new GitHubWorkflow(output);
    }

    public void HandleTestResult(TestResult testResult)
    {
        lock (_lock)
        {
            _handledTestResults.Add(testResult);

            // Only report tests that have not passed
            if (testResult.Outcome > TestOutcome.Passed)
            {
                var title = testResult.TestCase.DisplayName;
                var message = Options.MessageFormat.Apply(testResult);
                var filePath = testResult.TryGetSourceFilePath();
                var line = testResult.TryGetSourceLine();

                if (testResult.Outcome == TestOutcome.Failed)
                {
                    _github.ReportError(title, message, filePath, line);
                }
                else if (Options.ReportWarnings)
                {
                    _github.ReportWarning(title, message, filePath, line);
                }
            }
        }
    }

    public void HandleTestRunCompletion()
    {
        lock (_lock)
        {
            _github.ReportSummary(
                TestSummary.Generate(_handledTestResults)
            );
        }
    }
}