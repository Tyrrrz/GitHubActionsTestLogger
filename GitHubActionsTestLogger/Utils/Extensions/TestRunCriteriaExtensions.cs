using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TestRunCriteriaExtensions
{
    public static string? TryGetTargetFramework(this TestRunCriteria testRunCriteria)
    {
        if (string.IsNullOrWhiteSpace(testRunCriteria.TestRunSettings))
            return null;

        return (string?)XElement
            .Parse(testRunCriteria.TestRunSettings)
            .Element("RunConfiguration")?
            .Element("TargetFrameworkVersion");
    }
}