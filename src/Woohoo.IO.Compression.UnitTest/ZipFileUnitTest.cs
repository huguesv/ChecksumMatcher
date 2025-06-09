// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.UnitTest
{
    using System.Collections.Generic;
    using System.IO;
    using ICSharpCode.SharpZipLib.Zip;
    using Woohoo.IO.Compression.Zip;

    public class ZipFileUnitTest
    {
        [Fact]
        public void Open()
        {
            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFilePath, Archives.bin_data);

                using (ZipFile archive = new ZipFile(tempFilePath))
                {
                    archive.Count.Should().Be(1);

                    ZipEntry entry = archive[0];
                    entry.Name.Should().Be("bin-data.bin");
                    entry.Size.Should().Be(10);
                    entry.Crc.Should().Be(0x456cd746);
                }
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
                File.WriteAllBytes(tempFilePath, Archives.bin_data);

                using (ZipFile archive = new ZipFile(tempFilePath))
                {
                    archive.Count.Should().Be(1);

                    string extractedTempFilePath = Path.GetTempFileName();
                    try
                    {
                        archive.Extract(archive[0], extractedTempFilePath);
                        File.Exists(extractedTempFilePath).Should().BeTrue();
                        new FileInfo(extractedTempFilePath).Length.Should().Be(10);
                    }
                    finally
                    {
                        File.Delete(extractedTempFilePath);
                    }
                }
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Theory]
        [InlineData("multi1.bin")]
        [InlineData("multi2.bin")]
        [InlineData("multi3.bin")]
        public void GetStream(string entryName)
        {
            var entryNameToExpectedData = new Dictionary<string, byte[]>()
            {
                { "multi1.bin", Archives.multi1 },
                { "multi2.bin", Archives.multi2 },
                { "multi3.bin", Archives.multi3 },
            };

            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFilePath, Archives.zip_multi_data);

                using (ZipFile archive = new ZipFile(tempFilePath))
                {
                    archive.Count.Should().Be(3);

                    ZipEntry entry = archive.GetEntry(entryName);
                    using (Stream stream = archive.GetStream(entry))
                    {
                        byte[] buffer = new byte[entry.Size];
                        MemoryStream outputStream = new MemoryStream(buffer);

                        stream.CopyTo(outputStream);
                        outputStream.Flush();

                        byte[] expectedData = entryNameToExpectedData[entryName];
                        buffer.Should().BeEquivalentTo(expectedData);
                    }
                }
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
