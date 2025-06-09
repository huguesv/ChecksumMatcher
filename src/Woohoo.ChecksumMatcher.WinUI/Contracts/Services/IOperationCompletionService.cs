// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

using System.Threading.Tasks;
using Woohoo.ChecksumMatcher.WinUI.Models;

public interface IOperationCompletionService
{
    Task NotifyCompletion(OperationCompletionResult result, string message, CancellationToken ct);

    Task NotifyCompletionWithOpenInExplorer(OperationCompletionResult result, string message, string filePath, CancellationToken ct);
}
