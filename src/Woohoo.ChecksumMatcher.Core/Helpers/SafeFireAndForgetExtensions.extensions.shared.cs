namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System;
using System.Threading.Tasks;

public static partial class SafeFireAndForgetExtensions
{
    public static void SafeFireAndForget(this ValueTask task, Action<Exception>? onException, bool continueOnCapturedContext = false) => task.SafeFireAndForget(in onException, in continueOnCapturedContext);

    public static void SafeFireAndForget<T>(this ValueTask<T> task, Action<Exception>? onException, bool continueOnCapturedContext = false) => task.SafeFireAndForget(in onException, in continueOnCapturedContext);

    public static void SafeFireAndForget<TException>(this ValueTask task, Action<TException>? onException, bool continueOnCapturedContext = false)
        where TException : Exception
        => task.SafeFireAndForget(in onException, in continueOnCapturedContext);

    public static void SafeFireAndForget<T, TException>(this ValueTask<T> task, Action<TException>? onException, bool continueOnCapturedContext = false)
        where TException : Exception
        => task.SafeFireAndForget(in onException, in continueOnCapturedContext);

    public static void SafeFireAndForget(this Task task, Action<Exception>? onException, bool continueOnCapturedContext = false) => task.SafeFireAndForget(in onException, in continueOnCapturedContext);

    public static void SafeFireAndForget<TException>(this Task task, Action<TException>? onException, bool continueOnCapturedContext = false)
        where TException : Exception
        => task.SafeFireAndForget(in onException, in continueOnCapturedContext);

    public static void SafeFireAndForget(this Task task, ConfigureAwaitOptions configureAwaitOptions, Action<Exception>? onException = null) => task.SafeFireAndForget(in configureAwaitOptions, in onException);

    public static void SafeFireAndForget<TException>(this Task task, ConfigureAwaitOptions configureAwaitOptions, Action<TException>? onException = null)
        where TException : Exception
        => task.SafeFireAndForget(in configureAwaitOptions, in onException);

    public static void Initialize(bool shouldAlwaysRethrowException) => Initialize(in shouldAlwaysRethrowException);

    public static void SetDefaultExceptionHandling(Action<Exception> onException) => SetDefaultExceptionHandling(in onException);
}
