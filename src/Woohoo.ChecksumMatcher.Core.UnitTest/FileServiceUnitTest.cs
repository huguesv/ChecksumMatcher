// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest;

using System.Collections.Immutable;
using Woohoo.ChecksumMatcher.Core.Services;

public class FileServiceUnitTest
{
    [Fact]
    public void Save_String()
    {
        // Arrange
        var sut = new FileService();
        using var file = new TempFile();
        var content = "This is a test content.";
        var encodedContent = "\"This is a test content.\"";

        // Act
        sut.Save(file.FolderName, file.FileName, content);

        // Assert
        var actual = File.ReadAllText(file.FilePath);
        actual.Should().Be(encodedContent);
    }

    [Fact]
    public void Read_String()
    {
        // Arrange
        var sut = new FileService();
        using var file = new TempFile();
        var content = "This is a test content.";
        var encodedContent = "\"This is a test content.\"";
        File.WriteAllText(file.FilePath, encodedContent);

        // Act
        var actual = sut.Read<string>(file.FolderName, file.FileName);

        // Assert
        actual.Should().Be(content);
    }

    [Fact]
    public void Save_StringArray()
    {
        // Arrange
        var sut = new FileService();
        using var file = new TempFile();
        var content = new List<string> { "one", "two" };
        var encodedContent = "[\"one\",\"two\"]";

        // Act
        sut.Save(file.FolderName, file.FileName, content);

        // Assert
        var actual = File.ReadAllText(file.FilePath);
        actual.Should().Be(encodedContent);
    }

    [Fact]
    public void Read_StringArray()
    {
        // Arrange
        var sut = new FileService();
        using var file = new TempFile();
        var content = new List<string> { "one", "two" };
        var encodedContent = "[\"one\",\"two\"]";
        File.WriteAllText(file.FilePath, encodedContent);

        // Act
        var actual = sut.Read<List<string>>(file.FolderName, file.FileName);

        // Assert
        actual.Should().BeEquivalentTo(content);
    }

    [Fact]
    public void Save_Class()
    {
        // Arrange
        var sut = new FileService();
        using var file = new TempFile();
        var content = new ClassWithProps
        {
            RequiredString = "hello",
            OptionalInt = 42,
            LongImmutableArray = [1L, 2L, 3L],
            StringIntDictionary = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            },
        };
        var encodedContent = "{\"RequiredString\":\"hello\",\"OptionalInt\":42,\"LongImmutableArray\":[1,2,3],\"StringIntDictionary\":{\"one\":1,\"two\":2}}";

        // Act
        sut.Save(file.FolderName, file.FileName, content);

        // Assert
        var actual = File.ReadAllText(file.FilePath);
        actual.Should().Be(encodedContent);
    }

    [Fact]
    public void Read_Class()
    {
        // Arrange
        var sut = new FileService();
        using var file = new TempFile();
        var content = new ClassWithProps
        {
            RequiredString = "hello",
            OptionalInt = 42,
            LongImmutableArray = [1L, 2L, 3L],
            StringIntDictionary = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            },
        };
        var encodedContent = "{\"RequiredString\":\"hello\",\"OptionalInt\":42,\"LongImmutableArray\":[1,2,3],\"StringIntDictionary\":{\"one\":1,\"two\":2}}";
        File.WriteAllText(file.FilePath, encodedContent);

        // Act
        var actual = sut.Read<ClassWithProps>(file.FolderName, file.FileName);

        // Assert
        actual.Should().BeEquivalentTo(content);
    }

    internal class ClassWithProps
    {
        public required string? RequiredString { get; init; }

        public int OptionalInt { get; set; }

        public ImmutableArray<long> LongImmutableArray { get; init; } = [];

        public Dictionary<string, int> StringIntDictionary { get; init; } = [];
    }

    internal class TempFile : IDisposable
    {
        public TempFile()
        {
            this.FolderName = Path.GetTempPath();
            this.FileName = Guid.NewGuid().ToString();
            this.FilePath = Path.Combine(this.FolderName, this.FileName);
        }

        public string FolderName { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public void Dispose()
        {
            if (File.Exists(this.FilePath))
            {
                File.Delete(this.FilePath);
            }
        }
    }
}
