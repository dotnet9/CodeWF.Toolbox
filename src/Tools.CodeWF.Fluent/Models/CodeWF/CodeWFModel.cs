using ReactiveUI;
using System.ComponentModel;

namespace Tools.CodeWF.Fluent.Models.CodeWF;

public partial interface IWalletModel : INotifyPropertyChanged;

[AutoInterface]
public partial class CodeWFModel : ReactiveObject
{
	public CodeWFModel()
	{
	}
}
