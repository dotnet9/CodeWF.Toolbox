{ 仅安全夹具延长真实进度的观察时间，生产安装器不包含此文件。 }
type
  TCWFixtureMessage = record
    Window, Message, WParam, LParam, Time: LongWord;
    X, Y: Integer;
  end;

function CWFixturePeekMessage(var Message: TCWFixtureMessage; Window, Minimum, Maximum, Remove: LongWord): Boolean;
  external 'PeekMessageW@user32.dll stdcall';
function CWFixtureTranslateMessage(var Message: TCWFixtureMessage): Boolean;
  external 'TranslateMessage@user32.dll stdcall';
function CWFixtureDispatchMessage(var Message: TCWFixtureMessage): Longint;
  external 'DispatchMessageW@user32.dll stdcall';
function CWFixtureTickCount: LongWord;
  external 'GetTickCount@kernel32.dll stdcall';

var CWFixtureWaiting, CWFixtureStartObserved, CWFixtureEndObserved: Boolean;

procedure CWFixtureObserveFrame;
var Started: LongWord; Message: TCWFixtureMessage;
begin
  CWFixtureWaiting := True;
  try
    Started := CWFixtureTickCount;
    while CWFixtureTickCount-Started < 600 do begin
      while CWFixturePeekMessage(Message,0,0,0,1) do begin
        CWFixtureTranslateMessage(Message);
        CWFixtureDispatchMessage(Message);
      end;
      Sleep(10);
    end;
  finally
    CWFixtureWaiting := False;
  end;
end;

<event('CurInstallProgressChanged')>
procedure CWFixtureProgressChanged(Current, Maximum: Integer);
begin
  if CWFixtureWaiting or (Maximum <= 0) then exit;
  if (Current = 0) and not CWFixtureStartObserved then begin
    CWFixtureStartObserved := True;
    CWFixtureObserveFrame;
  end else if (Current >= Maximum) and not CWFixtureEndObserved then begin
    CWFixtureEndObserved := True;
    CWFixtureObserveFrame;
  end;
end;
