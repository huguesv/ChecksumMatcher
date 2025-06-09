// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Behaviors;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public class NavigationViewHeaderBehavior : Behavior<NavigationView>
{
    public static readonly DependencyProperty DefaultHeaderProperty =
        DependencyProperty.Register("DefaultHeader", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => current!.UpdateHeader()));

    public static readonly DependencyProperty HeaderModeProperty =
        DependencyProperty.RegisterAttached("HeaderMode", typeof(bool), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(NavigationViewHeaderMode.Always, (d, e) => current!.UpdateHeader()));

    public static readonly DependencyProperty HeaderContextProperty =
        DependencyProperty.RegisterAttached("HeaderContext", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => current!.UpdateHeader()));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => current!.UpdateHeaderTemplate()));

    private static NavigationViewHeaderBehavior? current;

    private Page? currentPage;

    public DataTemplate? DefaultHeaderTemplate
    {
        get; set;
    }

    public object DefaultHeader
    {
        get => this.GetValue(DefaultHeaderProperty);
        set => this.SetValue(DefaultHeaderProperty, value);
    }

    public static NavigationViewHeaderMode GetHeaderMode(Page item) => (NavigationViewHeaderMode)item.GetValue(HeaderModeProperty);

    public static void SetHeaderMode(Page item, NavigationViewHeaderMode value) => item.SetValue(HeaderModeProperty, value);

    public static object GetHeaderContext(Page item) => item.GetValue(HeaderContextProperty);

    public static void SetHeaderContext(Page item, object value) => item.SetValue(HeaderContextProperty, value);

    public static DataTemplate GetHeaderTemplate(Page item) => (DataTemplate)item.GetValue(HeaderTemplateProperty);

    public static void SetHeaderTemplate(Page item, DataTemplate value) => item.SetValue(HeaderTemplateProperty, value);

    protected override void OnAttached()
    {
        base.OnAttached();

        var navigationService = App.GetService<INavigationService>();
        navigationService.Navigated += this.OnNavigated;

        current = this;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        var navigationService = App.GetService<INavigationService>();
        navigationService.Navigated -= this.OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame && frame.Content is Page page)
        {
            this.currentPage = page;

            this.UpdateHeader();
            this.UpdateHeaderTemplate();
        }
    }

    private void UpdateHeader()
    {
        if (this.currentPage != null)
        {
            var headerMode = GetHeaderMode(this.currentPage);
            if (headerMode == NavigationViewHeaderMode.Never)
            {
                this.AssociatedObject.Header = null;
                this.AssociatedObject.AlwaysShowHeader = false;
            }
            else
            {
                var headerFromPage = GetHeaderContext(this.currentPage);
                if (headerFromPage != null)
                {
                    this.AssociatedObject.Header = headerFromPage;
                }
                else
                {
                    this.AssociatedObject.Header = this.DefaultHeader;
                }

                if (headerMode == NavigationViewHeaderMode.Always)
                {
                    this.AssociatedObject.AlwaysShowHeader = true;
                }
                else
                {
                    this.AssociatedObject.AlwaysShowHeader = false;
                }
            }
        }
    }

    private void UpdateHeaderTemplate()
    {
        if (this.currentPage != null)
        {
            var headerTemplate = GetHeaderTemplate(this.currentPage);
            this.AssociatedObject.HeaderTemplate = headerTemplate ?? this.DefaultHeaderTemplate;
        }
    }
}
