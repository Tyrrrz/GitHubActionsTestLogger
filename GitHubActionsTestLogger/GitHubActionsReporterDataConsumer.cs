using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;

namespace GitHubActionsTestLogger;

internal sealed class GitHubActionsReporterDataConsumer : IDataConsumer
{
    private IServiceProvider _serviceProvider;

    public GitHubActionsReporterDataConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

    public string Uid => nameof(GitHubActionsReporterDataConsumer);

    public string Version => "1.0.0";

    public string DisplayName => "GitHub Actions Reporter";

    public string Description => "Reports test results to GitHub actions";

    public Task ConsumeAsync(
        IDataProducer dataProducer,
        IData value,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(0);
    }

    public Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(true);
    }
}
