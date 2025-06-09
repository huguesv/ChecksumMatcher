namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

public static partial class SafeFireAndForgetExtensions
{
    private static Action<Exception>? onException;
    private static bool shouldAlwaysRethrowException;

    public static void SafeFireAndForget(this ValueTask task, in Action<Exception>? onException = null, in bool continueOnCapturedContext = false)
        => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget<T>(this ValueTask<T> task, in Action<Exception>? onException = null, in bool continueOnCapturedContext = false)
        => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget<TException>(this ValueTask task, in Action<TException>? onException = null, in bool continueOnCapturedContext = false)
        where TException : Exception
        => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget<T, TException>(this ValueTask<T> task, in Action<TException>? onException = null, in bool continueOnCapturedContext = false)
        where TException : Exception
        => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget(this Task task, in ConfigureAwaitOptions configureAwaitOptions, in Action<Exception>? onException = null)
        => HandleSafeFireAndForget(task, configureAwaitOptions, onException);

    public static void SafeFireAndForget<TException>(this Task task, in ConfigureAwaitOptions configureAwaitOptions, in Action<TException>? onException = null)
        where TException : Exception
        => HandleSafeFireAndForget(task, configureAwaitOptions, onException);

    public static void SafeFireAndForget(this Task task, in Action<Exception>? onException = null, in bool continueOnCapturedContext = false)
        => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget<TException>(this Task task, in Action<TException>? onException = null, in bool continueOnCapturedContext = false)
        where TException : Exception
        => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void Initialize(in bool shouldAlwaysRethrowException = false)
        => SafeFireAndForgetExtensions.shouldAlwaysRethrowException = shouldAlwaysRethrowException;

    public static void RemoveDefaultExceptionHandling() => onException = null;

    public static void SetDefaultExceptionHandling(in Action<Exception> onException)
        => SafeFireAndForgetExtensions.onException = onException ?? throw new ArgumentNullException(nameof(onException));

    private static async void HandleSafeFireAndForget<TException>(ValueTask valueTask, bool continueOnCapturedContext, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await valueTask.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (SafeFireAndForgetExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static async void HandleSafeFireAndForget<T, TException>(ValueTask<T> valueTask, bool continueOnCapturedContext, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await valueTask.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (SafeFireAndForgetExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static async void HandleSafeFireAndForget<TException>(Task task, bool continueOnCapturedContext, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (SafeFireAndForgetExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static async void HandleSafeFireAndForget<TException>(Task task, ConfigureAwaitOptions configureAwaitOptions, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await task.ConfigureAwait(configureAwaitOptions);
        }
        catch (TException ex) when (SafeFireAndForgetExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static void HandleException<TException>(in TException exception, in Action<TException>? onException)
        where TException : Exception
    {
        SafeFireAndForgetExtensions.onException?.Invoke(exception);
        onException?.Invoke(exception);
    }
}
