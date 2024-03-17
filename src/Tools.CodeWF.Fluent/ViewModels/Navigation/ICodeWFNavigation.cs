using Tools.CodeWF.Fluent.Models.CodeWF;
using Tools.CodeWF.Fluent.ViewModels.CodeWF;

namespace Tools.CodeWF.Fluent.ViewModels.Navigation;

public interface ICodeWFNavigation
{
	ICodeWFViewModel? To(IWalletModel wallet);
}

public interface IWalletSelector : ICodeWFNavigation
{
	ICodeWFViewModel? SelectedWallet { get; }

	IWalletModel? SelectedWalletModel { get; }
}
