using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TestRunStatisticsExtensions
{
    public static long GetPassedCount(this ITestRunStatistics testRunStatistics) =>
        testRunStatistics[TestOutcome.Passed];

    public static long GetFailedCount(this ITestRunStatistics testRunStatistics) =>
        testRunStatistics[TestOutcome.Failed];

    public static long GetSkippedCount(this ITestRunStatistics testRunStatistics) =>
        testRunStatistics[TestOutcome.Skipped];
}