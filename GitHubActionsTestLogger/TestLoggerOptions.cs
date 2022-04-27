using System.Collections.Generic;

namespace GitHubActionsTestLogger;

public partial class TestLoggerOptions
{
    public string AnnotationTitleFormat { get; init; } = $"{TestResultFormat.NameToken}";

    public string AnnotationMessageFormat { get; init; } = $"{TestResultFormat.ErrorMessageToken}";
}

public partial class TestLoggerOptions
{
    public static TestLoggerOptions Default { get; } = new();

    public static TestLoggerOptions Resolve(IReadOnlyDictionary<string, string> parameters) => new()
    {
        AnnotationTitleFormat =
            parameters.GetValueOrDefault("annotations.titleFormat") ??
            Default.AnnotationTitleFormat,

        AnnotationMessageFormat =
            parameters.GetValueOrDefault("annotations.messageFormat") ??
            Default.AnnotationMessageFormat
    };
}