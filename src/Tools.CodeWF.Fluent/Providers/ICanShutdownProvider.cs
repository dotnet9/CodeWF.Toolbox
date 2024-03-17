namespace Tools.CodeWF.Fluent.Providers;

public interface ICanShutdownProvider
{
	bool CanShutdown(bool restart, out bool isShutdownEnforced);
}
