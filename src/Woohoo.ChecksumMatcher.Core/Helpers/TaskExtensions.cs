// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.
// Based on code from AsyncAwaitBestPractices (https://github.com/TheCodeTraveler/AsyncAwaitBestPractices)
// Copyright (c) 2018 Brandon Minnick

namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

public static partial class TaskExtensions
{
    private static Action<Exception>? onException;
    private static bool shouldAlwaysRethrowException;

    public static void FireAndForget(this ValueTask task, in Action<Exception>? onException = null, in bool continueOnCapturedContext = false)
        => HandleFireAndForget(task, continueOnCapturedContext, onException);

    public static void FireAndForget<T>(this ValueTask<T> task, in Action<Exception>? onException = null, in bool continueOnCapturedContext = false)
        => HandleFireAndForget(task, continueOnCapturedContext, onException);

    public static void FireAndForget<TException>(this ValueTask task, in Action<TException>? onException = null, in bool continueOnCapturedContext = false)
        where TException : Exception
        => HandleFireAndForget(task, continueOnCapturedContext, onException);

    public static void FireAndForget<T, TException>(this ValueTask<T> task, in Action<TException>? onException = null, in bool continueOnCapturedContext = false)
        where TException : Exception
        => HandleFireAndForget(task, continueOnCapturedContext, onException);

    public static void FireAndForget(this Task task, in ConfigureAwaitOptions configureAwaitOptions, in Action<Exception>? onException = null)
        => HandleFireAndForget(task, configureAwaitOptions, onException);

    public static void FireAndForget<TException>(this Task task, in ConfigureAwaitOptions configureAwaitOptions, in Action<TException>? onException = null)
        where TException : Exception
        => HandleFireAndForget(task, configureAwaitOptions, onException);

    public static void FireAndForget(this Task task, in Action<Exception>? onException = null, in bool continueOnCapturedContext = false)
        => HandleFireAndForget(task, continueOnCapturedContext, onException);

    public static void FireAndForget<TException>(this Task task, in Action<TException>? onException = null, in bool continueOnCapturedContext = false)
        where TException : Exception
        => HandleFireAndForget(task, continueOnCapturedContext, onException);

    public static void Initialize(in bool shouldAlwaysRethrowException = false)
        => TaskExtensions.shouldAlwaysRethrowException = shouldAlwaysRethrowException;

    public static void RemoveDefaultExceptionHandling() => onException = null;

    public static void SetDefaultExceptionHandling(in Action<Exception> onException)
        => TaskExtensions.onException = onException ?? throw new ArgumentNullException(nameof(onException));

    private static async void HandleFireAndForget<TException>(ValueTask valueTask, bool continueOnCapturedContext, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await valueTask.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (TaskExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static async void HandleFireAndForget<T, TException>(ValueTask<T> valueTask, bool continueOnCapturedContext, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await valueTask.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (TaskExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static async void HandleFireAndForget<TException>(Task task, bool continueOnCapturedContext, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (TaskExtensions.onException is not null || onException is not null)
        {
            HandleException(ex, onException);

            if (shouldAlwaysRethrowException)
            {
                ExceptionDispatchInfo.Throw(ex);
            }
        }
    }

    private static async void HandleFireAndForget<TException>(Task task, ConfigureAwaitOptions configureAwaitOptions, Action<TException>? onException)
        where TException : Exception
    {
        try
        {
            await task.ConfigureAwait(configureAwaitOptions);
        }
        catch (TException ex) when (TaskExtensions.onException is not null || onException is not null)
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
        TaskExtensions.onException?.Invoke(exception);
        onException?.Invoke(exception);
    }
}
