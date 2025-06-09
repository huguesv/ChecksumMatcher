// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest
{
    using System.IO;
    using Woohoo.IO.Compression.SevenZip;

    public class SevenZipFileUnitTest
    {
        [Fact]
        public void Open()
        {
            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFilePath, Archives.sevenzip_multi_data);

                SevenZipFile archive = new SevenZipFile(tempFilePath);
                archive.Entries.Count.Should().Be(3);

                SevenZipEntry entry = archive.Entries[2];
                entry.Name.Should().Be("multi3.bin");
                entry.Size.Should().Be(64u);
                entry.CRC32.Should().Be(0xe23eff1b);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public void Extract()
        {
            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFilePath, Archives.sevenzip_multi_data);

                SevenZipFile archive = new SevenZipFile(tempFilePath);
                archive.Entries.Count.Should().Be(3);

                string extractedTempFilePath = Path.GetTempFileName();
                try
                {
                    archive.Extract(archive.Entries[2], extractedTempFilePath);
                    File.Exists(extractedTempFilePath).Should().BeTrue();
                    new FileInfo(extractedTempFilePath).Length.Should().Be(64);
                }
                finally
                {
                    File.Delete(extractedTempFilePath);
                }
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
