#ifndef CodeWFSetupMaintenanceIncluded
#define CodeWFSetupMaintenanceIncluded

[CustomMessages]
; 已有安装的原生维护入口；只读本产品注册，不接管安装或卸载引擎。
english.CWMMaintenance=Maintain existing installation
chinesesimplified.CWMMaintenance=维护已有安装
english.CWMMaintenanceDescription=Choose what to do with the registered installation.
chinesesimplified.CWMMaintenanceDescription=选择如何处理已注册的安装。
english.CWMMaintenanceHint=Repair uses this package and the normal installation steps. Uninstall opens the selected installation's registered uninstaller.
chinesesimplified.CWMMaintenanceHint=修复使用当前安装包和正常安装流程。卸载会打开所选安装已注册的卸载程序。
english.CWMRepair=Repair / reinstall: %1
chinesesimplified.CWMRepair=修复／重新安装：%1
english.CWMInstallCurrentScope=Continue installation for %1
chinesesimplified.CWMInstallCurrentScope=继续为%1安装
english.CWMUninstallOption=Uninstall (%1): %2
chinesesimplified.CWMUninstallOption=卸载（%1）：%2
english.CWMCurrentUser=Current user
chinesesimplified.CWMCurrentUser=当前用户
english.CWMAllUsers=All users
chinesesimplified.CWMAllUsers=所有用户
english.CWMUnknownLocation=Registered location is unavailable
chinesesimplified.CWMUnknownLocation=无法确定注册的安装位置
english.CWMSelectedLocation=Selected installation location
chinesesimplified.CWMSelectedLocation=所选安装位置
english.CWMUninstallButton=&Uninstall...
chinesesimplified.CWMUninstallButton=卸载(&U)...
english.CWMConfirmUninstall=Open the registered uninstaller for this installation?%n%nScope: %1%nLocation: %2%n%nThe system may request permission. Use the uninstaller to confirm or cancel.
chinesesimplified.CWMConfirmUninstall=打开此安装已注册的卸载程序？%n%n范围：%1%n位置：%2%n%n系统可能请求权限。请在卸载程序中确认或取消。
english.CWMInvalidRegistration=The registered uninstaller cannot be safely opened. Repair the installation or use Windows Installed apps.
chinesesimplified.CWMInvalidRegistration=无法安全打开已注册的卸载程序。请修复安装，或使用 Windows“已安装的应用”。
english.CWMRegistrationChanged=The installation registration changed. Close and reopen this setup before trying again.
chinesesimplified.CWMRegistrationChanged=安装注册已发生变化。请关闭并重新打开安装器后再试。
english.CWMMissingUninstaller=The registered uninstaller or installation folder is missing. Repair the installation or use Windows Installed apps.
chinesesimplified.CWMMissingUninstaller=已注册的卸载程序或安装目录不存在。请修复安装，或使用 Windows“已安装的应用”。
english.CWMStartFailed=The uninstaller could not be started (system code %1). No installation changes were made by this setup.
chinesesimplified.CWMStartFailed=无法启动卸载程序（系统代码 %1）。当前安装器未修改安装内容。
english.CWMCancelled=Uninstall was cancelled. Choose an action or close this setup.
chinesesimplified.CWMCancelled=卸载已取消。可以重新选择操作，或关闭安装器。
english.CWMUninstallFailed=The uninstaller returned code %1, or the installation remains registered. This setup will not continue installing automatically.
chinesesimplified.CWMUninstallFailed=卸载程序返回代码 %1，或安装注册仍然存在。当前安装器不会自动继续安装。
english.CWMInstallBlocked=Uninstall has completed. Close this setup; installation will not continue.
chinesesimplified.CWMInstallBlocked=卸载已完成。请关闭安装器，安装流程不会继续。

[Code]
type
  TCWMRegistration = record
    RootKey: Integer;
    ScopeName, AppPath, UninstallCommand, Uninstaller: String;
  end;

var
  CWMPage: TInputOptionWizardPage;
  CWMLocationLabel: TNewStaticText;
  CWMLocation: TMemo;
  CWMRegistrations: array[0..3] of TCWMRegistration;
  CWMRegistrationCount: Integer;
  CWMRepairPath: String;
  CWMBusy, CWMUninstalled: Boolean;

function CWMUninstallKey: String;
begin
  // 仅还原 Inno 的 {{ 转义一次；保留真实 AppId 的右花括号，不猜测或模糊匹配 GUID。
  Result := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\' +
    ExpandConstant('{#SetupSetting("AppId")}') + '_is1';
end;

function CWMIsLocalAbsolutePath(const Value: String): Boolean;
var Drive: String;
begin
  Result := False;
  if Length(Value) < 4 then exit;
  Drive := Uppercase(Copy(Value, 1, 1));
  Result := (Drive >= 'A') and (Drive <= 'Z') and
    (Copy(Value, 2, 2) = ':\') and (Pos('"', Value) = 0) and
    (Pos(#13, Value) = 0) and (Pos(#10, Value) = 0) and (Pos(#0, Value) = 0);
end;

function CWMNormalizePath(const Value: String): String;
begin
  Result := '';
  if not CWMIsLocalAbsolutePath(Value) then exit;
  try
    Result := RemoveBackslashUnlessRoot(ExpandFileName(Value));
  except
    Result := '';
  end;
  // 根目录不是可接受的本产品安装目录。
  if Length(Result) <= 3 then Result := '';
end;

function CWMParseUninstaller(const Command, RegisteredPath: String; var Executable: String): Boolean;
var Name, Digits, Candidate, Directory: String; I: Integer;
begin
  Result := False;
  Executable := '';
  Candidate := Trim(Command);
  if Candidate = '' then exit;
  if Copy(Candidate, 1, 1) = '"' then begin
    // 标准 Inno UninstallString 只有带引号的可执行路径，不接受尾参或 shell 命令。
    if (Length(Candidate) < 3) or (Copy(Candidate, Length(Candidate), 1) <> '"') then exit;
    Candidate := Copy(Candidate, 2, Length(Candidate) - 2);
  end;
  Candidate := CWMNormalizePath(Candidate);
  Directory := CWMNormalizePath(RegisteredPath);
  if (Candidate = '') or (Directory = '') then exit;
  if CompareText(RemoveBackslashUnlessRoot(ExtractFileDir(Candidate)), Directory) <> 0 then exit;
  Name := Lowercase(ExtractFileName(Candidate));
  if (Copy(Name, 1, 5) <> 'unins') or (ExtractFileExt(Name) <> '.exe') then exit;
  Digits := Copy(Name, 6, Length(Name) - 9);
  if Digits = '' then exit;
  for I := 1 to Length(Digits) do
    if (Digits[I] < '0') or (Digits[I] > '9') then exit;
  Executable := Candidate;
  Result := True;
end;

function CWMReadRegistration(RootKey: Integer; const ScopeName: String; var Entry: TCWMRegistration): Boolean;
var Key, Location, InnoPath: String;
begin
  Result := False;
  Entry.RootKey := RootKey;
  Entry.ScopeName := ScopeName;
  Entry.AppPath := '';
  Entry.UninstallCommand := '';
  Entry.Uninstaller := '';
  // 所有开发夹具在编译时移除真实注册探测，不能由命令行重新启用。
  #ifndef CodeWFSetupFixture
    try
      Key := CWMUninstallKey;
      if not RegKeyExists(RootKey, Key) then exit;
      Result := True;
      RegQueryStringValue(RootKey, Key, 'UninstallString', Entry.UninstallCommand);
      RegQueryStringValue(RootKey, Key, 'InstallLocation', Location);
      RegQueryStringValue(RootKey, Key, 'Inno Setup: App Path', InnoPath);
      Entry.AppPath := CWMNormalizePath(InnoPath);
      if Entry.AppPath = '' then Entry.AppPath := CWMNormalizePath(Location);
      // 非空位置异常或两个位置不一致时只允许修复，不启动卸载器。
      if (InnoPath <> '') and (CWMNormalizePath(InnoPath) = '') then exit;
      if (Location <> '') and ((CWMNormalizePath(Location) = '') or
        (CompareText(CWMNormalizePath(Location), Entry.AppPath) <> 0)) then exit;
      CWMParseUninstaller(Entry.UninstallCommand, Entry.AppPath, Entry.Uninstaller);
    except
      Log('CodeWF maintenance: registration could not be read.');
    end;
  #endif
end;

procedure CWMAddRegistration(RootKey: Integer; const ScopeName: String; CurrentScope: Boolean);
var Entry: TCWMRegistration; I: Integer;
begin
  if CWMRegistrationCount >= 4 then exit;
  if not CWMReadRegistration(RootKey, ScopeName, Entry) then exit;
  // 32/64 位视图可能指向同一卸载器；同一作用域不重复展示。
  for I := 0 to CWMRegistrationCount - 1 do
    if (Entry.ScopeName = CWMRegistrations[I].ScopeName) and
      (Entry.AppPath <> '') and (CompareText(Entry.AppPath, CWMRegistrations[I].AppPath) = 0) and
      (CompareText(Entry.UninstallCommand, CWMRegistrations[I].UninstallCommand) = 0) then exit;
  CWMRegistrations[CWMRegistrationCount] := Entry;
  CWMRegistrationCount := CWMRegistrationCount + 1;
  if CurrentScope and (CWMRepairPath = '') and (Entry.AppPath <> '') then
    CWMRepairPath := Entry.AppPath;
end;

procedure CWMReadScope(MachineScope, CurrentScope: Boolean);
var ScopeName: String;
begin
  if MachineScope then ScopeName := CustomMessage('CWMAllUsers')
  else ScopeName := CustomMessage('CWMCurrentUser');
  if IsWin64 and Is64BitInstallMode then begin
    if MachineScope then CWMAddRegistration(HKLM64, ScopeName, CurrentScope)
    else CWMAddRegistration(HKCU64, ScopeName, CurrentScope);
  end;
  if MachineScope then CWMAddRegistration(HKLM32, ScopeName, CurrentScope)
  else CWMAddRegistration(HKCU32, ScopeName, CurrentScope);
  if IsWin64 and not Is64BitInstallMode then begin
    if MachineScope then CWMAddRegistration(HKLM64, ScopeName, CurrentScope)
    else CWMAddRegistration(HKCU64, ScopeName, CurrentScope);
  end;
end;

function CWMDisplayPath(const Value: String): String;
begin
  if Value = '' then Result := CustomMessage('CWMUnknownLocation')
  else Result := Value;
end;

procedure CWMSelectionChanged(Sender: TObject);
begin
  if (CWMPage <> nil) and (WizardForm.CurPageID = CWMPage.ID) then begin
    if CWMPage.SelectedValueIndex > 0 then
      WizardForm.NextButton.Caption := CustomMessage('CWMUninstallButton')
    else WizardForm.NextButton.Caption := SetupMessage(msgButtonNext);
    if CWMLocation <> nil then begin
      if CWMPage.SelectedValueIndex > 0 then
        CWMLocation.Text := CWMDisplayPath(CWMRegistrations[CWMPage.SelectedValueIndex-1].AppPath)
      else CWMLocation.Text := CWMDisplayPath(CWMRepairPath);
    end;
  end;
end;

procedure CWMCreatePage;
var I: Integer; ScopeName: String;
begin
  if CWMRegistrationCount = 0 then exit;
  CWMPage := CreateInputOptionPage(wpWelcome, CustomMessage('CWMMaintenance'),
    CustomMessage('CWMMaintenanceDescription'), CustomMessage('CWMMaintenanceHint'), True, False);
  if CWMRepairPath <> '' then
    CWMPage.Add(FmtMessage(CustomMessage('CWMRepair'), [ExtractFileName(CWMRepairPath)]))
  else begin
    if IsAdminInstallMode then ScopeName := CustomMessage('CWMAllUsers')
    else ScopeName := CustomMessage('CWMCurrentUser');
    CWMPage.Add(FmtMessage(CustomMessage('CWMInstallCurrentScope'), [ScopeName]));
  end;
  for I := 0 to CWMRegistrationCount - 1 do
    CWMPage.Add(FmtMessage(CustomMessage('CWMUninstallOption'), [CWMRegistrations[I].ScopeName, ExtractFileName(CWMRegistrations[I].AppPath)]));
  CWMPage.SelectedValueIndex := 0;
  CWMPage.CheckListBox.OnClickCheck := @CWMSelectionChanged;
  { 完整路径单独展示并可复制，避免长路径挤出操作名称和作用域。 }
  CWMLocationLabel := TNewStaticText.Create(WizardForm);
  CWMLocationLabel.Parent := CWMPage.Surface;
  CWMLocationLabel.Caption := CustomMessage('CWMSelectedLocation');
  CWMLocation := TMemo.Create(WizardForm);
  CWMLocation.Parent := CWMPage.Surface;
  CWMLocation.ReadOnly := True;
  CWMLocation.WordWrap := True;
  CWMLocation.ScrollBars := ssVertical;
end;

<event('InitializeWizard')>
procedure CWMInitializeWizard;
begin
  if WizardSilent then exit;
  CWMReadScope(IsAdminInstallMode, True);
  CWMReadScope(not IsAdminInstallMode, False);
  CWMCreatePage;
end;

function CWMStartUninstaller(const Executable: String; var ResultCode: Integer): Boolean;
begin
  Result := False;
  ResultCode := 5;
  // 夹具绝无外部卸载启动路径；生产只把已验证文件名交给系统，不传静默或任意参数。
  #ifndef CodeWFSetupFixture
    Result := ShellExec('open', Executable, '', ExtractFileDir(Executable),
      SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
  #endif
end;

function CWMRegistrationRemains(RootKey: Integer): Boolean;
begin
  Result := True;
  #ifndef CodeWFSetupFixture
    Result := RegKeyExists(RootKey, CWMUninstallKey);
  #endif
end;

function CWMUninstallSucceeded(Started: Boolean; ResultCode: Integer; RegistrationRemains: Boolean): Boolean;
begin
  Result := Started and (ResultCode = 0) and not RegistrationRemains;
end;

procedure CWMUninstallSelected(Index: Integer);
var Previous, Current: TCWMRegistration; Started: Boolean; ResultCode: Integer;
begin
  if (Index < 0) or (Index >= CWMRegistrationCount) or CWMBusy or CWMUninstalled then exit;
  Previous := CWMRegistrations[Index];
  if not CWMReadRegistration(Previous.RootKey, Previous.ScopeName, Current) then begin
    MsgBox(CustomMessage('CWMRegistrationChanged'), mbError, MB_OK); exit;
  end;
  if (CompareText(Current.AppPath, Previous.AppPath) <> 0) or
    (Current.UninstallCommand <> Previous.UninstallCommand) then begin
    MsgBox(CustomMessage('CWMRegistrationChanged'), mbError, MB_OK); exit;
  end;
  if Current.Uninstaller = '' then begin
    MsgBox(CustomMessage('CWMInvalidRegistration'), mbError, MB_OK); exit;
  end;
  if not DirExists(Current.AppPath) or not FileExists(Current.Uninstaller) then begin
    MsgBox(CustomMessage('CWMMissingUninstaller'), mbError, MB_OK); exit;
  end;
  if MsgBox(FmtMessage(CustomMessage('CWMConfirmUninstall'), [Current.ScopeName, Current.AppPath]), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) <> IDYES then exit;
  CWMBusy := True;
  WizardForm.NextButton.Enabled := False;
  WizardForm.BackButton.Enabled := False;
  WizardForm.CancelButton.Enabled := False;
  try
    try
      Started := CWMStartUninstaller(Current.Uninstaller, ResultCode);
      if not Started then begin
        if ResultCode = 1223 then MsgBox(CustomMessage('CWMCancelled'), mbInformation, MB_OK)
        else MsgBox(FmtMessage(CustomMessage('CWMStartFailed'), [IntToStr(ResultCode)]), mbError, MB_OK);
      end else if CWMUninstallSucceeded(True, ResultCode, CWMRegistrationRemains(Current.RootKey)) then
        CWMUninstalled := True
      else if ResultCode = 2 then MsgBox(CustomMessage('CWMCancelled'), mbInformation, MB_OK)
      else MsgBox(FmtMessage(CustomMessage('CWMUninstallFailed'), [IntToStr(ResultCode)]), mbError, MB_OK);
    except
      MsgBox(CustomMessage('CWMInvalidRegistration'), mbError, MB_OK);
    end;
  finally
    CWMBusy := False;
    WizardForm.NextButton.Enabled := not CWMUninstalled;
    WizardForm.BackButton.Enabled := not CWMUninstalled;
    WizardForm.CancelButton.Enabled := True;
  end;
  if CWMUninstalled then WizardForm.Close;
end;

<event('CurPageChanged')>
procedure CWMMaintenancePageChanged(PageID: Integer);
begin
  if (CWMPage <> nil) and (PageID = CWMPage.ID) then begin
    { 自定义页面使用扩展后的实际内容宽度，长安装路径由原生列表处理。 }
    CWMPage.SubCaptionLabel.Width := CWMPage.SurfaceWidth;
    CWMPage.SubCaptionLabel.Height := ScaleY(54);
    CWMPage.CheckListBox.SetBounds(0, ScaleY(66), CWMPage.SurfaceWidth, ScaleY(100));
    CWMLocationLabel.SetBounds(0, ScaleY(180), CWMPage.SurfaceWidth, ScaleY(24));
    CWMLocation.SetBounds(0, ScaleY(210), CWMPage.SurfaceWidth, CWMPage.SurfaceHeight-ScaleY(210));
    #ifdef CodeWFSetupVisualEngine
      CWStepLabels[0].Caption := '↺    ' + CustomMessage('CWMMaintenance');
      CWFooter.Caption := CustomMessage('CWMMaintenance') + '   ·   {#CodeWFSetupProductName}';
    #endif
    CWMSelectionChanged(nil);
  end;
end;

<event('NextButtonClick')>
function CWMMaintenanceNext(PageID: Integer): Boolean;
begin
  Result := not CWMBusy and not CWMUninstalled;
  if not Result or (CWMPage = nil) then exit;
  if PageID <> CWMPage.ID then exit;
  if (CWMPage.SelectedValueIndex < 0) or
    (CWMPage.SelectedValueIndex > CWMRegistrationCount) then begin Result := False; exit; end;
  if CWMPage.SelectedValueIndex = 0 then begin
    if CWMRepairPath <> '' then WizardForm.DirEdit.Text := CWMRepairPath;
    // 后续仍由原生安装流程复制/注册/执行，不新增第二份 Run 或 Registry 项。
  end else begin
    Result := False;
    CWMUninstallSelected(CWMPage.SelectedValueIndex - 1);
  end;
end;

<event('CancelButtonClick')>
procedure CWMMaintenanceCancel(PageID: Integer; var Cancel, Confirm: Boolean);
begin
  if CWMBusy then begin Cancel := False; Confirm := False; end
  else if CWMUninstalled then begin Cancel := True; Confirm := False; end;
end;

<event('PrepareToInstall')>
function CWMMaintenancePrepare(var NeedsRestart: Boolean): String;
begin
  // 即使其他事件代码尝试跳页，成功卸载后也不能进入安装阶段。
  if CWMUninstalled or CWMBusy then Result := CustomMessage('CWMInstallBlocked')
  else Result := '';
end;
#endif
