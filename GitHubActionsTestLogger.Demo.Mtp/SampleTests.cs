using System;
using Xunit;

namespace GitHubActionsTestLogger.Demo.Mtp;

public class SampleTests
{
    [Fact]
    public void Test1() => Assert.True(true);

    [Fact]
    public void Test2() => throw new InvalidOperationException();

    [Fact]
    public void Test3() => Assert.True(false);
}
