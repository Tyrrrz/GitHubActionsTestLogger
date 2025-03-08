using System;

namespace GitHubActionsTestLogger;

public record LoggerTestRunComplete(
    long? PassedTests,
    long? FailedTests,
    long? SkippedTests,
    long? ExecutedTests,
    TimeSpan ElapsedTimeInRunningTests
);
