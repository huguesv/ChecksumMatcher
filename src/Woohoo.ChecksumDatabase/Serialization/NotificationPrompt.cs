// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;

[Flags]
public enum NotificationPrompt
{
    Yes = 0x01,
    YesToAll = 0x02,
    No = 0x04,
    NoToAll = 0x08,
}
