using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger.Tests.Utils;

internal class TestResultBuilder
{
    private string _fullyQualifiedName = "FakeTests.FakeTest";
    private string _displayName = "FakeTest";
    private LoggerTestOutcome _outcome;
    private string? _errorMessage;
    private string? _errorStackTrace;
    private string? _sourceFilePath;
    private int? _sourceFileLine;
    private readonly Dictionary<string, string> _traits = new();

    public TestResultBuilder SetDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public TestResultBuilder SetFullyQualifiedName(string fullyQualifiedName)
    {
        _fullyQualifiedName = fullyQualifiedName;
        return this;
    }

    public TestResultBuilder SetTrait(string name, string value)
    {
        _traits.Add(name, value);
        return this;
    }

    public TestResultBuilder SetOutcome(LoggerTestOutcome outcome)
    {
        _outcome = outcome;
        return this;
    }

    public TestResultBuilder SetErrorMessage(string message)
    {
        _errorMessage = message;
        return this;
    }

    public TestResultBuilder SetErrorStackTrace(string stackTrace)
    {
        _errorStackTrace = stackTrace;
        return this;
    }

    public TestResultBuilder SetSourceFilePath(string sourceFilePath)
    {
        // Not used, because this path is untested.
        _sourceFilePath = sourceFilePath;
        return this;
    }

    public TestResultBuilder SetSourceFileLine(int sourceFileLine)
    {
        // Not used, because this path is untested.
        _sourceFileLine = sourceFileLine;
        return this;
    }

    public LoggerTestResult Build()
    {
        var _testResult = new TestResult(new TestCase());

        _testResult.ErrorMessage = _errorMessage;
        _testResult.ErrorStackTrace = _errorStackTrace;

        var a = _testResult;

        return new LoggerTestResult(
            _displayName,
            _fullyQualifiedName,
            _fullyQualifiedName,
            _traits,
            _sourceFilePath,
            _sourceFileLine,
            _outcome,
            _errorMessage,
            _errorStackTrace
        );
    }
}
