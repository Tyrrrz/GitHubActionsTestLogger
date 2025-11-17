using System.Collections.Generic;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger.Tests.Mtp;

internal static class FakeTestFrameworkExtensions
{
    public static ITestApplicationBuilder RegisterFakeTests(
        this ITestApplicationBuilder builder,
        params IReadOnlyList<TestNode> testNodes
    ) =>
        builder.RegisterTestFramework(
            _ => new TestFrameworkCapabilities(),
            (_, _) => new FakeTestFramework(testNodes)
        );
}
