// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

public interface INotificationSink
{
    void Warning(string item, string warning);

    NotificationPrompt Prompt(string item, string warning, NotificationPrompt prompt);
}
