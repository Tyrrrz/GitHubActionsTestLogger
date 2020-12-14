using System;
using System.Collections.Generic;
using GitHubActionsTestLogger.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger
{
    // The main idea behind this logger is that it writes messages to console in a special format
    // that GitHub Actions runner recognizes as workflow commands.
    // We try to get the source information (file, line number) of the failed tests and
    // report them to GitHub Actions so it highlights them as failed checks in diff and annotations.
    // Unfortunately .NET test platform does not provide source information, so most of the logic in
    // the code revolves around parsing stack traces and using other heuristics to extract information.

    [FriendlyName("GitHubActions")]
    [ExtensionUri("logger://tyrrrz/ghactions/v1")]
    public class TestLogger : ITestLoggerWithParameters
    {
        public TestLoggerContext? Context { get; private set; }

        private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
        {
            if (!GitHubActions.IsRunningInsideWorkflow())
                Console.WriteLine("WARN: Not running inside GitHub Actions, but using GitHub Actions Test Logger.");

            Context = new TestLoggerContext(Console.Out, options);
            events.TestResult += (_, args) => Context.ProcessTestResult(args.Result);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
            Initialize(events, TestLoggerOptions.Default);

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters) =>
            Initialize(events, TestLoggerOptions.Extract(parameters));
    }
}