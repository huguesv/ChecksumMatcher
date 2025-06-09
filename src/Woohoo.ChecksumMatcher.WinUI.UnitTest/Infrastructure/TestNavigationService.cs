// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

internal class TestNavigationService : INavigationService
{
#pragma warning disable CS0067
    public event NavigatedEventHandler? Navigated;
#pragma warning restore CS0067

    public bool CanGoBack => throw new NotImplementedException();

    public Frame? Frame { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool GoBack()
    {
        throw new NotImplementedException();
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        throw new NotImplementedException();
    }

    public void SetListDataItemForNextConnectedAnimation(object item)
    {
        throw new NotImplementedException();
    }
}
