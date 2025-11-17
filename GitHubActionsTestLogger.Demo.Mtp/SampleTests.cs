using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitHubActionsTestLogger.Demo.Mtp;

[TestClass]
public class SampleTests
{
    [TestMethod]
    public void Test1() => Assert.IsEmpty("");

    [TestMethod]
    public void Test2() => throw new InvalidOperationException();

    [TestMethod]
    public void Test3() => Assert.Fail();
}
