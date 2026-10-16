#ifndef CodeWFSetupUninstallIncluded
#define CodeWFSetupUninstallIncluded
#ifndef CodeWFSetupAssetPath
  #define CodeWFSetupAssetPath "installer"
#endif
#ifndef CodeWFSetupProductName
  #define CodeWFSetupProductName SetupSetting("AppName")
#endif

[CustomMessages]
; 原生卸载窗口的独立文案，不替换确认、错误或完成对话框。
english.CWUUninstalling=Uninstalling
chinesesimplified.CWUUninstalling=正在卸载
english.CWUDescription=The native uninstaller is removing installed files. Please wait.
chinesesimplified.CWUDescription=原生卸载程序正在移除已安装的文件，请稍候。
english.CWUNative=Native uninstall engine
chinesesimplified.CWUNative=原生卸载引擎
english.CWUProgress=Uninstall progress
chinesesimplified.CWUProgress=卸载进度
english.CWUAppearance=Appearance: %1 (dark / light / system)
chinesesimplified.CWUAppearance=外观：%1（深色／浅色／跟随系统）
english.CWUModeDark=Dark
chinesesimplified.CWUModeDark=深色
english.CWUModeLight=Light
chinesesimplified.CWUModeLight=浅色
english.CWUModeSystem=System
chinesesimplified.CWUModeSystem=跟随系统
english.CWUMinimize=Minimize
chinesesimplified.CWUMinimize=最小化
english.CWUClose=Close
chinesesimplified.CWUClose=关闭

[Files]
; 独立卸载器不能解包安装包的 dontcopy 资源；精确登记这些自有资源并由原生日志清理。
#if !Defined(CodeWFSetupFixture) || Defined(CodeWFSetupUninstallFixture)
Source: "{#CodeWFSetupAssetPath}\frame-dark.bmp"; DestDir: "{app}\.codewf-installer"; Flags: ignoreversion
Source: "{#CodeWFSetupAssetPath}\frame-light.bmp"; DestDir: "{app}\.codewf-installer"; Flags: ignoreversion
Source: "{#CodeWFSetupAssetPath}\reactor-dark.bmp"; DestDir: "{app}\.codewf-installer"; Flags: ignoreversion
Source: "{#CodeWFSetupAssetPath}\reactor-light.bmp"; DestDir: "{app}\.codewf-installer"; Flags: ignoreversion
Source: "{#CodeWFSetupAssetPath}\core-dark.bmp"; DestDir: "{app}\.codewf-installer"; Flags: ignoreversion
Source: "{#CodeWFSetupAssetPath}\core-light.bmp"; DestDir: "{app}\.codewf-installer"; Flags: ignoreversion
#endif

[Code]
type
  TCWUPoint = record X, Y: Integer; end;
  TCWURect = record Left, Top, Right, Bottom: Integer; end;
  TCWUDrawItem = record
    CtlType, CtlID, ItemID, ItemAction, ItemState: LongWord;
    ItemWindow, DC: LongWord;
    Bounds: TCWURect;
    ItemData: LongWord;
  end;
  TCWUPaint = record
    DC: LongWord;
    Erase: Longint;
    Bounds: TCWURect;
    Restore, Update: Longint;
    Reserved: array[0..31] of Byte;
  end;

function CWUCreatePolygonRegion(var Points: TCWUPoint; Count, Mode: Integer): LongWord;
  external 'CreatePolygonRgn@gdi32.dll stdcall';
function CWUSetWindowRegion(Window, Region: LongWord; Redraw: Boolean): Integer;
  external 'SetWindowRgn@user32.dll stdcall';
function CWUDeleteObject(Handle: LongWord): Boolean;
  external 'DeleteObject@gdi32.dll stdcall';
function CWUGetSystemMetrics(Index: Integer): Integer;
  external 'GetSystemMetrics@user32.dll stdcall';
function CWUSetWindowLong(Window: LongWord; Index: Integer; Value: Longint): Longint;
  external 'SetWindowLongW@user32.dll stdcall';
function CWUGetWindowLong(Window: LongWord; Index: Integer): Longint;
  external 'GetWindowLongW@user32.dll stdcall';
function CWUCallWindowProc(Previous, Window, Message, WParam, LParam: LongWord): Longint;
  external 'CallWindowProcW@user32.dll stdcall';
function CWUDefaultWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
  external 'DefWindowProcW@user32.dll stdcall';
function CWUIsWindow(Window: LongWord): Boolean;
  external 'IsWindow@user32.dll stdcall';
function CWUFillRect(DC: LongWord; var Bounds: TCWURect; Brush: LongWord): Integer;
  external 'FillRect@user32.dll stdcall';
function CWUCreateSolidBrush(Color: LongWord): LongWord;
  external 'CreateSolidBrush@gdi32.dll stdcall';
function CWUSetTextColor(DC, Color: LongWord): LongWord;
  external 'SetTextColor@gdi32.dll stdcall';
function CWUSetBkColor(DC, Color: LongWord): LongWord;
  external 'SetBkColor@gdi32.dll stdcall';
function CWUSetBkMode(DC: LongWord; Mode: Integer): Integer;
  external 'SetBkMode@gdi32.dll stdcall';
function CWUDrawText(DC: LongWord; Text: String; Length: Integer; var Bounds: TCWURect; Format: LongWord): Integer;
  external 'DrawTextW@user32.dll stdcall';
function CWUDrawFocusRect(DC: LongWord; var Bounds: TCWURect): Boolean;
  external 'DrawFocusRect@user32.dll stdcall';
procedure CWUCopyMemory(var Target: TCWUDrawItem; Source, Size: LongWord);
  external 'RtlMoveMemory@kernel32.dll stdcall';
function CWUStretchBlt(Target: LongWord; X, Y, Width, Height: Integer; Source: LongWord; SourceX, SourceY, SourceWidth, SourceHeight: Integer; Mode: LongWord): Boolean;
  external 'StretchBlt@gdi32.dll stdcall';
function CWUSetStretchBltMode(DC: LongWord; Mode: Integer): Integer;
  external 'SetStretchBltMode@gdi32.dll stdcall';
function CWUGetClientRect(Window: LongWord; var Bounds: TCWURect): Boolean;
  external 'GetClientRect@user32.dll stdcall';
function CWUBeginPaint(Window: LongWord; var Paint: TCWUPaint): LongWord;
  external 'BeginPaint@user32.dll stdcall';
function CWUEndPaint(Window: LongWord; var Paint: TCWUPaint): Boolean;
  external 'EndPaint@user32.dll stdcall';
function CWUInvalidateRect(Window, Bounds: LongWord; Erase: Boolean): Boolean;
  external 'InvalidateRect@user32.dll stdcall';
function CWUShowWindow(Window: LongWord; Command: Integer): Boolean;
  external 'ShowWindow@user32.dll stdcall';
function CWUPostMessage(Window, Message, WParam, LParam: LongWord): Boolean;
  external 'PostMessageW@user32.dll stdcall';
function CWUGetWindowRect(Window: LongWord; var Bounds: TCWURect): Boolean;
  external 'GetWindowRect@user32.dll stdcall';
function CWUSendMessage(Window, Message, WParam, LParam: LongWord): Longint;
  external 'SendMessageW@user32.dll stdcall';

var
  CWUFrames, CWUReactors, CWUCores: array[0..1] of TBitmap;
  CWUBackground, CWUReactor: TBitmapImage;
  CWUTitle, CWUHeading, CWUDescription, CWUProduct, CWUFooter, CWUPercent: TLabel;
  CWUThemeButton, CWUMinimizeButton, CWUCloseButton: TNewButton;
  CWUFormHandle, CWUGaugeHandle, CWUPageHandle: LongWord;
  CWUFormProc, CWUGaugeProc, CWUPageProc, CWUBackgroundBrush: LongWord;
  CWUScalePercent, CWUProgress, CWUThemeMode: Integer;
  CWUInitialized, CWUDark: Boolean;
  CWUSurface, CWUText, CWUMuted, CWUField: TColor;

function CWUSize(Value: Integer): Integer;
begin Result := (ScaleX(Value) * CWUScalePercent) div 100; end;

procedure CWUBounds(Control: TControl; X, Y, Width, Height: Integer);
begin Control.SetBounds(CWUSize(X), CWUSize(Y), CWUSize(Width), CWUSize(Height)); end;

function CWULabel(Parent: TWinControl; X, Y, Width, Height, Size: Integer; Caption: String): TLabel;
begin
  Result := TLabel.Create(UninstallProgressForm); Result.Parent := Parent;
  Result.Transparent := True; Result.AutoSize := False; Result.WordWrap := True;
  Result.Caption := Caption; Result.Font.Name := 'Segoe UI';
  Result.Font.Size := (Size * CWUScalePercent) div 100;
  CWUBounds(Result, X, Y, Width, Height);
end;

procedure CWUFreeBitmaps;
var I: Integer;
begin
  for I := 0 to 1 do begin
    if CWUFrames[I] <> nil then begin CWUFrames[I].Free; CWUFrames[I] := nil; end;
    if CWUReactors[I] <> nil then begin CWUReactors[I].Free; CWUReactors[I] := nil; end;
    if CWUCores[I] <> nil then begin CWUCores[I].Free; CWUCores[I] := nil; end;
  end;
end;

function CWULoadBitmaps: Boolean;
var I: Integer; Theme, Root: String;
begin
  Result := False;
  Root := ExpandConstant('{app}\.codewf-installer\');
  try
    for I := 0 to 1 do begin
      if I = 0 then Theme := 'dark' else Theme := 'light';
      CWUFrames[I] := TBitmap.Create; CWUFrames[I].LoadFromFile(Root + 'frame-' + Theme + '.bmp');
      CWUReactors[I] := TBitmap.Create; CWUReactors[I].LoadFromFile(Root + 'reactor-' + Theme + '.bmp');
      CWUCores[I] := TBitmap.Create; CWUCores[I].LoadFromFile(Root + 'core-' + Theme + '.bmp');
    end;
    Result := True;
  except
    // 缺失或损坏资源只回退原生窗口，不阻断卸载。
    Log('CodeWF uninstall: branding resources unavailable; native window retained.');
    CWUFreeBitmaps;
  end;
end;

procedure CWUApplyRegion;
var Points: array[0..15] of TCWUPoint; I: Integer; Region: LongWord;
begin
  Points[0].X:=324; Points[0].Y:=0; Points[1].X:=818; Points[1].Y:=0;
  Points[2].X:=840; Points[2].Y:=18; Points[3].X:=1088; Points[3].Y:=18;
  Points[4].X:=1136; Points[4].Y:=66; Points[5].X:=1136; Points[5].Y:=648;
  Points[6].X:=1096; Points[6].Y:=688; Points[7].X:=682; Points[7].Y:=688;
  Points[8].X:=658; Points[8].Y:=672; Points[9].X:=24; Points[9].Y:=672;
  Points[10].X:=0; Points[10].Y:=648; Points[11].X:=0; Points[11].Y:=132;
  Points[12].X:=38; Points[12].Y:=94; Points[13].X:=240; Points[13].Y:=94;
  Points[14].X:=300; Points[14].Y:=154; Points[15].X:=300; Points[15].Y:=24;
  for I := 0 to 15 do begin Points[I].X := CWUSize(Points[I].X); Points[I].Y := CWUSize(Points[I].Y); end;
  Region := CWUCreatePolygonRegion(Points[0], 16, 1);
  if (Region <> 0) and (CWUSetWindowRegion(CWUFormHandle, Region, True) = 0) then CWUDeleteObject(Region);
end;

procedure CWUDrawCore;
var Theme: Integer; Canvas: TCanvas; Text: String;
begin
  if not CWUInitialized then exit;
  if CWUDark then Theme := 0 else Theme := 1;
  CWUReactor.Bitmap.Width := CWUSize(220); CWUReactor.Bitmap.Height := CWUSize(180);
  Canvas := CWUReactor.Bitmap.Canvas; CWUSetStretchBltMode(Canvas.Handle, 4);
  CWUStretchBlt(Canvas.Handle, 0, 0, CWUSize(220), CWUSize(180), CWUReactors[Theme].Canvas.Handle,
    0, 0, CWUReactors[Theme].Width, CWUReactors[Theme].Height, $00CC0020);
  CWUStretchBlt(Canvas.Handle, CWUSize(48), CWUSize(28), CWUSize(124), CWUSize(124), CWUCores[Theme].Canvas.Handle,
    0, 0, CWUCores[Theme].Width, CWUCores[Theme].Height, $00CC0020);
  Canvas.Font.Name := 'Segoe UI'; Canvas.Font.Size := (25 * CWUScalePercent) div 100;
  Canvas.Font.Color := clWhite; Canvas.Brush.Style := bsClear;
  Text := IntToStr(CWUProgress) + '%';
  Canvas.TextOut((CWUReactor.Width - Canvas.TextWidth(Text)) div 2,
    (CWUReactor.Height - Canvas.TextHeight(Text)) div 2, Text);
  CWUReactor.Invalidate;
end;

procedure CWUApplyTheme;
var Theme: Integer;
begin
  if not CWUInitialized then exit;
  if CWUDark then begin
    Theme := 0; CWUSurface := $251016; CWUText := $F7F2F5; CWUMuted := $CDB6C2; CWUField := $2E1821;
  end else begin
    Theme := 1; CWUSurface := clWhite; CWUText := $351226; CWUMuted := $705366; CWUField := $FCF7FA;
  end;
  CWUBackground.Bitmap := CWUFrames[Theme];
  UninstallProgressForm.Color := CWUSurface;
  UninstallProgressForm.InnerPage.Color := CWUSurface;
  UninstallProgressForm.InstallingPage.Color := CWUSurface;
  UninstallProgressForm.StatusLabel.Font.Color := CWUText;
  CWUHeading.Font.Color := CWUText; CWUDescription.Font.Color := CWUMuted;
  CWUTitle.Font.Color := CWUMuted; CWUFooter.Font.Color := CWUMuted;
  CWUPercent.Font.Color := CWUText; CWUProduct.Font.Color := clWhite;
  if CWUBackgroundBrush <> 0 then CWUDeleteObject(CWUBackgroundBrush);
  CWUBackgroundBrush := CWUCreateSolidBrush(CWUSurface);
  CWUDrawCore;
  CWUInvalidateRect(CWUFormHandle, 0, True); CWUInvalidateRect(CWUPageHandle, 0, True);
  CWUInvalidateRect(CWUGaugeHandle, 0, True);
end;

function CWUSystemDark: Boolean;
var Light: Cardinal;
begin
  // 只读 Windows 应用外观，不保存或更改系统偏好；缺项沿用浅色系统默认。
  Result := False;
  if RegQueryDWordValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Themes\Personalize',
    'AppsUseLightTheme', Light) then Result := Light = 0;
end;

procedure CWUApplyAppearance;
var ModeKey: String;
begin
  if CWUThemeMode = 0 then begin CWUDark := True; ModeKey := 'CWUModeDark'; end
  else if CWUThemeMode = 1 then begin CWUDark := False; ModeKey := 'CWUModeLight'; end
  else begin CWUDark := CWUSystemDark; ModeKey := 'CWUModeSystem'; end;
  CWUThemeButton.Hint := FmtMessage(CustomMessage('CWUAppearance'), [CustomMessage(ModeKey)]);
  CWUApplyTheme;
end;

procedure CWUThemeClick(Sender: TObject);
begin CWUThemeMode := (CWUThemeMode + 1) mod 3; CWUApplyAppearance; end;
procedure CWUMinimizeClick(Sender: TObject);
begin CWUShowWindow(CWUFormHandle, 6); end;
procedure CWUCloseClick(Sender: TObject);
begin CWUPostMessage(CWUFormHandle, $0010, 0, 0); end;

procedure CWUOwnerDraw(Button: TNewButton);
begin CWUSetWindowLong(Button.Handle, -16, (CWUGetWindowLong(Button.Handle, -16) and not $F) or $B); end;

procedure CWUPaintProgress(DC, Window: LongWord);
var Bounds, Segment: TCWURect; Brush, Color: LongWord; I, Width, Fill: Integer;
begin
  CWUGetClientRect(Window, Bounds); Width := Bounds.Right - Bounds.Left;
  Brush := CWUCreateSolidBrush(CWUSurface); CWUFillRect(DC, Bounds, Brush); CWUDeleteObject(Brush);
  Fill := (Width * CWUProgress) div 100;
  for I := 0 to 22 do begin
    Segment.Left := (I * Width) div 23; Segment.Right := ((I + 1) * Width) div 23 - CWUSize(3);
    Segment.Top := 0; Segment.Bottom := Bounds.Bottom;
    if CWUDark then Color := $442B38 else Color := $EDE2E8;
    Brush := CWUCreateSolidBrush(Color); CWUFillRect(DC, Segment, Brush); CWUDeleteObject(Brush);
    if Segment.Left < Fill then begin
      if Segment.Right > Fill then Segment.Right := Fill;
      Color := (81 + (76 * I) div 22) or ((43 + (70 * I) div 22) shl 8) or ((212 + (43 * I) div 22) shl 16);
      Brush := CWUCreateSolidBrush(Color); CWUFillRect(DC, Segment, Brush); CWUDeleteObject(Brush);
    end;
  end;
end;

procedure CWUReadProgress;
var Current, Minimum, Maximum: Integer;
begin
  if not CWUInitialized or not CWUIsWindow(CWUGaugeHandle) then exit;
  // 仅查询原生进度条；不模拟时间进度，不改 Position/Min/Max 或取消处理。
  Minimum := CWUSendMessage(CWUGaugeHandle, $0407, 1, 0);
  Maximum := CWUSendMessage(CWUGaugeHandle, $0407, 0, 0);
  Current := CWUSendMessage(CWUGaugeHandle, $0408, 0, 0);
  if Maximum > Minimum then CWUProgress := Round((Current - Minimum) * 100.0 / (Maximum - Minimum))
  else CWUProgress := 0;
  if CWUProgress < 0 then CWUProgress := 0;
  if CWUProgress > 100 then CWUProgress := 100;
  CWUPercent.Caption := CustomMessage('CWUProgress') + '  ' + IntToStr(CWUProgress) + '%';
  CWUCloseButton.Enabled := UninstallProgressForm.CancelButton.Enabled;
  // Inno 显示进度窗时可能更新按钮样式，真实进度消息后恢复其原生 owner-draw 外观。
  CWUOwnerDraw(UninstallProgressForm.CancelButton); CWUOwnerDraw(CWUThemeButton);
  CWUOwnerDraw(CWUMinimizeButton); CWUOwnerDraw(CWUCloseButton);
  CWUInvalidateRect(UninstallProgressForm.CancelButton.Handle, 0, True);
  CWUInvalidateRect(CWUThemeButton.Handle, 0, True);
  CWUInvalidateRect(CWUMinimizeButton.Handle, 0, True);
  CWUInvalidateRect(CWUCloseButton.Handle, 0, True);
  CWUDrawCore; CWUInvalidateRect(CWUGaugeHandle, 0, True);
end;

function CWUGaugeWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var Paint: TCWUPaint; DC, Previous: LongWord;
begin
  Previous := CWUGaugeProc;
  if CWUInitialized then begin
    if Message = $000F then begin
      DC := CWUBeginPaint(Window, Paint); CWUPaintProgress(DC, Window); CWUEndPaint(Window, Paint);
      Result := 0; exit;
    end;
    if Message = $0318 then begin CWUPaintProgress(WParam, Window); Result := 0; exit; end;
    if Message = $0014 then begin Result := 1; exit; end;
  end;
  if Previous <> 0 then Result := CWUCallWindowProc(Previous, Window, Message, WParam, LParam)
  else Result := CWUDefaultWindowProc(Window, Message, WParam, LParam);
  // 先转发原生范围/位置更新，再查询结果；保留原始消息返回值与辅助功能。
  if (Message = $0401) or (Message = $0402) or (Message = $0403) or
    (Message = $0405) or (Message = $0406) then CWUReadProgress;
  if Message = $0082 then begin CWUGaugeHandle := 0; CWUGaugeProc := 0; end;
end;

function CWUPageWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var Previous: LongWord;
begin
  Previous := CWUPageProc;
  if CWUInitialized and ((Message = $0135) or (Message = $0138)) then begin
    CWUSetTextColor(WParam, CWUText); CWUSetBkColor(WParam, CWUSurface);
    Result := CWUBackgroundBrush; exit;
  end;
  if Previous <> 0 then Result := CWUCallWindowProc(Previous, Window, Message, WParam, LParam)
  else Result := CWUDefaultWindowProc(Window, Message, WParam, LParam);
  if Message = $0082 then begin CWUPageHandle := 0; CWUPageProc := 0; end;
end;

function CWUSignedWord(Value: LongWord): Integer;
begin
  Result := Value and $FFFF;
  if Result >= $8000 then Result := Result - $10000;
end;

function CWUFormWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var Item: TCWUDrawItem; Bounds: TCWURect; Caption: String; Color, Foreground, Brush, Previous: LongWord; X, Y: Integer;
begin
  Previous := CWUFormProc;
  if CWUInitialized then begin
    if (Message = $001A) and (CWUThemeMode = 2) then CWUApplyAppearance;
    if Message = $0084 then begin
      CWUGetWindowRect(Window, Bounds);
      X := CWUSignedWord(LParam) - Bounds.Left; Y := CWUSignedWord(LParam shr 16) - Bounds.Top;
      if (Y >= 0) and (Y < CWUSize(60)) and (X >= 0) and (X < CWUSize(970)) then begin Result := 2; exit; end;
    end;
    if Message = $002B then begin
      CWUCopyMemory(Item, LParam, 48);
      if Item.CtlType = 4 then begin
        Caption := ''; Color := CWUSurface; Foreground := CWUText;
        if Item.ItemWindow = UninstallProgressForm.CancelButton.Handle then begin
          Caption := UninstallProgressForm.CancelButton.Caption; Color := $D42B51; Foreground := clWhite;
        end else if Item.ItemWindow = CWUThemeButton.Handle then Caption := '◐'
        else if Item.ItemWindow = CWUMinimizeButton.Handle then Caption := '−'
        else if Item.ItemWindow = CWUCloseButton.Handle then Caption := '×';
        if Caption <> '' then begin
          if (Item.ItemState and 4) <> 0 then begin Color := CWUField; Foreground := CWUMuted; end;
          Bounds := Item.Bounds;
          Brush := CWUCreateSolidBrush(Color); CWUFillRect(Item.DC, Bounds, Brush); CWUDeleteObject(Brush);
          CWUSetTextColor(Item.DC, Foreground); CWUSetBkMode(Item.DC, 1);
          CWUDrawText(Item.DC, Caption, -1, Bounds, $25);
          if (Item.ItemState and $10) <> 0 then begin
            Bounds.Left := Bounds.Left + 4; Bounds.Top := Bounds.Top + 4;
            Bounds.Right := Bounds.Right - 4; Bounds.Bottom := Bounds.Bottom - 4;
            CWUDrawFocusRect(Item.DC, Bounds);
          end;
          Result := 1; exit;
        end;
      end;
    end;
  end;
  if Message = $0082 then CWUInitialized := False;
  if Previous <> 0 then Result := CWUCallWindowProc(Previous, Window, Message, WParam, LParam)
  else Result := CWUDefaultWindowProc(Window, Message, WParam, LParam);
  if Message = $0082 then begin CWUFormHandle := 0; CWUFormProc := 0; end;
end;

<event('InitializeUninstallProgressForm')>
procedure CWUInitializeUninstallProgressForm;
begin
  if UninstallSilent or (UninstallProgressForm = nil) then exit;
  if not CWULoadBitmaps then exit;
  CWUScalePercent := 100;
  if ScaleX(1136) > CWUGetSystemMetrics(0) - 64 then CWUScalePercent := ((CWUGetSystemMetrics(0) - 64) * 100) div ScaleX(1136);
  if (ScaleX(688) * CWUScalePercent) div 100 > CWUGetSystemMetrics(1) - 100 then
    CWUScalePercent := ((CWUGetSystemMetrics(1) - 100) * 100) div ScaleX(688);
  UninstallProgressForm.BorderStyle := bsNone;
  UninstallProgressForm.ClientWidth := CWUSize(1136); UninstallProgressForm.ClientHeight := CWUSize(688);
  UninstallProgressForm.Position := poScreenCenter; UninstallProgressForm.Font.Name := 'Segoe UI';
  UninstallProgressForm.MainPanel.Visible := False; UninstallProgressForm.Bevel.Visible := False;
  UninstallProgressForm.Bevel1.Visible := False; UninstallProgressForm.BeveledLabel.Visible := False;
  CWUBackground := TBitmapImage.Create(UninstallProgressForm); CWUBackground.Parent := UninstallProgressForm;
  CWUBackground.Stretch := True; CWUBounds(CWUBackground, 0, 0, 1136, 688); CWUBackground.SendToBack;
  CWUReactor := TBitmapImage.Create(UninstallProgressForm); CWUReactor.Parent := UninstallProgressForm;
  CWUBounds(CWUReactor, 40, 104, 220, 180);
  CWUProduct := CWULabel(UninstallProgressForm, 28, 294, 250, 72, 12, '{#CodeWFSetupProductName}');
  CWUProduct.Font.Style := [fsBold];
  CWUHeading := CWULabel(UninstallProgressForm, 337, 96, 760, 46, 23, CustomMessage('CWUUninstalling'));
  CWUHeading.Font.Style := [fsBold];
  CWUDescription := CWULabel(UninstallProgressForm, 337, 147, 760, 48, 10, CustomMessage('CWUDescription'));
  CWUFooter := CWULabel(UninstallProgressForm, 337, 635, 500, 20, 8, CustomMessage('CWUNative'));
  CWUTitle := CWULabel(UninstallProgressForm, 338, 24, 600, 24, 9, CustomMessage('CWUUninstalling'));
  CWUPercent := CWULabel(UninstallProgressForm, 337, 376, 760, 40, 16, '0%');
  UninstallProgressForm.OuterNotebook.Anchors := [akLeft, akTop];
  UninstallProgressForm.InnerNotebook.Anchors := [akLeft, akTop];
  CWUBounds(UninstallProgressForm.OuterNotebook, 337, 208, 761, 150);
  CWUBounds(UninstallProgressForm.InnerPage, 0, 0, 761, 150);
  CWUBounds(UninstallProgressForm.InnerNotebook, 0, 0, 761, 150);
  CWUBounds(UninstallProgressForm.InstallingPage, 0, 0, 761, 150);
  CWUBounds(UninstallProgressForm.StatusLabel, 0, 16, 761, 70);
  CWUBounds(UninstallProgressForm.ProgressBar, 0, 116, 761, 12);
  CWUBounds(UninstallProgressForm.CancelButton, 989, 625, 109, 38);
  CWUThemeButton := TNewButton.Create(UninstallProgressForm); CWUThemeButton.Parent := UninstallProgressForm;
  CWUThemeButton.Caption := '◐'; CWUThemeButton.Hint := CustomMessage('CWUAppearance'); CWUThemeButton.ShowHint := True;
  CWUThemeButton.OnClick := @CWUThemeClick; CWUBounds(CWUThemeButton, 978, 23, 30, 28);
  CWUMinimizeButton := TNewButton.Create(UninstallProgressForm); CWUMinimizeButton.Parent := UninstallProgressForm;
  CWUMinimizeButton.Caption := '−'; CWUMinimizeButton.Hint := CustomMessage('CWUMinimize'); CWUMinimizeButton.ShowHint := True;
  CWUMinimizeButton.OnClick := @CWUMinimizeClick; CWUBounds(CWUMinimizeButton, 1013, 23, 30, 28);
  CWUCloseButton := TNewButton.Create(UninstallProgressForm); CWUCloseButton.Parent := UninstallProgressForm;
  CWUCloseButton.Caption := '×'; CWUCloseButton.Hint := CustomMessage('CWUClose'); CWUCloseButton.ShowHint := True;
  CWUCloseButton.Enabled := UninstallProgressForm.CancelButton.Enabled;
  CWUCloseButton.OnClick := @CWUCloseClick; CWUBounds(CWUCloseButton, 1048, 23, 30, 28);
  // BorderStyle/字体可能重建 HWND，完成布局后再保存并挂钩最终句柄。
  CWUFormHandle := UninstallProgressForm.Handle;
  CWUGaugeHandle := UninstallProgressForm.ProgressBar.Handle;
  CWUPageHandle := UninstallProgressForm.InstallingPage.Handle;
  CWUApplyRegion;
  CWUOwnerDraw(UninstallProgressForm.CancelButton); CWUOwnerDraw(CWUThemeButton);
  CWUOwnerDraw(CWUMinimizeButton); CWUOwnerDraw(CWUCloseButton);
  CWUFormProc := CWUSetWindowLong(CWUFormHandle, -4, CreateCallback(@CWUFormWindowProc));
  CWUPageProc := CWUSetWindowLong(CWUPageHandle, -4, CreateCallback(@CWUPageWindowProc));
  CWUGaugeProc := CWUSetWindowLong(CWUGaugeHandle, -4, CreateCallback(@CWUGaugeWindowProc));
  CWUThemeMode := 0; CWUInitialized := True; CWUApplyAppearance; CWUReadProgress;
end;

<event('DeinitializeUninstall')>
procedure CWUDeinitializeUninstall;
begin
  CWUInitialized := False;
  // 仅恢复自己仍存活的精确 HWND；先撤回调，再释放可能被回调使用的 GDI 资源。
  if (CWUGaugeProc <> 0) and CWUIsWindow(CWUGaugeHandle) then CWUSetWindowLong(CWUGaugeHandle, -4, CWUGaugeProc);
  if (CWUPageProc <> 0) and CWUIsWindow(CWUPageHandle) then CWUSetWindowLong(CWUPageHandle, -4, CWUPageProc);
  if (CWUFormProc <> 0) and CWUIsWindow(CWUFormHandle) then CWUSetWindowLong(CWUFormHandle, -4, CWUFormProc);
  CWUGaugeProc := 0; CWUPageProc := 0; CWUFormProc := 0;
  CWUGaugeHandle := 0; CWUPageHandle := 0; CWUFormHandle := 0;
  if CWUBackgroundBrush <> 0 then begin CWUDeleteObject(CWUBackgroundBrush); CWUBackgroundBrush := 0; end;
  CWUFreeBitmaps;
end;
#endif
