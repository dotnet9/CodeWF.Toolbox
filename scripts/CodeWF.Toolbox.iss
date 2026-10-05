; CodeWF.Toolbox Windows installer.
; Build from the repository root with Inno Setup 6 and pass /DAppVersion=x.y.z.

#ifndef AppVersion
#define AppVersion "0.0.0"
#endif

#ifndef SourceDir
#define SourceDir "..\artifacts\publish\win-x64\CodeWF.Toolbox"
#endif

#ifndef OutputDir
#define OutputDir "..\artifacts\release"
#endif

[Setup]
AppId={{{A3B4C5D6-E7F8-4A9B-8C7D-6E5F4A3B2C1D}}
AppName=CodeWF.Toolbox
AppVersion={#AppVersion}
AppPublisher=Dotnet9
AppPublisherURL=https://github.com/dotnet9/CodeWF.Toolbox
AppSupportURL=https://github.com/dotnet9/CodeWF.Toolbox/issues
DefaultDirName={autopf}\CodeWF.Toolbox
DefaultGroupName=CodeWF.Toolbox
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=CodeWF.Toolbox-v{#AppVersion}-win-x64-setup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\CodeWF.Toolbox"; Filename: "{app}\CodeWF.Toolbox.exe"
Name: "{autodesktop}\CodeWF.Toolbox"; Filename: "{app}\CodeWF.Toolbox.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Run]
Filename: "{app}\CodeWF.Toolbox.exe"; Description: "{cm:LaunchProgram,CodeWF.Toolbox}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
