// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System.Threading;
using System.Threading.Tasks;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;

internal class TestOperationCompletionService : IOperationCompletionService
{
    public bool IsCompleted { get; private set; }

    public Task NotifyCompletion(OperationCompletionResult result, string message, CancellationToken ct)
    {
        this.IsCompleted = true;
        return Task.CompletedTask;
    }

    public Task NotifyCompletionWithOpenInExplorer(OperationCompletionResult result, string message, string filePath, CancellationToken ct)
    {
        this.IsCompleted = true;
        return Task.CompletedTask;
    }
}
