using CodeWF.EventBus;
using System;

namespace AvaloniaAotDemo.Commands;

internal class ChangeTimeCommand : Command
{
    public DateTime Time { get; set; }
}