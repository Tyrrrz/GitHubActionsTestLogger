using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger.Tests.Utils;

internal static class TestLoggerExtensions
{
    public static TestLoggerContext InitializeAndGetContext(
        this TestLogger logger,
        TestLoggerEvents events,
        Dictionary<string, string> options)
    {
        logger.Initialize(events, options);

        return
            logger.Context ??
            throw new InvalidOperationException("Test logger context is null.");
    }

    public static TestLoggerContext InitializeAndGetContext(
        this TestLogger logger,
        TestLoggerEvents events,
        string testRunDirectory)
    {
        logger.Initialize(events, testRunDirectory);

        return
            logger.Context ??
            throw new InvalidOperationException("Test logger context is null.");
    }
}