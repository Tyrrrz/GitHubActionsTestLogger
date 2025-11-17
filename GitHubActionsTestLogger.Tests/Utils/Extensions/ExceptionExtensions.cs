using System;
using System.Runtime.ExceptionServices;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class ExceptionExtensions
{
    extension(Exception exception)
    {
        public Exception ReplaceStackTrace(string stackTrace)
        {
            ExceptionDispatchInfo.SetRemoteStackTrace(exception, stackTrace);
            return exception;
        }
    }
}
