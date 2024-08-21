using CodeWF.AvaloniaControls.Extensions;
using System.Collections.ObjectModel;

namespace CodeWF.Modules.AI.ViewModels;

public class ChoiceLanguagesViewModel
{
    public RangeObservableCollection<string> SelectedLanguages { get; set; }

    public RangeObservableCollection<string> AllLanguages { get; set; }
}