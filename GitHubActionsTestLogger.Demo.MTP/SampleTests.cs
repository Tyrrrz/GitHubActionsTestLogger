using System;

namespace GitHubActionsTestLogger.Demo;

[TestClass]
public class SampleTests
{
    [TestMethod]
    public void Test1() => Assert.IsTrue(true);

    [TestMethod]
    public void Test2() => throw new InvalidOperationException();

    [TestMethod]
    public void Test3() => Assert.Fail();
}
