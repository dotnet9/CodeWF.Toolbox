using CodeWF.AvaloniaControls.Extensions;

namespace CodeWF.Modules.AI.ViewModels;

public class ChoiceLanguagesViewModel
{
    public RangeObservableCollection<string> SelectedLanguages { get; set; }

    public RangeObservableCollection<string> AllLanguages { get; set; }
}