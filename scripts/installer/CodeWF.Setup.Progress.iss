{ 给原生进度条绘制紫色分段外观，保留真实 HWND、进度数值与辅助功能。 }
type
  TCWPaint = record
    DC: LongWord;
    Erase: Longint;
    Bounds: TCWRect;
    Restore, Update: Longint;
    Reserved: array[0..31] of Byte;
  end;

function CWBeginPaint(Window: LongWord; var Paint: TCWPaint): LongWord;
  external 'BeginPaint@user32.dll stdcall';
function CWEndPaint(Window: LongWord; var Paint: TCWPaint): Boolean;
  external 'EndPaint@user32.dll stdcall';
function CWGetClientRect(Window: LongWord; var Bounds: TCWRect): Boolean;
  external 'GetClientRect@user32.dll stdcall';

var CWPreviousProgressProc: LongWord;

procedure CWPaintProgress(DC, Window: LongWord);
var Bounds, Segment: TCWRect; Brush, Color: LongWord; I, Width, Fill: Integer;
begin
  CWGetClientRect(Window,Bounds);
  Width := Bounds.Right-Bounds.Left;
  Brush := CWCreateSolidBrush(CWSurface); CWFillRect(DC,Bounds,Brush); CWDeleteObject(Brush);
  Fill := (Width*CWProgress) div 100;
  for I := 0 to 22 do begin
    Segment.Left := (I*Width) div 23; Segment.Right := ((I+1)*Width) div 23-CWSize(3);
    Segment.Top := 0; Segment.Bottom := Bounds.Bottom;
    if CWDark then Color := $442B38 else Color := $EDE2E8;
    Brush := CWCreateSolidBrush(Color); CWFillRect(DC,Segment,Brush); CWDeleteObject(Brush);
    if Segment.Left < Fill then begin
      if Segment.Right > Fill then Segment.Right := Fill;
      Color := (81+(76*I) div 22) or ((43+(70*I) div 22) shl 8) or ((212+(43*I) div 22) shl 16);
      Brush := CWCreateSolidBrush(Color); CWFillRect(DC,Segment,Brush); CWDeleteObject(Brush);
    end;
  end;
end;

function CWProgressWindowProc(Window, Message, WParam, LParam: LongWord): Longint;
var Paint: TCWPaint; DC: LongWord;
begin
  if Message = $000F then begin
    DC := CWBeginPaint(Window,Paint); CWPaintProgress(DC,Window); CWEndPaint(Window,Paint);
    Result := 0; exit;
  end;
  if Message = $0318 then begin CWPaintProgress(WParam,Window); Result := 0; exit; end;
  if Message = $0014 then begin Result := 1; exit; end;
  Result := CWCallWindowProc(CWPreviousProgressProc,Window,Message,WParam,LParam);
end;

procedure CWInitializeProgress;
begin
  CWPreviousProgressProc := CWSetWindowLong(WizardForm.ProgressGauge.Handle,-4,CreateCallback(@CWProgressWindowProc));
end;

procedure CWReleaseProgress;
begin
  if (CWPreviousProgressProc <> 0) and (WizardForm <> nil) then
    CWSetWindowLong(WizardForm.ProgressGauge.Handle,-4,CWPreviousProgressProc);
end;
