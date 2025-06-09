namespace Woohoo.ChecksumMatcher.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

public class ColumnSharedSizeBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty SharedSizeGroupProperty =
        DependencyProperty.RegisterAttached(nameof(SharedSizeGroup), typeof(string), typeof(ColumnSharedSizeBehavior), new PropertyMetadata(null));

    public string SharedSizeGroup
    {
        get => (string)this.GetValue(SharedSizeGroupProperty);
        set => this.SetValue(SharedSizeGroupProperty, value);
    }

    public void OnColumnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not FrameworkElement o)
        {
            return;
        }

        if (this.SharedSizeGroup is null)
        {
            return;
        }

        FrameworkElement p = o;
        while (!ColumnSharedSizeHelper.GetIsSharedSizeScope(p))
        {
            if (VisualTreeHelper.GetParent(p) is not FrameworkElement fe)
            {
                break;
            }
            else
            {
                p = fe;
            }
        }

        if (!ColumnSharedSizeHelper.GetIsSharedSizeScope(p))
        {
            return;
        }

        var groups = ColumnSharedSizeHelper.GetGroups(p);
        if (groups is null)
        {
            groups = new();
            ColumnSharedSizeHelper.SetGroups(p, groups);
        }

        if (!groups.ContainsKey(this.SharedSizeGroup))
        {
            groups[this.SharedSizeGroup] = new ColumnSharedSizeGroup();
        }

        groups[this.SharedSizeGroup].Update(o);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        this.AssociatedObject.SizeChanged += this.OnColumnSizeChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        this.AssociatedObject.SizeChanged -= this.OnColumnSizeChanged;
    }
}
