; .NET 紫异形安装器。保留 Inno 原生页面、安装引擎、键盘与辅助功能。
; 每个仓库提交完整快照，安装构建不读取其他仓库或下载外部皮肤。
#define CodeWFSetupVisualEngine
#ifndef CodeWFSetupProductName
  #define CodeWFSetupProductName SetupSetting("AppName")
#endif
#ifndef CodeWFSetupVersion
  #define CodeWFSetupVersion AppVersion
#endif
#ifndef CodeWFSetupAssetPath
  #define CodeWFSetupAssetPath "installer"
#endif

[Setup]
#if VER >= EncodeVer(7,0,0)
SetupArchitecture=x86
#endif
DisableWelcomePage=no
DisableReadyPage=no
DisableFinishedPage=no
WizardSizePercent=100
WizardResizable=no
ShowLanguageDialog=auto

[Files]
Source: "{#CodeWFSetupAssetPath}\frame-dark.bmp"; Flags: dontcopy
Source: "{#CodeWFSetupAssetPath}\frame-light.bmp"; Flags: dontcopy
Source: "{#CodeWFSetupAssetPath}\reactor-dark.bmp"; Flags: dontcopy
Source: "{#CodeWFSetupAssetPath}\reactor-light.bmp"; Flags: dontcopy
Source: "{#CodeWFSetupAssetPath}\core-dark.bmp"; Flags: dontcopy
Source: "{#CodeWFSetupAssetPath}\core-light.bmp"; Flags: dontcopy

#include "CodeWF.Setup.Messages.iss"

[Code]
type
  TCWPoint = record X, Y: Integer; end;
  TCWRect = record Left, Top, Right, Bottom: Integer; end;
  TCWCharFormat = array[0..22] of LongWord;
  TCWDrawItem = record
    CtlType, CtlID, ItemID, ItemAction, ItemState: LongWord;
    ItemWindow, DC: LongWord;
    Bounds: TCWRect;
    ItemData: LongWord;
  end;

function CWCreatePolygonRegion(var Points: TCWPoint; Count, Mode: Integer): LongWord;
  external 'CreatePolygonRgn@gdi32.dll stdcall';
function CWSetWindowRegion(Window, Region: LongWord; Redraw: Boolean): Integer;
  external 'SetWindowRgn@user32.dll stdcall';
function CWDeleteObject(Handle: LongWord): Boolean;
  external 'DeleteObject@gdi32.dll stdcall';
function CWSetWindowTheme(Window: LongWord; Application, IDs: String): Integer;
  external 'SetWindowTheme@uxtheme.dll stdcall';
function CWIsIconic(Window: LongWord): Boolean;
  external 'IsIconic@user32.dll stdcall';
function CWIsWindow(Window: LongWord): Boolean;
  external 'IsWindow@user32.dll stdcall';
function CWSendCharFormat(Window, Message, Flags: LongWord; var Format: TCWCharFormat): Longint;
  external 'SendMessageW@user32.dll stdcall';
function CWGetSystemMetrics(Index: Integer): Integer;
  external 'GetSystemMetrics@user32.dll stdcall';
function CWSetTimer(Window, ID, Interval, Callback: LongWord): LongWord;
  external 'SetTimer@user32.dll stdcall';
function CWKillTimer(Window, ID: LongWord): Boolean;
  external 'KillTimer@user32.dll stdcall';
function CWReleaseCapture: Boolean;
  external 'ReleaseCapture@user32.dll stdcall';
function CWPostMessage(Window, Message, WParam, LParam: LongWord): Boolean;
  external 'PostMessageW@user32.dll stdcall';
function CWGetWindowLong(Window: LongWord; Index: Integer): Longint;
  external 'GetWindowLongW@user32.dll stdcall';
function CWGetWindowRect(Window: LongWord; var Bounds: TCWRect): Boolean;
  external 'GetWindowRect@user32.dll stdcall';
function CWSetWindowLong(Window: LongWord; Index: Integer; Value: Longint): Longint;
  external 'SetWindowLongW@user32.dll stdcall';
function CWCallWindowProc(Previous, Window, Message, WParam, LParam: LongWord): Longint;
  external 'CallWindowProcW@user32.dll stdcall';
procedure CWCopyMemory(var Target: TCWDrawItem; Source, Size: LongWord);
  external 'RtlMoveMemory@kernel32.dll stdcall';
function CWFillRect(DC: LongWord; var Bounds: TCWRect; Brush: LongWord): Integer;
  external 'FillRect@user32.dll stdcall';
function CWCreateSolidBrush(Color: LongWord): LongWord;
  external 'CreateSolidBrush@gdi32.dll stdcall';
function CWSetTextColor(DC, Color: LongWord): LongWord;
  external 'SetTextColor@gdi32.dll stdcall';
function CWSetBkColor(DC, Color: LongWord): LongWord;
  external 'SetBkColor@gdi32.dll stdcall';
function CWSetBkMode(DC: LongWord; Mode: Integer): Integer;
  external 'SetBkMode@gdi32.dll stdcall';
function CWDrawText(DC: LongWord; Text: String; Length: Integer; var Bounds: TCWRect; Format: LongWord): Integer;
  external 'DrawTextW@user32.dll stdcall';
function CWDrawFocusRect(DC: LongWord; var Bounds: TCWRect): Boolean;
  external 'DrawFocusRect@user32.dll stdcall';
function CWStretchBlt(Target: LongWord; X, Y, Width, Height: Integer; Source: LongWord; SourceX, SourceY, SourceWidth, SourceHeight: Integer; Mode: LongWord): Boolean;
  external 'StretchBlt@gdi32.dll stdcall';
function CWSetStretchBltMode(DC: LongWord; Mode: Integer): Integer;
  external 'SetStretchBltMode@gdi32.dll stdcall';
function CWShowWindow(Window: LongWord; Command: Integer): Boolean;
  external 'ShowWindow@user32.dll stdcall';
function CWInvalidateRect(Window, Bounds: LongWord; Erase: Boolean): Boolean;
  external 'InvalidateRect@user32.dll stdcall';
function CWSystemParametersInfo(Action, Parameter: LongWord; var Enabled: LongWord; Update: LongWord): Boolean;
  external 'SystemParametersInfoW@user32.dll stdcall';
function CWSin(Value: Double): Double;
  external 'sin@msvcrt.dll cdecl';
function CWCos(Value: Double): Double;
  external 'cos@msvcrt.dll cdecl';

var
  CWBackground, CWReactor: TBitmapImage;
  CWReactorBase, CWCore: TBitmap;
  CWTitle, CWHeading, CWDescription, CWProduct, CWVersionLabel, CWFooter, CWEngineLabel, CWPercentLabel: TLabel;
  CWStepLabels: array[0..5] of TLabel;
  CWWelcomeLines: array[0..1] of TPanel;
  CWThemeButton, CWMinimizeButton, CWCloseButton: TNewButton;
  CWReduceMotion: TNewCheckBox;
  CWTimer, CWPreviousProc, CWBackgroundBrush: LongWord;
  CWPageHandles, CWPageProcs: array[0..63] of LongWord;
  CWButtonHandles, CWButtonProcs: array[0..5] of LongWord;
  CWButtonCount: Integer;
  CWPageHookCount: Integer;
  CWFrame, CWProgress, CWStep, CWScalePercent, CWCompletedAt, CWThemeMode, CWThemeTicks: Integer;
  CWDark, CWInstalled, CWInitialized, CWButtonsStyled: Boolean;
  CWText, CWMuted, CWSurface, CWField: TColor;

function CWSize(Value: Integer): Integer;
begin
  Result := (ScaleX(Value) * CWScalePercent) div 100;
end;

#include "CodeWF.Setup.Progress.iss"

procedure CWBounds(Control: TControl; X, Y, Width, Height: Integer);
begin
  Control.SetBounds(CWSize(X), CWSize(Y), CWSize(Width), CWSize(Height));
end;

function CWLabel(Parent: TWinControl; X, Y, Width, Height, Size: Integer; Caption: String): TLabel;
begin
  Result := TLabel.Create(WizardForm);
  Result.Transparent := True;
  Result.Parent := Parent;
  Result.AutoSize := False;
  Result.WordWrap := True;
  Result.Caption := Caption;
  Result.Font.Name := 'Segoe UI';
  Result.Font.Size := (Size * CWScalePercent) div 100;
  CWBounds(Result, X, Y, Width, Height);
end;

procedure CWApplyRegion;
var Points: array[0..15] of TCWPoint; Region: LongWord;
begin
  Points[0].X:=324; Points[0].Y:=0; Points[1].X:=818; Points[1].Y:=0;
  Points[2].X:=840; Points[2].Y:=18; Points[3].X:=1088; Points[3].Y:=18;
  Points[4].X:=1136; Points[4].Y:=66; Points[5].X:=1136; Points[5].Y:=648;
  Points[6].X:=1096; Points[6].Y:=688; Points[7].X:=682; Points[7].Y:=688;
  Points[8].X:=658; Points[8].Y:=672; Points[9].X:=24; Points[9].Y:=672;
  Points[10].X:=0; Points[10].Y:=648; Points[11].X:=0; Points[11].Y:=132;
  Points[12].X:=38; Points[12].Y:=94; Points[13].X:=240; Points[13].Y:=94;
  Points[14].X:=300; Points[14].Y:=154; Points[15].X:=300; Points[15].Y:=24;
  for CWFrame := 0 to 15 do begin
    Points[CWFrame].X := CWSize(Points[CWFrame].X);
    Points[CWFrame].Y := CWSize(Points[CWFrame].Y);
  end;
  Region := CWCreatePolygonRegion(Points[0], 16, 1);
  if (Region <> 0) and (CWSetWindowRegion(WizardForm.Handle, Region, True) = 0) then
    CWDeleteObject(Region);
  CWFrame := 0;
end;

procedure CWDrawCore;
var I, X, Y, Radius, CenterX, CenterY, OrbitIndex, CoreSize: Integer; Angle, Phase: Extended;
    Canvas: TCanvas; LabelText: String;
begin
  if not CWInitialized then exit;
  CWReactor.Bitmap.Width := CWSize(220);
  CWReactor.Bitmap.Height := CWSize(180);
  Canvas := CWReactor.Bitmap.Canvas;
  CWSetStretchBltMode(Canvas.Handle,4);
  CWStretchBlt(Canvas.Handle, 0, 0, CWSize(220), CWSize(180), CWReactorBase.Canvas.Handle, 0, 0, CWReactorBase.Width, CWReactorBase.Height, $00CC0020);
  CenterX := CWSize(110); CenterY := CWSize(90);
  Canvas.Brush.Style := bsClear;
  for I := 0 to 47 do begin
    Angle := I * 2.39996 + CWFrame * 0.018;
    Phase := ((I * 31 + CWFrame * 3) mod 100) / 100.0;
    if WizardForm.CurPageID = wpInstalling then Radius := 100 - Round(Phase * 58)
    else Radius := 82 + (I mod 17);
    X := CenterX + Round(CWSin(Angle) * CWSize(Radius));
    Y := CenterY + Round(CWCos(Angle) * CWSize(Radius) * 0.73);
    Canvas.Pixels[X, Y] := $F4D9DD;
    if I mod 8 = 0 then begin Canvas.Pixels[X+1,Y] := $F4D9DD; Canvas.Pixels[X,Y+1] := $F4D9DD; end;
  end;
  if (CWProgress > 0) or CWInstalled then begin
    Angle := CWProgress / 100.0 * 6.283185 - 1.570796;
    Canvas.Pen.Color := $F5CFCF; Canvas.Pen.Width := CWSize(2);
    if CWInstalled or (CWProgress = 100) then Canvas.Ellipse(CWSize(23),CWSize(3),CWSize(197),CWSize(177))
    else Canvas.Arc(CWSize(23),CWSize(3),CWSize(197),CWSize(177),
      CenterX + Round(CWCos(Angle) * CWSize(87)),CenterY + Round(CWSin(Angle) * CWSize(87)),CenterX,CWSize(3));
    Canvas.Pen.Width := 1;
  end;
  Canvas.Pen.Color := $E6BCBA;
  OrbitIndex := CWFrame mod 120;
  X := CenterX + Round(CWSin(OrbitIndex * 0.05236) * CWSize(84));
  Y := CenterY + Round(CWCos(OrbitIndex * 0.05236) * CWSize(84));
  Canvas.Brush.Style := bsSolid; Canvas.Brush.Color := $FCE1E4;
  Canvas.Ellipse(X-CWSize(3),Y-CWSize(3),X+CWSize(3),Y+CWSize(3));
  CoreSize := CWSize(124);
  if (WizardForm.CurPageID = wpInstalling) and not CWReduceMotion.Checked then
    CoreSize := CoreSize + Round(CWSin(CWFrame*0.2)*CWSize(2));
  CWStretchBlt(Canvas.Handle, CenterX-CoreSize div 2, CenterY-CoreSize div 2, CoreSize, CoreSize, CWCore.Canvas.Handle, 0, 0, CWCore.Width, CWCore.Height, $00CC0020);
  if CWInstalled and not CWReduceMotion.Checked and (CWFrame-CWCompletedAt < 24) then begin
    Radius := CWSize(48+(CWFrame-CWCompletedAt)*40 div 24);
    Canvas.Pen.Color := $FADDE9; Canvas.Brush.Style := bsClear;
    Canvas.Ellipse(CenterX-Radius,CenterY-Radius,CenterX+Radius,CenterY+Radius);
  end;
  Canvas.Font.Name := 'Segoe UI'; Canvas.Font.Size := 25;
  Canvas.Font.Color := clWhite;
  if WizardForm.CurPageID = wpInstalling then LabelText := IntToStr(CWProgress) + '%'
  else if CWInstalled then LabelText := '✓' else LabelText := '.NET';
  Canvas.Brush.Style := bsClear;
  Canvas.TextOut((CWReactor.Width-Canvas.TextWidth(LabelText)) div 2,
    (CWReactor.Height-Canvas.TextHeight(LabelText)) div 2, LabelText);
  CWReactor.Invalidate;
end;

procedure CWStyleChildren(Parent: TWinControl);
var I: Integer; Control: TControl;
begin
  for I := 0 to Parent.ControlCount - 1 do begin
    Control := Parent.Controls[I];
    if Control is TWinControl then CWStyleChildren(TWinControl(Control));
    if Control is TNewNotebookPage then TNewNotebookPage(Control).Color := CWSurface;
    if Control is TPanel then TPanel(Control).Color := CWSurface;
    if Control is TNewStaticText then TNewStaticText(Control).Font.Color := CWText;
    if Control is TLabel then TLabel(Control).Font.Color := CWText;
    if Control is TEdit then begin TEdit(Control).Color := CWField; TEdit(Control).Font.Color := CWText; end;
    if Control is TMemo then begin TMemo(Control).Color := CWField; TMemo(Control).Font.Color := CWText; end;
    if Control is TRichEditViewer then begin TRichEditViewer(Control).Color := CWField; TRichEditViewer(Control).Font.Color := CWText; end;
    if Control is TNewCheckListBox then begin TNewCheckListBox(Control).Color := CWSurface; TNewCheckListBox(Control).Font.Color := CWText; end;
    if Control is TNewRadioButton then begin CWSetWindowTheme(TWinControl(Control).Handle,' ',' '); TNewRadioButton(Control).Color := CWSurface; TNewRadioButton(Control).Font.Color := CWText; end;
    if Control is TNewCheckBox then begin CWSetWindowTheme(TWinControl(Control).Handle,' ',' '); TNewCheckBox(Control).Color := CWSurface; TNewCheckBox(Control).Font.Color := CWText; end;
  end;
end;

function CWButtonWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var I: Integer;
begin
  { 对话框焦点管理会重设默认按钮样式；保留自绘低位，同时继续转发原生键盘和点击消息。 }
  if Message = $00F4 then WParam := (WParam and not $F) or $B;
  if Message = $000F then
    CWSetWindowLong(Window, -16, (CWGetWindowLong(Window, -16) and not $F) or $B);
  Result := 0;
  for I := 0 to CWButtonCount-1 do
    if CWButtonHandles[I] = Window then begin
      Result := CWCallWindowProc(CWButtonProcs[I], Window, Message, WParam, LParam); exit;
    end;
end;

procedure CWOwnerDraw(Button: TNewButton);
var I: Integer;
begin
  CWSetWindowLong(Button.Handle, -16, (CWGetWindowLong(Button.Handle, -16) and not $F) or $B);
  for I := 0 to CWButtonCount-1 do if CWButtonHandles[I] = Button.Handle then exit;
  if CWButtonCount > 5 then exit;
  CWButtonHandles[CWButtonCount] := Button.Handle;
  CWButtonProcs[CWButtonCount] := CWSetWindowLong(Button.Handle, -4, CreateCallback(@CWButtonWindowProc));
  CWButtonCount := CWButtonCount+1;
end;

procedure CWApplyTheme;
var Theme: String; I: Integer; Format: TCWCharFormat;
begin
  if CWDark then begin
    Theme := 'dark'; CWSurface := $251016; CWText := $F7F2F5; CWMuted := $CDB6C2; CWField := $2E1821;
  end else begin
    Theme := 'light'; CWSurface := clWhite; CWText := $351226; CWMuted := $705366; CWField := $FCF7FA;
  end;
  CWBackground.Bitmap.LoadFromFile(ExpandConstant('{tmp}\frame-' + Theme + '.bmp'));
  CWReactorBase.LoadFromFile(ExpandConstant('{tmp}\reactor-' + Theme + '.bmp'));
  CWReactor.Bitmap := CWReactorBase;
  CWCore.LoadFromFile(ExpandConstant('{tmp}\core-' + Theme + '.bmp'));
  WizardForm.Color := CWSurface;
  if CWBackgroundBrush <> 0 then CWDeleteObject(CWBackgroundBrush);
  CWBackgroundBrush := CWCreateSolidBrush(CWSurface);
  CWStyleChildren(WizardForm);
  for I := 0 to 22 do Format[I] := 0;
  Format[0] := 92; Format[1] := $40000000; Format[5] := CWText;
  CWSendCharFormat(WizardForm.LicenseMemo.Handle,$0444,4,Format);
  CWReduceMotion.Font.Color := CWMuted;
  CWTitle.Font.Color := CWMuted; CWDescription.Font.Color := CWMuted; CWFooter.Font.Color := CWMuted;
  CWProduct.Font.Color := clWhite; CWVersionLabel.Font.Color := $FFE5E4;
  CWEngineLabel.Font.Color := $FFE5E4;
  for I := 0 to 5 do CWStepLabels[I].Font.Color := $FFE0DC;
  for I := 0 to 1 do begin
    if CWDark then CWWelcomeLines[I].Color := $572C3E else CWWelcomeLines[I].Color := $F9D3DF;
  end;
  CWOwnerDraw(WizardForm.NextButton); CWOwnerDraw(WizardForm.BackButton); CWOwnerDraw(WizardForm.CancelButton);
  CWOwnerDraw(CWThemeButton); CWOwnerDraw(CWMinimizeButton); CWOwnerDraw(CWCloseButton);
  CWDrawCore;
  CWInvalidateRect(WizardForm.Handle, 0, True);
end;

function CWSystemDark: Boolean;
var Light: Cardinal;
begin
  { 只读取 Windows 外观，不修改用户个性化设置。无法读取时使用默认深色。 }
  Result := True;
  if RegQueryDWordValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Themes\Personalize',
    'AppsUseLightTheme', Light) then Result := Light = 0;
end;

procedure CWRefreshAppearance;
var ModeKey: String;
begin
  if CWThemeMode = 0 then begin CWDark := True; ModeKey := 'CWModeDark'; end
  else if CWThemeMode = 1 then begin CWDark := False; ModeKey := 'CWModeLight'; end
  else begin CWDark := CWSystemDark; ModeKey := 'CWModeSystem'; end;
  CWThemeButton.Hint := FmtMessage(CustomMessage('CWAppearance'), [CustomMessage(ModeKey)]);
  CWApplyTheme;
end;

procedure CWThemeClick(Sender: TObject);
begin CWThemeMode := (CWThemeMode + 1) mod 3; CWRefreshAppearance; end;
procedure CWMinimizeClick(Sender: TObject);
begin CWShowWindow(WizardForm.Handle,6); end;
procedure CWCloseClick(Sender: TObject);
begin CWPostMessage(WizardForm.Handle,$0010,0,0); end;

function CWPageWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var I: Integer;
begin
  if (Message = $0135) or (Message = $0138) then begin
    CWSetTextColor(WParam,CWText); CWSetBkColor(WParam,CWSurface);
    Result := CWBackgroundBrush; exit;
  end;
  Result := 0;
  for I := 0 to CWPageHookCount-1 do
    if CWPageHandles[I] = Window then begin
      Result := CWCallWindowProc(CWPageProcs[I],Window,Message,WParam,LParam); exit;
    end;
end;

procedure CWHookPage(Control: TWinControl);
var I: Integer;
begin
  for I := 0 to CWPageHookCount-1 do if CWPageHandles[I] = Control.Handle then exit;
  if CWPageHookCount > 63 then exit;
  CWPageHandles[CWPageHookCount] := Control.Handle;
  CWPageProcs[CWPageHookCount] := CWSetWindowLong(Control.Handle,-4,CreateCallback(@CWPageWindowProc));
  CWPageHookCount := CWPageHookCount+1;
end;

procedure CWLayoutPage;
var I: Integer;
begin
  { 先设置父页面，再设置子页面，避免 Anchors 在父级变化时二次缩小内容。 }
  WizardForm.OuterNotebook.Anchors := [akLeft,akTop];
  WizardForm.InnerNotebook.Anchors := [akLeft,akTop];
  CWBounds(WizardForm.OuterNotebook,337,208,761,370);
  CWBounds(WizardForm.InnerPage,0,0,761,370);
  CWBounds(WizardForm.InnerNotebook,0,0,761,370);
  for I := 0 to WizardForm.InnerNotebook.PageCount-1 do begin
    CWBounds(WizardForm.InnerNotebook.Pages[I],0,0,761,370);
    CWHookPage(WizardForm.InnerNotebook.Pages[I]);
  end;
  CWBounds(WizardForm.WelcomePage,0,0,761,370);
  CWBounds(WizardForm.FinishedPage,0,0,761,370);
  CWHookPage(WizardForm.WelcomePage); CWHookPage(WizardForm.FinishedPage);
  CWBounds(WizardForm.RunList,0,104,730,190);
  CWBounds(WizardForm.LicenseMemo,0,36,761,250); CWBounds(WizardForm.LicenseLabel1,0,0,761,30);
  CWBounds(WizardForm.LicenseAcceptedRadio,0,302,761,24); CWBounds(WizardForm.LicenseNotAcceptedRadio,0,332,761,24);
  CWBounds(WizardForm.SelectDirLabel,0,0,761,32);
  CWBounds(WizardForm.SelectDirBrowseLabel,0,36,761,26); CWBounds(WizardForm.DirEdit,0,78,630,34);
  CWBounds(WizardForm.DirBrowseButton,647,78,114,34); CWBounds(WizardForm.DiskSpaceLabel,0,142,761,35);
  CWBounds(WizardForm.SelectTasksLabel,0,0,761,45); CWBounds(WizardForm.TasksList,0,55,761,285);
  CWBounds(WizardForm.ReadyLabel,0,0,761,40); CWBounds(WizardForm.ReadyMemo,0,48,761,302);
  CWBounds(WizardForm.StatusLabel,0,28,560,40); CWBounds(WizardForm.FilenameLabel,0,82,761,40);
  CWBounds(WizardForm.ProgressGauge,0,142,761,12);
  CWBounds(WizardForm.CancelButton,878,625,96,38); CWBounds(WizardForm.BackButton,768,625,96,38); CWBounds(WizardForm.NextButton,989,625,109,38);
end;

function CWWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var Item: TCWDrawItem; Color, Foreground, Brush: LongWord; Bounds: TCWRect; Caption: String; X, Y: Integer;
begin
  if (Message = $0135) or (Message = $0138) then begin
    if (CWReduceMotion <> nil) and (LParam = CWReduceMotion.Handle) then begin
      CWSetTextColor(WParam,CWMuted); CWSetBkColor(WParam,CWSurface);
      Result := CWBackgroundBrush; exit;
    end;
  end;
  if Message = $0084 then begin
    CWGetWindowRect(Window, Bounds);
    { 屏幕坐标是有符号短整数，兼容位于主屏左侧或上方的显示器。 }
    X := LParam and $FFFF; if X >= $8000 then X := X - $10000;
    Y := (LParam shr 16) and $FFFF; if Y >= $8000 then Y := Y - $10000;
    X := X - Bounds.Left; Y := Y - Bounds.Top;
    if (Y >= 0) and (Y < CWSize(60)) and (X < CWSize(970)) then begin Result := 2; exit; end;
  end;
  if Message = $002B then begin
    CWCopyMemory(Item, LParam, 48);
    if Item.CtlType = 4 then begin
      Caption := ''; Color := CWSurface; Foreground := CWText;
      if Item.ItemWindow = WizardForm.NextButton.Handle then begin
        Caption := WizardForm.NextButton.Caption; Color := $D42B51; Foreground := clWhite;
        if (Item.ItemState and 1) <> 0 then Color := $A51D3E;
      end else if Item.ItemWindow = WizardForm.BackButton.Handle then Caption := WizardForm.BackButton.Caption
      else if Item.ItemWindow = WizardForm.CancelButton.Handle then Caption := WizardForm.CancelButton.Caption
      else if Item.ItemWindow = CWThemeButton.Handle then Caption := '◐'
      else if Item.ItemWindow = CWMinimizeButton.Handle then Caption := '−'
      else if Item.ItemWindow = CWCloseButton.Handle then Caption := '×';
      if Caption <> '' then begin
        if (Item.ItemState and 4) <> 0 then begin Color := CWField; Foreground := CWMuted; end;
        Bounds := Item.Bounds;
        Brush := CWCreateSolidBrush(Color); CWFillRect(Item.DC, Bounds, Brush); CWDeleteObject(Brush);
        CWSetTextColor(Item.DC, Foreground); CWSetBkMode(Item.DC, 1);
        CWDrawText(Item.DC, Caption, -1, Bounds, $25);
        if (Item.ItemState and $10) <> 0 then begin
          Bounds.Left := Bounds.Left + 4; Bounds.Top := Bounds.Top + 4;
          Bounds.Right := Bounds.Right - 4; Bounds.Bottom := Bounds.Bottom - 4;
          CWDrawFocusRect(Item.DC, Bounds);
        end;
        Result := 1; exit;
      end;
    end;
  end;
  Result := CWCallWindowProc(CWPreviousProc, Window, Message, WParam, LParam);
end;

procedure CWTimerProc(Window, Message, ID, Time: LongWord);
begin
  if not CWInitialized then exit;
  if not WizardForm.Visible or CWIsIconic(WizardForm.Handle) then exit;
  CWThemeTicks := CWThemeTicks + 1;
  if (CWThemeMode = 2) and (CWThemeTicks mod 20 = 0) and (CWDark <> CWSystemDark) then CWRefreshAppearance;
  if not CWButtonsStyled then begin
    CWLayoutPage;
    CWApplyTheme;
    CWOwnerDraw(WizardForm.NextButton); CWOwnerDraw(WizardForm.BackButton); CWOwnerDraw(WizardForm.CancelButton);
    CWOwnerDraw(CWThemeButton); CWOwnerDraw(CWMinimizeButton); CWOwnerDraw(CWCloseButton);
    CWButtonsStyled := True;
  end;
  if not CWReduceMotion.Checked then begin CWFrame := CWFrame + 1; CWDrawCore; end;
end;

<event('CurPageChanged')>
procedure CWPageChanged(PageID: Integer);
var I: Integer; Keys: array[0..5] of String;
begin
  if not CWInitialized then exit;
  CWButtonsStyled := False;
  CWLayoutPage;
  CWStep := 0;
  if PageID = wpLicense then CWStep := 1
  else if PageID = wpSelectDir then CWStep := 2
  else if (PageID = wpSelectTasks) or (PageID = wpSelectComponents) or (PageID = wpSelectProgramGroup) then CWStep := 3
  else if (PageID = wpReady) or (PageID = wpPreparing) or (PageID = wpInstalling) then CWStep := 4
  else if PageID = wpFinished then begin CWStep := 5; CWInstalled := True; CWCompletedAt := CWFrame; end;
  Keys[0]:='CWWelcome'; Keys[1]:='CWLicense'; Keys[2]:='CWLocation';
  Keys[3]:='CWOptions'; Keys[4]:='CWReady'; Keys[5]:='CWComplete';
  for I := 0 to 5 do begin
    CWStepLabels[I].Caption := IntToStr(I+1) + '    ' + CustomMessage(Keys[I]);
    CWStepLabels[I].Font.Style := [];
    if I = CWStep then begin CWStepLabels[I].Font.Color := clWhite; CWStepLabels[I].Font.Style := [fsBold]; end;
  end;
  if PageID = wpWelcome then begin
    CWHeading.Caption := '{#CodeWFSetupProductName}'; CWDescription.Caption := CustomMessage('CWWelcomeHint');
  end else if PageID = wpFinished then begin
    CWHeading.Caption := CustomMessage('CWFinished'); CWDescription.Caption := WizardForm.FinishedLabel.Caption;
  end else begin
    CWHeading.Caption := WizardForm.PageNameLabel.Caption; CWDescription.Caption := WizardForm.PageDescriptionLabel.Caption;
  end;
  CWFooter.Caption := IntToStr(CWStep+1) + ' / 6   ·   {#CodeWFSetupProductName}';
  CWDrawCore;
end;

<event('CurInstallProgressChanged')>
procedure CWInstallProgressChanged(Current, Maximum: Integer);
begin
  if Maximum > 0 then CWProgress := Round(Current * 100.0 / Maximum);
  if CWPercentLabel <> nil then CWPercentLabel.Caption := IntToStr(CWProgress) + '%';
  if (WizardForm <> nil) and CWInitialized then CWInvalidateRect(WizardForm.ProgressGauge.Handle,0,False);
  CWDrawCore;
end;

<event('InitializeWizard')>
procedure CWInitializeWizard;
var I: Integer; Enabled: LongWord; LabelControl: TLabel;
begin
  if WizardSilent then exit;
  CWScalePercent := 100;
  if ScaleX(1136) > CWGetSystemMetrics(0)-64 then CWScalePercent := ((CWGetSystemMetrics(0)-64)*100) div ScaleX(1136);
  if (ScaleX(688)*CWScalePercent) div 100 > CWGetSystemMetrics(1)-100 then CWScalePercent := ((CWGetSystemMetrics(1)-100)*100) div ScaleX(688);
  WizardForm.BorderStyle := bsNone;
  WizardForm.ClientWidth := CWSize(1136); WizardForm.ClientHeight := CWSize(688);
  WizardForm.Position := poScreenCenter;
  WizardForm.Font.Name := 'Segoe UI'; WizardForm.Font.Size := 10;
  CWApplyRegion;
  ExtractTemporaryFile('frame-dark.bmp'); ExtractTemporaryFile('frame-light.bmp');
  ExtractTemporaryFile('reactor-dark.bmp'); ExtractTemporaryFile('reactor-light.bmp');
  ExtractTemporaryFile('core-dark.bmp'); ExtractTemporaryFile('core-light.bmp');
  WizardForm.MainPanel.Visible := False; WizardForm.Bevel.Visible := False;
  WizardForm.Bevel1.Visible := False; WizardForm.BeveledLabel.Visible := False;
  WizardForm.WizardBitmapImage.Visible := False; WizardForm.WizardBitmapImage2.Visible := False;
  CWBackground := TBitmapImage.Create(WizardForm); CWBackground.Parent := WizardForm;
  CWBounds(CWBackground,0,0,1136,688); CWBackground.Stretch := True; CWBackground.SendToBack;
  CWReactorBase := TBitmap.Create;
  CWCore := TBitmap.Create;
  CWReactor := TBitmapImage.Create(WizardForm); CWReactor.Parent := WizardForm;
  CWBounds(CWReactor,40,104,220,180);
  CWTitle := CWLabel(WizardForm,338,24,600,24,9,'{#CodeWFSetupProductName} ' + CustomMessage('CWInstall'));
  CWProduct := CWLabel(WizardForm,28,294,250,48,12,'{#CodeWFSetupProductName}'); CWProduct.Font.Style := [fsBold];
  CWVersionLabel := CWLabel(WizardForm,28,342,245,22,8,'{#CodeWFSetupVersion}   ·   WINDOWS X64');
  for I := 0 to 5 do CWStepLabels[I] := CWLabel(WizardForm,28,385+35*I,250,28,10,'');
  CWEngineLabel := CWLabel(WizardForm,28,637,250,18,8,CustomMessage('CWNative')); CWEngineLabel.Font.Color := clWhite;
  CWHeading := CWLabel(WizardForm,337,96,760,46,23,''); CWHeading.Font.Style := [fsBold];
  CWDescription := CWLabel(WizardForm,337,147,760,48,10,'');
  CWFooter := CWLabel(WizardForm,337,635,500,20,8,'');
  CWBounds(WizardForm.OuterNotebook,337,208,761,370);
  CWBounds(WizardForm.InnerNotebook,0,0,761,370);
  CWBounds(WizardForm.InnerPage,0,0,761,370);
  for I := 0 to WizardForm.InnerNotebook.PageCount-1 do CWBounds(WizardForm.InnerNotebook.Pages[I],0,0,761,370);
  WizardForm.WelcomeLabel1.Visible := False; WizardForm.WelcomeLabel2.Visible := False;
  WizardForm.FinishedHeadingLabel.Visible := False; WizardForm.FinishedLabel.Visible := False;
  LabelControl := CWLabel(WizardForm.WelcomePage,0,0,700,22,9,CustomMessage('CWVersion')); LabelControl.Font.Color := $E0A3B9;
  LabelControl := CWLabel(WizardForm.WelcomePage,0,34,700,72,34,'{#CodeWFSetupVersion}');
  for I := 0 to 1 do begin
    CWWelcomeLines[I] := TPanel.Create(WizardForm);
    CWWelcomeLines[I].Parent := WizardForm.WelcomePage;
    CWWelcomeLines[I].BevelOuter := bvNone;
    CWBounds(CWWelcomeLines[I],0,I*110,761,1);
    CWWelcomeLines[I].SendToBack;
  end;
  LabelControl := CWLabel(WizardForm.WelcomePage,0,128,700,26,10,CustomMessage('CWOffline') + '    ·    MIT LICENSE    ·    NATIVE AOT');
  LabelControl := CWLabel(WizardForm.WelcomePage,0,181,700,22,9,CustomMessage('CWInstallScope'));
  if IsAdminInstallMode then
    LabelControl := CWLabel(WizardForm.WelcomePage,0,210,700,24,10,CustomMessage('CWAllUsers'))
  else LabelControl := CWLabel(WizardForm.WelcomePage,0,210,700,24,10,CustomMessage('CWCurrentUser'));
  LabelControl := CWLabel(WizardForm.WelcomePage,0,263,700,50,10,CustomMessage('CWRuntime'));
  CWBounds(WizardForm.RunList,0,104,730,190);
  CWBounds(WizardForm.FinishedPage,0,0,761,370);
  CWBounds(WizardForm.LicenseMemo,0,36,761,250); CWBounds(WizardForm.LicenseLabel1,0,0,761,30);
  CWBounds(WizardForm.LicenseAcceptedRadio,0,302,761,24); CWBounds(WizardForm.LicenseNotAcceptedRadio,0,332,761,24);
  CWBounds(WizardForm.SelectDirLabel,0,0,761,50); CWBounds(WizardForm.DirEdit,0,68,630,34);
  CWBounds(WizardForm.DirBrowseButton,647,68,114,34); CWBounds(WizardForm.DiskSpaceLabel,0,132,761,35);
  CWBounds(WizardForm.SelectTasksLabel,0,0,761,45); CWBounds(WizardForm.TasksList,0,55,761,285);
  CWBounds(WizardForm.ReadyLabel,0,0,761,40); CWBounds(WizardForm.ReadyMemo,0,48,761,302);
  CWBounds(WizardForm.StatusLabel,0,28,560,40); CWBounds(WizardForm.FilenameLabel,0,82,761,40);
  CWBounds(WizardForm.ProgressGauge,0,142,761,12);
  CWPercentLabel := CWLabel(WizardForm.InstallingPage,581,4,180,74,34,'0%');
  CWPercentLabel.Alignment := taRightJustify; CWPercentLabel.WordWrap := False;
  CWInitializeProgress;
  CWBounds(WizardForm.CancelButton,878,625,96,38); CWBounds(WizardForm.BackButton,768,625,96,38); CWBounds(WizardForm.NextButton,989,625,109,38);
  CWThemeButton := TNewButton.Create(WizardForm); CWThemeButton.Parent:=WizardForm; CWThemeButton.Caption:='◐'; CWThemeButton.Hint:=CustomMessage('CWAppearance'); CWThemeButton.ShowHint:=True; CWThemeButton.OnClick:=@CWThemeClick; CWBounds(CWThemeButton,978,23,30,28);
  CWMinimizeButton := TNewButton.Create(WizardForm); CWMinimizeButton.Parent:=WizardForm; CWMinimizeButton.Caption:='−'; CWMinimizeButton.Hint:=CustomMessage('CWMinimize'); CWMinimizeButton.ShowHint:=True; CWMinimizeButton.OnClick:=@CWMinimizeClick; CWBounds(CWMinimizeButton,1013,23,30,28);
  CWCloseButton := TNewButton.Create(WizardForm); CWCloseButton.Parent:=WizardForm; CWCloseButton.Caption:='×'; CWCloseButton.Hint:=CustomMessage('CWClose'); CWCloseButton.ShowHint:=True; CWCloseButton.OnClick:=@CWCloseClick; CWBounds(CWCloseButton,1048,23,30,28);
  CWReduceMotion := TNewCheckBox.Create(WizardForm); CWReduceMotion.Parent:=WizardForm; CWReduceMotion.Caption:=CustomMessage('CWMotion'); CWBounds(CWReduceMotion,337,580,400,22);
  Enabled := 1; if CWSystemParametersInfo($1042,0,Enabled,0) then CWReduceMotion.Checked := Enabled = 0;
  CWOwnerDraw(WizardForm.NextButton); CWOwnerDraw(WizardForm.BackButton); CWOwnerDraw(WizardForm.CancelButton);
  CWOwnerDraw(CWThemeButton); CWOwnerDraw(CWMinimizeButton); CWOwnerDraw(CWCloseButton);
  CWPreviousProc := CWSetWindowLong(WizardForm.Handle,-4,CreateCallback(@CWWindowProc));
  CWThemeMode := 0; CWDark := True; CWInitialized := True; CWRefreshAppearance;
  CWTimer := CWSetTimer(0,0,50,CreateCallback(@CWTimerProc));
end;

<event('DeinitializeSetup')>
procedure CWDeinitializeSetup;
var I: Integer;
begin
  CWInitialized := False;
  if CWTimer <> 0 then CWKillTimer(0,CWTimer);
  CWReleaseProgress;
  if (CWPreviousProc <> 0) and (WizardForm <> nil) then CWSetWindowLong(WizardForm.Handle,-4,CWPreviousProc);
  for I := 0 to CWPageHookCount-1 do
    if CWIsWindow(CWPageHandles[I]) then CWSetWindowLong(CWPageHandles[I],-4,CWPageProcs[I]);
  for I := 0 to CWButtonCount-1 do
    if CWIsWindow(CWButtonHandles[I]) then CWSetWindowLong(CWButtonHandles[I],-4,CWButtonProcs[I]);
  if CWReactorBase <> nil then CWReactorBase.Free;
  if CWCore <> nil then CWCore.Free;
  if CWBackgroundBrush <> 0 then CWDeleteObject(CWBackgroundBrush);
end;

#include "CodeWF.Setup.Maintenance.iss"

#include "CodeWF.Setup.Uninstall.iss"
