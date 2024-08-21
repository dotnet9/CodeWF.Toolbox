using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaAotDemo.Commands;
using AvaloniaAotDemo.ViewModels;
using CodeWF.EventBus;
using System;

namespace AvaloniaAotDemo.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        EventBus.Default.Subscribe<ChangeTimeCommand>(ReceiveEvent);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        EventBus.Default.Publish(new ChangeTimeCommand() { Time = DateTime.Now });
    }

    private void ReceiveEvent(ChangeTimeCommand command)
    {
        this.FindControl<TextBlock>("Txt").Text = command.Time.ToString("yyyy-MM-dd HH:mm:ss fff");
    }
}