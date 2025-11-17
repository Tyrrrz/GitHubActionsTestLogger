using System;
using System.Collections.Generic;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger.Tests.Mtp;

internal class TestNodeBuilder
{
    private string _displayName = "FakeTest";
    private string _assemblyName = "FakeTests.dll";
    private string _namespace = "FakeTests";
    private string _typeName = "FakeTestClass";
    private string _methodName = "FakeTestMethod";
    private string? _sourceFilePath;
    private int? _sourceLineNumber;
    private readonly Dictionary<string, string> _traits = new(StringComparer.Ordinal);
    private TestOutcome _testOutcome;
    private string? _errorMessage;
    private string? _errorStackTrace;

    public TestNodeBuilder SetDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public TestNodeBuilder SetAssemblyName(string assemblyName)
    {
        _assemblyName = assemblyName;
        return this;
    }

    public TestNodeBuilder SetNamespace(string @namespace)
    {
        _namespace = @namespace;
        return this;
    }

    public TestNodeBuilder SetTypeName(string typeName)
    {
        _typeName = typeName;
        return this;
    }

    public TestNodeBuilder SetMethodName(string methodName)
    {
        _methodName = methodName;
        return this;
    }

    public TestNodeBuilder SetSourceFilePath(string sourceFilePath)
    {
        _sourceFilePath = sourceFilePath;
        return this;
    }

    public TestNodeBuilder SetSourceLineNumber(int sourceLineNumber)
    {
        _sourceLineNumber = sourceLineNumber;
        return this;
    }

    public TestNodeBuilder SetTrait(string name, string value)
    {
        _traits[name] = value;
        return this;
    }

    public TestNodeBuilder SetOutcome(TestOutcome testOutcome)
    {
        _testOutcome = testOutcome;
        return this;
    }

    public TestNodeBuilder SetErrorMessage(string errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    public TestNodeBuilder SetErrorStackTrace(string errorStackTrace)
    {
        _errorStackTrace = errorStackTrace;
        return this;
    }

    public TestNode Build()
    {
        var properties = new PropertyBag();

        // State
        properties.Add(
            _testOutcome switch
            {
                TestOutcome.None => DiscoveredTestNodeStateProperty.CachedInstance,
                TestOutcome.Passed => PassedTestNodeStateProperty.CachedInstance,
                TestOutcome.Failed => !string.IsNullOrWhiteSpace(_errorStackTrace)
                    ? new FailedTestNodeStateProperty(
                        new Exception(_errorMessage).ReplaceStackTrace(_errorStackTrace),
                        _errorMessage
                    )
                    : new FailedTestNodeStateProperty(_errorMessage ?? "Test failed."),
                TestOutcome.Skipped => SkippedTestNodeStateProperty.CachedInstance,
                _ => DiscoveredTestNodeStateProperty.CachedInstance,
            }
        );

        // Method info
        properties.Add(
            new TestMethodIdentifierProperty(
                _assemblyName,
                _namespace,
                _typeName,
                _methodName,
                0,
                [],
                "void"
            )
        );

        // Location info
        if (_sourceFilePath is not null && _sourceLineNumber is not null)
        {
            properties.Add(
                new TestFileLocationProperty(
                    _sourceFilePath,
                    new LinePositionSpan(
                        new LinePosition(_sourceLineNumber.Value, 0),
                        new LinePosition(_sourceLineNumber.Value, 0)
                    )
                )
            );
        }

        // Traits
        foreach (var trait in _traits)
            properties.Add(new TestMetadataProperty(trait.Key, trait.Value));

        return new TestNode
        {
            Uid = Guid.NewGuid().ToString(),
            DisplayName = _displayName,
            Properties = properties,
        };
    }
}
