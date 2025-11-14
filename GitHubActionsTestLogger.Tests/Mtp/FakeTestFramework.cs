using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;

namespace GitHubActionsTestLogger.Tests.Mtp;

internal class FakeTestFramework(IReadOnlyList<TestNode> testNodes) : ITestFramework, IDataProducer
{
    public string Uid => nameof(FakeTestFramework);

    public string Version => "1.33.7";

    public string DisplayName => "Fake Test Framework";

    public string Description => "A fake test framework with predefined test nodes.";

    public Type[] DataTypesProduced { get; } = [typeof(TestNodeUpdateMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context) =>
        Task.FromResult(new CreateTestSessionResult { IsSuccess = true });

    public async Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
        try
        {
            foreach (var testNode in testNodes)
            {
                await context.MessageBus.PublishAsync(
                    this,
                    new TestNodeUpdateMessage(context.Request.Session.SessionUid, testNode)
                );
            }
        }
        finally
        {
            context.Complete();
        }
    }

    public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context) =>
        Task.FromResult(new CloseTestSessionResult { IsSuccess = true });
}
