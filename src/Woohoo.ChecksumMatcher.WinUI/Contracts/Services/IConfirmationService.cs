// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

using System.Threading.Tasks;

public interface IConfirmationService
{
    Task<bool> ShowAsync(string title, string message, bool defaultToCancel = false, string? confirmButtonText = null, string? cancelButtonText = null);
}
