﻿using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger.Tests.Fakes
{
    public class FakeTestLoggerEvents : TestLoggerEvents
    {
        public override event EventHandler<TestRunMessageEventArgs>? TestRunMessage;
        public override event EventHandler<TestRunStartEventArgs>? TestRunStart;
        public override event EventHandler<TestResultEventArgs>? TestResult;
        public override event EventHandler<TestRunCompleteEventArgs>? TestRunComplete;
        public override event EventHandler<DiscoveryStartEventArgs>? DiscoveryStart;
        public override event EventHandler<TestRunMessageEventArgs>? DiscoveryMessage;
        public override event EventHandler<DiscoveredTestsEventArgs>? DiscoveredTests;
        public override event EventHandler<DiscoveryCompleteEventArgs>? DiscoveryComplete;

        public void TriggerTestResult(TestResult result) =>
            TestResult?.Invoke(this, new TestResultEventArgs(result));
    }
}