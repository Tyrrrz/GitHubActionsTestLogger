using System.Collections.Generic;
using System.IO;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

public class TestLoggerContext
{
    private readonly object _lock = new();
    private readonly GitHubWorkflow _github;
    private readonly List<string> _testSources = new();
    private readonly List<TestResult> _testResults = new();

    public TextWriter Output { get; }

    public TestLoggerOptions Options { get; }

    public TestLoggerContext(TextWriter output, TestLoggerOptions options)
    {
        Output = output;
        Options = options;

        _github = new GitHubWorkflow(output, Options.SummaryFilePath);
    }

    public void HandleTestRunStart(TestRunCriteria testRun)
    {
        lock (_lock)
        {
            _testSources.AddRange(testRun.Sources);
        }
    }

    public void HandleTestResult(TestResult testResult)
    {
        lock (_lock)
        {
            _testResults.Add(testResult);

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

    public void HandleTestRunComplete()
    {
        lock (_lock)
        {
            _github.ReportSummary(
                TestSummary.Generate(_testSources, _testResults)
            );
        }
    }
}