; 只构造内存中的维护选项，不访问真实产品注册或启动任何卸载器。
#define AppVersion "1.0.0"
#define CodeWFSetupFixture
#define CodeWFSetupAssetPath ".."
[Setup]
AppId=CodeWF.Installer.Maintenance.Verification.Only
AppName=CodeWF Installer Maintenance Fixture
AppVersion={#AppVersion}
DefaultDirName={tmp}\CodeWF.Installer.Maintenance.Fixture
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
Uninstallable=no
CreateUninstallRegKey=no
OutputBaseFilename=CodeWF.Installer.Maintenance.Fixture
WizardStyle=modern
LicenseFile=..\..\..\LICENSE
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "..\..\Languages\ChineseSimplified.isl"
[Files]
Source: "..\..\..\LICENSE"; DestDir: "{app}"
#include "..\CodeWF.Setup.iss"

[Code]
<event('InitializeWizard')>
procedure CWFixtureMaintenance;
begin
  CWMRegistrationCount := 1;
  CWMRepairPath := ExpandConstant('{tmp}\维护样例 with spaces');
  CWMRegistrations[0].RootKey := HKCU;
  CWMRegistrations[0].ScopeName := CustomMessage('CWMCurrentUser');
  CWMRegistrations[0].AppPath := ExpandConstant('{tmp}\已注册样例 with spaces');
  CWMCreatePage;
end;
