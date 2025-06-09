// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Models;

using System.Diagnostics;

[DebuggerDisplay("Name = {Name} Label = {Label} Serial = {SerialNumber}")]
public class OfflineHeader
{
    public string Name { get; set; } = string.Empty;

    public string? Label { get; set; }

    public string? SerialNumber { get; set; }

    public long TotalSize { get; set; }
}
