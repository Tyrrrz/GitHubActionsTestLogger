using System;
using System.Runtime.ExceptionServices;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class ExceptionExtensions
{
    public static Exception ReplaceStackTrace(this Exception exception, string stackTrace)
    {
        ExceptionDispatchInfo.SetRemoteStackTrace(exception, stackTrace);
        return exception;
    }
}
