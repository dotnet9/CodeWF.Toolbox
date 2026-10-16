; 独立安装器验收夹具：不创建卸载注册表、快捷方式、文件关联或启动应用。
#define AppVersion "1.0.0"
#define CodeWFSetupFixture
#define CodeWFSetupAssetPath ".."
[Setup]
AppId=CodeWF.Installer.Verification.Only
AppName=CodeWF Installer Fixture
AppVersion={#AppVersion}
DefaultDirName={tmp}\CodeWF.Installer.Fixture
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
Uninstallable=no
CreateUninstallRegKey=no
OutputBaseFilename=CodeWF.Installer.Fixture
WizardStyle=modern
LicenseFile=..\..\..\LICENSE
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "..\..\Languages\ChineseSimplified.isl"
[Files]
Source: "..\..\..\LICENSE"; DestDir: "{app}"
[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
#include "..\CodeWF.Setup.iss"
#include "Fixture.Progress.iss"
