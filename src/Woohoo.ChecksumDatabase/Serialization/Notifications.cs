// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

public class Notifications
{
    public static class Warnings
    {
        public const string DuplicateGameName = "Duplicate game name";
        public const string DuplicateRomName = "Duplicate rom name";
        public const string DuplicateRomNameDifferentContent = "Duplicate rom name with different size or checksums";
    }
}
