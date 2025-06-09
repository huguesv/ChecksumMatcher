// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

public interface IDatabaseHeaderLoader
{
    string Load(string workingFolderPath, string headerFileName);
}
