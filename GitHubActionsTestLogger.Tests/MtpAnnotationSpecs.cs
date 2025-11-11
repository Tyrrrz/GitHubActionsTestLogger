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
        // ..todo
    }
}
