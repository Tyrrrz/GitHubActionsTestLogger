using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger
{
    internal class VSTestTestRunComplete : ITestRunComplete
    {
        public long? PassedTests { get; set; }

        public long? FailedTests { get; set; }

        public long? SkippedTests { get; set; }

        public long? ExecutedTests { get; set; }

        public TimeSpan ElapsedTimeInRunningTests { get; set; }

        internal static ITestRunComplete Convert(TestRunCompleteEventArgs args)
        {
            return new VSTestTestRunComplete
            {
                PassedTests = args
                    ?.TestRunStatistics
                    ?[Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed],
                FailedTests = args
                    ?.TestRunStatistics
                    ?[Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed],
                SkippedTests = args
                    ?.TestRunStatistics
                    ?[Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped],
                ExecutedTests = args?.TestRunStatistics?.ExecutedTests,
                ElapsedTimeInRunningTests = args?.ElapsedTimeInRunningTests ?? TimeSpan.Zero,
            };
        }
    }
}
