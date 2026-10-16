{ 纯逻辑断言仅供 CodeWFSetupFixture 使用；InitializeSetup 返回 False，安装向导不会创建。 }
#ifndef CodeWFSetupFixture
#error Maintenance logic assertions require CodeWFSetupFixture
#endif

[Code]
var TestCount: Integer; TestReport: String;
procedure Check(Condition: Boolean; const Name: String);
begin
  if not Condition then RaiseException('FAIL: ' + Name);
  TestCount := TestCount + 1;
  TestReport := TestReport + 'PASS: ' + Name + #13#10;
end;
procedure Parse(const Command, Directory: String; Expected: Boolean; const Name: String);
var Executable: String;
begin
  Check(CWMParseUninstaller(Command, Directory, Executable) = Expected, Name);
  if Expected then Check(CompareText(Executable, 'C:\CodeWF Test\unins000.exe') = 0, Name + ' exact path')
  else Check(Executable = '', Name + ' no output');
end;
<event('InitializeSetup')>
function TestInitialize: Boolean;
var Entry: TCWMRegistration; Code: Integer; NeedsRestart, Cancel, Confirm: Boolean;
begin
  Result := False;
  try
    Check(CWMUninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#ExpectedAppId}_is1', 'exact AppId escape / suffix');
    Parse('"C:\CodeWF Test\unins000.exe"', 'C:\CodeWF Test', True, 'quoted path with spaces');
    Parse('C:\CodeWF Test\unins000.exe', 'C:\CodeWF Test\', True, 'unquoted entire path');
    Parse('"c:\codewf test\UNINS000.EXE"', 'C:\CodeWF Test', True, 'case-insensitive exact directory');
    Parse('"C:\CodeWF Test\unins000.exe" /SILENT', 'C:\CodeWF Test', False, 'reject arguments');
    Parse('"C:\CodeWF Test\unins000.exe" & calc.exe', 'C:\CodeWF Test', False, 'reject shell expression');
    Parse('"C:\CodeWF Test\unins000.exe" "x"', 'C:\CodeWF Test', False, 'reject quoted arguments');
    Parse('"C:\CodeWF Test\unins000.exe', 'C:\CodeWF Test', False, 'reject unmatched quotes');
    Parse('C:\CodeWF Test\other.exe', 'C:\CodeWF Test', False, 'reject unrelated executable');
    Parse('C:\CodeWF Test\unins.exe', 'C:\CodeWF Test', False, 'require numeric Inno suffix');
    Parse('C:\CodeWF Test\uninsABC.exe', 'C:\CodeWF Test', False, 'reject nonnumeric Inno suffix');
    Parse('C:\CodeWF Test-other\unins000.exe', 'C:\CodeWF Test', False, 'reject sibling prefix');
    Parse('C:\CodeWF Test\child\unins000.exe', 'C:\CodeWF Test', False, 'reject child location');
    Parse('C:\Elsewhere\unins000.exe', 'C:\CodeWF Test', False, 'reject other directory');
    Parse('unins000.exe', 'C:\CodeWF Test', False, 'reject relative path');
    Parse('\\server\share\unins000.exe', '\\server\share', False, 'reject network path');
    Parse('C:\CodeWF Test\unins000.exe', '', False, 'reject unknown install location');
    Parse('C:\CodeWF Test\unins000.exe', 'C:\', False, 'reject drive root');
    Parse('C:\CodeWF Test\unins000.exe' + #13#10 + 'x', 'C:\CodeWF Test', False, 'reject line breaks');
    Parse('C:\CodeWF Test\unins000.exe:payload', 'C:\CodeWF Test', False, 'reject alternate data stream');
    Parse('C:\CodeWF Test\unins000.exe', 'C:\CodeWF Test-other', False, 'reject prefix registration');
    Check(CWMNormalizePath('C:\CodeWF Test\..\Elsewhere') = 'C:\Elsewhere', 'canonical directory');
    Check(not CWMUninstallSucceeded(False, 0, False), 'launch failure cannot succeed');
    Check(not CWMUninstallSucceeded(True, 2, False), 'cancel cannot succeed');
    Check(not CWMUninstallSucceeded(True, 0, True), 'remaining registration cannot succeed');
    Check(CWMUninstallSucceeded(True, 0, False), 'success requires exit zero and registration removed');
    Check(not CWMReadRegistration(HKCU32, 'fixture', Entry), 'fixture registration probe disabled');
    Check(not CWMStartUninstaller('Z:\CodeWF.Nonexistent.Fixture\unins000.exe', Code), 'fixture process launch disabled');
    Check(Code = 5, 'fixture launch cannot signal success');
    CWMUninstalled := True;
    Check(not CWMMaintenanceNext(wpWelcome), 'uninstall success blocks Next');
    NeedsRestart := False;
    Check(CWMMaintenancePrepare(NeedsRestart) <> '', 'uninstall success blocks install preparation');
    Cancel := False; Confirm := True;
    CWMMaintenanceCancel(wpWelcome, Cancel, Confirm);
    Check(Cancel and not Confirm, 'uninstall success closes without cancel confirmation');
    CWMUninstalled := False; CWMBusy := True;
    Check(not CWMMaintenanceNext(wpWelcome), 'running uninstaller blocks Next');
    Check(CWMMaintenancePrepare(NeedsRestart) <> '', 'running uninstaller blocks install preparation');
    Cancel := True; Confirm := True;
    CWMMaintenanceCancel(wpWelcome, Cancel, Confirm);
    Check(not Cancel and not Confirm, 'running uninstaller blocks close');
    CWMBusy := False;
    Check(CWMMaintenanceNext(wpWelcome), 'normal install retains Next');
    Check(CWMMaintenancePrepare(NeedsRestart) = '', 'normal install retains preparation');
    SaveStringToFile('{#TestReport}', 'PASS ' + IntToStr(TestCount) + #13#10 + TestReport, False);
  except
    SaveStringToFile('{#TestReport}', GetExceptionMessage, False);
  end;
end;
