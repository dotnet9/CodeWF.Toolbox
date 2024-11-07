# CodeWF Toolbox

简体中文 | [English](README.md)

## 注意

Prism 9.X AOT有异常，暂时未解决

```shell
Application: CodeWF.Toolbox.Desktop.exe
CoreCLR Version: 9.0.0-rc.2.24473.5
Description: The process was terminated due to an unhandled exception.
Exception Info: DryIoc.ContainerException: code: Error.UnableToSelectSinglePublicConstructorFromNone;
message: Unable to select a single public constructor from the implementation type 'Prism.Ioc.Internals.ContainerProviderLocator' because the type does not have public constructors.
   at DryIoc.Throw.When(Boolean throwIfInvalid, Int32 error, Object arg0, Object arg1, Object arg2, Object arg3) + 0x78
   at DryIoc.Container.Register(Factory factory, Type serviceType, Object serviceKey, Nullable`1 ifAlreadyRegistered, Boolean isStaticallyChecked) + 0x6f
   at Prism.Container.DryIoc.DryIocContainerExtension..ctor(IContainer) + 0x8c
   at Prism.DryIoc.PrismApplication.CreateContainerExtension() + 0x59
   at Prism.PrismApplicationBase.Initialize() + 0x19
   at Avalonia.AppBuilder.SetupUnsafe() + 0x112
   at Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder, String[], Action`1) + 0x2f
   at CodeWF.Toolbox.Desktop.Program.Main(String[] args) + 0x1b

```