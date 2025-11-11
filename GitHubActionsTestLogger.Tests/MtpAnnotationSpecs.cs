using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.VsTest;
using Microsoft.Testing.Platform.Builder;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class MtpAnnotationSpecs
{
    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_for_failed_tests()
    {
        // Arrange
        var builder = await TestApplication.CreateBuilderAsync([]);

        await using var commandWriter = new StringWriter();

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);
        var app = await builder.BuildAsync();

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build(),
            new VsTestResultBuilder()
                .SetDisplayName("Test2")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new VsTestResultBuilder()
                .SetDisplayName("Test3")
                .SetOutcome(TestOutcome.Skipped)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage");

        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");

        testOutput.WriteLine(output);
    }
}
