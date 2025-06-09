// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest;

using Woohoo.IO.Compression.Chd;
using Woohoo.IO.Compression.UnitTest.Infrastructure;

public class ChdFileUnitTest
{
    [Fact]
    public void OpenV5()
    {
        // Arrange
        using var tempArchiveFile = DisposableTempFile.WithBytes(Archives.smalldvdv5);

        // Act
        var archive = new ChdFile(tempArchiveFile.FilePath);

        // Assert
        archive.FormatVersion.Should().Be(5);
        archive.SHA1.Should().BeEquivalentTo(new byte[] { 0xAC, 0x0F, 0xAF, 0x0A, 0xDA, 0x37, 0xFA, 0x71, 0x7E, 0xA8, 0xA3, 0x81, 0x43, 0xD4, 0x19, 0x9A, 0x17, 0xBD, 0xF3, 0xC8 });
    }
}
