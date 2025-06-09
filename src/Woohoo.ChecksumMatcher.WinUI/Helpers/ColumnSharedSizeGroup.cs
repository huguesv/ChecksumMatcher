namespace Woohoo.ChecksumMatcher.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public class ColumnSharedSizeGroup : DependencyObject
{
    private readonly List<ColumnDefinition> columns = new();
    private double colSize = 0.0;

    public void Update(FrameworkElement item)
    {
        var grid = (Grid)item.Parent;
        var column = grid.ColumnDefinitions[(int)item.GetValue(Grid.ColumnProperty)];
        if (!this.columns.Contains(column))
        {
            this.columns.Add(column);
        }

        var adjustments = new List<ColumnDefinition>();
        var width = item.ActualWidth + item.Margin.Left + item.Margin.Right;
        if (width > this.colSize)
        {
            this.colSize = width;
            adjustments.AddRange(this.columns);
        }
        else
        {
            adjustments.Add(column);
        }

        foreach (var col in adjustments)
        {
            col.Width = new GridLength(this.colSize);
        }
    }
}
