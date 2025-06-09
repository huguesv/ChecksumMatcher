// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

public sealed class DateTimeProviderService : IDateTimeProviderService
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
}
