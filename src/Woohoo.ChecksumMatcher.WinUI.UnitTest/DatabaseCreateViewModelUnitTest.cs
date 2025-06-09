// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest;

using Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

public class DatabaseCreateViewModelUnitTest
{
    [UIFact]
    public void Create()
    {
        // Arrange

        // Act
        var sut = new DatabaseCreateViewModel(
            new TestDatabaseService(),
            new TestFilePickerService(),
            new TestOperationCompletionService(),
            new TestDateTimeProviderService(),
            new TestDispatcherQueueService());

        // Assert
    }

    [UIFact]
    public void InsertDateRedump()
    {
        // Arrange
        var sut = new DatabaseCreateViewModel(
            new TestDatabaseService(),
            new TestFilePickerService(),
            new TestOperationCompletionService(),
            new TestDateTimeProviderService(now: new DateTime(2000, 12, 24, 10, 15, 00), utcNow: new DateTime(2000, 12, 24, 18, 15, 00)),
            new TestDispatcherQueueService());

        // Act
        sut.InsertDateRedumpCommand.Execute(null);

        // Assert
        sut.DatabaseVersion.Should().Be("2000-12-24 18-15-00");
    }

    [UIFact]
    public void InsertDateNoIntro()
    {
        // Arrange
        var sut = new DatabaseCreateViewModel(
            new TestDatabaseService(),
            new TestFilePickerService(),
            new TestOperationCompletionService(),
            new TestDateTimeProviderService(now: new DateTime(2000, 12, 24, 10, 15, 00), utcNow: new DateTime(2000, 12, 24, 18, 15, 00)),
            new TestDispatcherQueueService());

        // Act
        sut.InsertDateNoIntroCommand.Execute(null);

        // Assert
        sut.DatabaseVersion.Should().Be("20001224-181500");
    }
}
