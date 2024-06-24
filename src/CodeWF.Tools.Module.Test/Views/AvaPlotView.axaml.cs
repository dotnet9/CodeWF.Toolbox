using Avalonia.Markup.Xaml;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.DataSources;
using ScottPlot.Plottables;

namespace CodeWF.Tools.Module.Test.Views;

public partial class AvaPlotView : UserControl
{
    public AvaPlotView()
    {
        InitializeComponent();

        AvaPlot avaPlot1 = this.Find<AvaPlot>("AvaPlot1");

        const int lineCount = 5;
        for (int i = 0; i < lineCount; i++)
        {
            var x = Enumerable.Range(0, 10).Select(index => index + 1.0).ToArray();
            var y = Enumerable.Range(0, 10).Select(index => Random.Shared.Next(1, 20) + 1.0).ToList();
            var logger = new DataStreamer(avaPlot1.Plot, x);
            avaPlot1.Plot.Add.Plottable(logger);
        }

        avaPlot1.Refresh();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}