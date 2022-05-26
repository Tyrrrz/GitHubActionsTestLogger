using System.Collections.Generic;

namespace GitHubActionsTestLogger;

public partial class TestLoggerOptions
{
    public string AnnotationTitleFormat { get; init; } = "$test";

    public string AnnotationMessageFormat { get; init; } = "$error";
}

public partial class TestLoggerOptions
{
    public static TestLoggerOptions Default { get; } = new();

    public static TestLoggerOptions Resolve(IReadOnlyDictionary<string, string?> parameters) => new()
    {
        AnnotationTitleFormat =
            parameters.GetValueOrDefault("annotations.titleFormat") ??
            Default.AnnotationTitleFormat,

        AnnotationMessageFormat =
            parameters.GetValueOrDefault("annotations.messageFormat") ??
            Default.AnnotationMessageFormat
    };
}