using CodeWF.Core.Models;
using Prism.Events;

namespace CodeWF.Toolbox.Events;

internal class ChangeToolMenuEvent : PubSubEvent<ToolMenuItem>
{
}