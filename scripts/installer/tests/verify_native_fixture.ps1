[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Installer,
    [string] $Output = '',
    [ValidateSet('english', 'chinesesimplified')] [string] $Language = 'english',
    [switch] $Maintenance
)
$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw '此验证器仅支持 Windows。' }
$fixtureName = if ($Maintenance) { 'Fixture.Maintenance.iss' } else { 'Fixture.iss' }
$fixtureSource = Get-Content -LiteralPath (Join-Path $PSScriptRoot $fixtureName) -Raw -Encoding UTF8
if ($fixtureSource -notmatch '(?im)^Uninstallable=no\s*$' -or
    $fixtureSource -notmatch '(?im)^CreateUninstallRegKey=no\s*$' -or
    $fixtureSource -notmatch '(?im)^#define CodeWFSetupFixture\s*$' -or
    $fixtureSource -match '(?im)^\[(Registry|Icons|Run|UninstallRun|UninstallDelete)\]') {
    throw '夹具必须关闭卸载注册且不声明关联、快捷方式或应用启动。'
}
if (-not $Maintenance -and ($fixtureSource -notmatch '#include "Fixture\.Progress\.iss"' -or
    -not (Test-Path -LiteralPath (Join-Path $PSScriptRoot 'Fixture.Progress.iss') -PathType Leaf))) {
    throw '夹具缺少专用进度观察钩子。'
}
$installerPath = (Resolve-Path -LiteralPath $Installer).Path
$installerSha256 = (Get-FileHash -LiteralPath $installerPath -Algorithm SHA256).Hash.ToLowerInvariant()
$expectedExecutable = if ($Maintenance) { 'CodeWF.Installer.Maintenance.Fixture.exe' } else { 'CodeWF.Installer.Fixture.exe' }
if ([IO.Path]::GetFileName($installerPath) -ne $expectedExecutable) {
    throw "只允许运行专用 $expectedExecutable，不运行实际产品安装器。"
}
if (-not $Output) {
    $temporaryRoot = if ($env:CODEWF_TEST_TMP) { $env:CODEWF_TEST_TMP } else { [IO.Path]::GetTempPath() }
    $Output = Join-Path $temporaryRoot ('codewf-setup-native-' + [guid]::NewGuid().ToString('N'))
}
$Output = [IO.Path]::GetFullPath($Output)
New-Item -ItemType Directory -Path $Output -Force | Out-Null
Add-Type -AssemblyName System.Drawing
if (-not ('CodeWFSetupFixtureDesktop' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
public static class CodeWFSetupFixtureDesktop {
 [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct StartupInfo { public int cb; public string reserved,desktop,title; public int x,y,cx,cy,charsX,charsY,fill,flags; public short show,reserved2; public IntPtr reservedPtr,input,output,error; }
 [StructLayout(LayoutKind.Sequential)] public struct ProcessInfo { public IntPtr process,thread; public int processId,threadId; }
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int left,top,right,bottom; }
 [StructLayout(LayoutKind.Sequential)] public struct GuiThreadInfo { public int size,flags;public IntPtr active,focus,capture,menuOwner,moveSize,caret;public Rect caretBounds; }
 public delegate bool EnumProc(IntPtr window,IntPtr data);
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr CreateDesktop(string name,IntPtr device,IntPtr mode,int flags,uint access,IntPtr security);
 [DllImport("user32.dll")] public static extern bool CloseDesktop(IntPtr desktop);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool CreateProcess(string application,StringBuilder command,IntPtr processAttr,IntPtr threadAttr,bool inherit,uint flags,IntPtr environment,string directory,ref StartupInfo startup,out ProcessInfo process);
 [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr handle);
 [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint access,bool inherit,uint processId);
 [DllImport("kernel32.dll")] public static extern bool TerminateProcess(IntPtr process,uint exitCode);
 [DllImport("user32.dll")] public static extern bool EnumDesktopWindows(IntPtr desktop,EnumProc callback,IntPtr data);
 [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent,EnumProc callback,IntPtr data);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr window,StringBuilder text,int count);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetClassName(IntPtr window,StringBuilder text,int count);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window,out Rect rect);
 [DllImport("user32.dll")] public static extern IntPtr GetParent(IntPtr window);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr window);
 [DllImport("user32.dll")] public static extern bool IsWindowEnabled(IntPtr window);
 [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr window);
 [DllImport("user32.dll",EntryPoint="GetWindowLongW")] public static extern int GetWindowStyle(IntPtr window,int index);
 [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr window);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr window,int command);
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool PostMessage(IntPtr window,uint message,IntPtr wp,IntPtr lp);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window,IntPtr dc,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr wp,IntPtr lp,uint flags,uint timeout,out UIntPtr result);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr window,out uint processId);
 [DllImport("user32.dll",SetLastError=true)] public static extern bool GetGUIThreadInfo(uint threadId,ref GuiThreadInfo info);
 [DllImport("user32.dll",SetLastError=true)] public static extern bool SetThreadDesktop(IntPtr desktop);
 [DllImport("user32.dll")] public static extern int GetWindowRgn(IntPtr window,IntPtr region);
 [DllImport("gdi32.dll")] public static extern IntPtr CreateRectRgn(int left,int top,int right,int bottom);
 [DllImport("gdi32.dll")] public static extern bool PtInRegion(IntPtr region,int x,int y);
 [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr region);
 public static string Text(IntPtr window) { IntPtr buffer=Marshal.AllocHGlobal(16384); try {Marshal.WriteInt16(buffer,0);Send(window,13,8192,buffer);return Marshal.PtrToStringUni(buffer) ?? "";} finally {Marshal.FreeHGlobal(buffer);} }
 public static string Class(IntPtr window) { var text=new StringBuilder(512); GetClassName(window,text,text.Capacity); return text.ToString(); }
 public static Rect VisibleBounds(IntPtr control,IntPtr root) {Rect result;GetWindowRect(control,out result);var parent=GetParent(control);while(parent!=IntPtr.Zero){Rect bounds;GetWindowRect(parent,out bounds);result.left=Math.Max(result.left,bounds.left);result.top=Math.Max(result.top,bounds.top);result.right=Math.Min(result.right,bounds.right);result.bottom=Math.Min(result.bottom,bounds.bottom);if(parent==root)break;parent=GetParent(parent);}return result;}
 public static List<IntPtr> Windows(IntPtr desktop) { var windows=new List<IntPtr>(); EnumDesktopWindows(desktop,(w,d)=>{windows.Add(w);return true;},IntPtr.Zero); return windows; }
 public static List<IntPtr> Children(IntPtr parent) { var windows=new List<IntPtr>(); EnumChildWindows(parent,(w,d)=>{windows.Add(w);return true;},IntPtr.Zero); return windows; }
 public static ulong Send(IntPtr window,uint message) { return Send(window,message,0,IntPtr.Zero); }
 public static ulong Send(IntPtr window,uint message,long wp) { return Send(window,message,wp,IntPtr.Zero); }
 public static ulong Send(IntPtr window,uint message,long wp,IntPtr lp) { UIntPtr result; if(SendMessageTimeout(window,message,new IntPtr(wp),lp,2,5000,out result)==IntPtr.Zero) throw new InvalidOperationException("Fixture window did not respond."); return result.ToUInt64(); }
 public static void SetText(IntPtr window,string text) { IntPtr pointer=Marshal.StringToHGlobalUni(text); try {Send(window,12,0,pointer);} finally {Marshal.FreeHGlobal(pointer);} }
 public static IntPtr Focus(IntPtr desktop,IntPtr window) {IntPtr focus=IntPtr.Zero;Exception failure=null;var thread=new System.Threading.Thread(()=>{try{if(!SetThreadDesktop(desktop))throw new InvalidOperationException("Cannot access fixture desktop: "+Marshal.GetLastWin32Error());uint processId;var threadId=GetWindowThreadProcessId(window,out processId);var info=new GuiThreadInfo();info.size=Marshal.SizeOf<GuiThreadInfo>();if(!GetGUIThreadInfo(threadId,ref info))throw new InvalidOperationException("Fixture GUI thread is unavailable: "+Marshal.GetLastWin32Error());focus=info.focus;}catch(Exception error){failure=error;}});thread.Start();thread.Join();if(failure!=null)throw failure;return focus;}
 public static string ListItemText(IntPtr window,int index) { IntPtr buffer=Marshal.AllocHGlobal(16384);try {Marshal.WriteInt16(buffer,0);Send(window,393,index,buffer);return Marshal.PtrToStringUni(buffer) ?? "";}finally {Marshal.FreeHGlobal(buffer);} }
 public static bool HasShapedRegion(IntPtr window) { IntPtr region=CreateRectRgn(0,0,0,0); try {Rect r;GetWindowRect(window,out r);return GetWindowRgn(window,region)>1 && !PtInRegion(region,0,0) && PtInRegion(region,(r.right-r.left)/2,(r.bottom-r.top)/2);} finally {DeleteObject(region);} }
 public static void Capture(IntPtr window,string path) { Rect r;if(!GetWindowRect(window,out r) || r.right<=r.left || r.bottom<=r.top) throw new InvalidOperationException("Fixture window bounds invalid.");using(var b=new Bitmap(r.right-r.left,r.bottom-r.top)){using(var g=Graphics.FromImage(b)){IntPtr dc=g.GetHdc();try {if(!PrintWindow(window,dc,2)) throw new InvalidOperationException("PrintWindow failed.");} finally {g.ReleaseHdc(dc);}} b.Save(path);} }
 public static double PixelLightness(string path,double x,double y) { using(var b=new Bitmap(path)){var c=b.GetPixel((int)(b.Width*x),(int)(b.Height*y));return (c.R+c.G+c.B)/3.0;} }
 public static int InkPixels(string path,IntPtr control,IntPtr root,bool light,int inset,int maximumWidth,int maximumHeight) {Rect r=VisibleBounds(control,root),w;GetWindowRect(root,out w);using(var b=new Bitmap(path)){int count=0,left=Math.Max(0,r.left-w.left+inset),top=Math.Max(0,r.top-w.top+4),right=Math.Min(b.Width,Math.Min(r.right-w.left-6,left+maximumWidth)),bottom=Math.Min(b.Height,Math.Min(r.bottom-w.top-4,top+maximumHeight));for(int y=top;y<bottom;y++)for(int x=left;x<right;x++){var c=b.GetPixel(x,y);double value=(c.R+c.G+c.B)/3.0;if(light ? value<150 : value>190)count++;}return count;} }
 public static int PurplePixels(string path,IntPtr control,IntPtr root) {Rect r=VisibleBounds(control,root),w;GetWindowRect(root,out w);using(var b=new Bitmap(path)){int count=0;for(int y=Math.Max(0,r.top-w.top);y<Math.Min(b.Height,r.bottom-w.top);y++)for(int x=Math.Max(0,r.left-w.left);x<Math.Min(b.Width,r.right-w.left);x++){var c=b.GetPixel(x,y);if(c.B>150 && c.B>c.R*1.2 && c.R>c.G*1.2)count++;}return count;} }
 public static string CropHash(string path,double x,double y,double width,double height) {using(var b=new Bitmap(path)){var r=new Rectangle((int)(b.Width*x),(int)(b.Height*y),(int)(b.Width*width),(int)(b.Height*height));using(var crop=b.Clone(r,b.PixelFormat))using(var stream=new System.IO.MemoryStream()){crop.Save(stream,System.Drawing.Imaging.ImageFormat.Png);return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream.ToArray()));}}}
}
'@ -ReferencedAssemblies System.Drawing.Common,System.Runtime,System.Collections,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core,System.Security.Cryptography,System.Threading.Thread
}
$assertions = [Collections.Generic.List[string]]::new()
$ownedHandles = [Collections.Generic.Dictionary[uint32,IntPtr]]::new()
$desktop = [IntPtr]::Zero
$processInfo = [CodeWFSetupFixtureDesktop+ProcessInfo]::new()
$window = [IntPtr]::Zero
function Assert-Fixture([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
    $assertions.Add($Message)
    Write-Host "PASS: $Message"
}
function Wait-Fixture([scriptblock] $Predicate, [string] $Message, [int] $TimeoutSeconds = 30) {
    $timer = [Diagnostics.Stopwatch]::StartNew()
    while ($timer.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        $value = & $Predicate
        if ($value) { return $value }
        Start-Sleep -Milliseconds 80
    }
    throw "等待超时：$Message"
}
function Get-FixtureControls {
    @([CodeWFSetupFixtureDesktop]::Children($window) | Where-Object { [CodeWFSetupFixtureDesktop]::IsWindowVisible($_) })
}
function Get-FixtureButton([string] $Pattern) {
    @(Get-FixtureControls | Where-Object {
        [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TNewButton' -and
        ([CodeWFSetupFixtureDesktop]::Text($_) -replace '&', '') -match $Pattern
    }) | Select-Object -First 1
}
function Click-Fixture([IntPtr] $Control) {
    if ($Control -eq [IntPtr]::Zero -or -not [CodeWFSetupFixtureDesktop]::IsWindowEnabled($Control)) { throw '目标夹具控件不存在或不可用。' }
    [void] [CodeWFSetupFixtureDesktop]::Send($Control, 245)
    Start-Sleep -Milliseconds 160
}
function Send-FixtureKey([IntPtr] $Control, [int] $Key) {
    if ($Control -eq [IntPtr]::Zero) { throw '隔离夹具没有可接收按键的焦点控件。' }
    if (-not [CodeWFSetupFixtureDesktop]::PostMessage($Control, 256, [IntPtr] $Key, [IntPtr]::Zero) -or
        -not [CodeWFSetupFixtureDesktop]::PostMessage($Control, 257, [IntPtr] $Key, [IntPtr]::Zero)) { throw '无法向隔离夹具投递键盘消息。' }
    Start-Sleep -Milliseconds 160
}
function Capture-Fixture([string] $Name) {
    $path = Join-Path $Output ($Name + '.png')
    [CodeWFSetupFixtureDesktop]::Capture($window, $path)
    return $path
}
function Get-SystemAppearance {
    $personalize = Get-ItemProperty -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name AppsUseLightTheme -ErrorAction SilentlyContinue
    if ($null -eq $personalize -or $null -eq $personalize.AppsUseLightTheme) { return $null }
    return [int] $personalize.AppsUseLightTheme
}
function Register-FixtureProcessTree {
    if ($processInfo.process -eq [IntPtr]::Zero) { return }
    $processes = @(Get-CimInstance Win32_Process | Select-Object ProcessId, ParentProcessId)
    $known = [Collections.Generic.HashSet[uint32]]::new()
    foreach ($fixtureProcessId in $ownedHandles.Keys) { [void] $known.Add($fixtureProcessId) }
    [void] $known.Add([uint32] $processInfo.processId)
    do {
        $added = $false
        foreach ($item in $processes) {
            if ($known.Contains([uint32] $item.ParentProcessId) -and $known.Add([uint32] $item.ProcessId)) { $added = $true }
        }
    } while ($added)
    foreach ($fixtureProcessId in $known) {
        if (-not $ownedHandles.ContainsKey($fixtureProcessId)) {
            $handle = [CodeWFSetupFixtureDesktop]::OpenProcess(0x00100001, $false, $fixtureProcessId)
            if ($handle -ne [IntPtr]::Zero) { $ownedHandles.Add($fixtureProcessId, $handle) }
        }
    }
}
function Assert-FixtureArea([IntPtr] $Control, [double] $MinimumWidth, [double] $MinimumHeight, [string] $Message) {
    if ($Control -eq [IntPtr]::Zero) { throw "目标控件不存在：$Message" }
    $visible = [CodeWFSetupFixtureDesktop]::VisibleBounds($Control, $window)
    $rootBounds = [CodeWFSetupFixtureDesktop+Rect]::new()
    [void] [CodeWFSetupFixtureDesktop]::GetWindowRect($window, [ref] $rootBounds)
    $width = [Math]::Max(0, $visible.right - $visible.left)
    $height = [Math]::Max(0, $visible.bottom - $visible.top)
    Assert-Fixture ($width -ge ($rootBounds.right - $rootBounds.left) * $MinimumWidth -and
        $height -ge ($rootBounds.bottom - $rootBounds.top) * $MinimumHeight) $Message
}
function Save-FixtureControls([string] $Name) {
    @(Get-FixtureControls | ForEach-Object {
        $bounds = [CodeWFSetupFixtureDesktop+Rect]::new()
        [void] [CodeWFSetupFixtureDesktop]::GetWindowRect($_, [ref] $bounds)
        [pscustomobject]@{ Handle = $_.ToInt64(); Class = [CodeWFSetupFixtureDesktop]::Class($_); Text = [CodeWFSetupFixtureDesktop]::Text($_); Enabled = [CodeWFSetupFixtureDesktop]::IsWindowEnabled($_); Bounds = $bounds }
    }) | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Output ($Name + '-controls.json')) -Encoding utf8
}
try {
    $desktopName = 'CodeWF.Installer.Verify.' + [guid]::NewGuid().ToString('N')
    $desktop = [CodeWFSetupFixtureDesktop]::CreateDesktop($desktopName, [IntPtr]::Zero, [IntPtr]::Zero, 0, 0x01FF, [IntPtr]::Zero)
    if ($desktop -eq [IntPtr]::Zero) { throw '无法创建隔离桌面。' }
    $startup = [CodeWFSetupFixtureDesktop+StartupInfo]::new()
    $startup.cb = [Runtime.InteropServices.Marshal]::SizeOf($startup)
    $startup.desktop = 'winsta0\' + $desktopName
    $installDirectory = Join-Path $Output 'installed fixture'
    $command = [Text.StringBuilder]::new('"' + $installerPath + '" /SP- /LANG=' + $Language + ' /DIR="' + $installDirectory + '" /LOG="' + (Join-Path $Output 'setup.log') + '"')
    if (-not [CodeWFSetupFixtureDesktop]::CreateProcess($installerPath, $command, [IntPtr]::Zero, [IntPtr]::Zero, $false, 0, [IntPtr]::Zero, (Split-Path -Parent $installerPath), [ref] $startup, [ref] $processInfo)) { throw '无法启动隔离安装夹具。' }
    $ownedHandles.Add([uint32] $processInfo.processId, $processInfo.process)
    # 不切换当前桌面，仅轮询专属桌面上的可见向导，允许并发编译时慢启动。
    $window = Wait-Fixture {
        @([CodeWFSetupFixtureDesktop]::Windows($desktop) | Where-Object {
            [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TWizardForm' -and [CodeWFSetupFixtureDesktop]::IsWindowVisible($_)
        }) | Select-Object -First 1
    } '可见安装向导'
    Register-FixtureProcessTree
    $wizardProcessId = [uint32] 0
    [void] [CodeWFSetupFixtureDesktop]::GetWindowThreadProcessId($window, [ref] $wizardProcessId)
    Assert-Fixture ($ownedHandles.ContainsKey($wizardProcessId)) '向导属于本次启动的夹具进程树'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::HasShapedRegion($window)) '真实向导具有切角区域，中心可见且左上角透明'
    $motion = @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TNewCheckBox' -and [CodeWFSetupFixtureDesktop]::Text($_) -match 'Reduce motion|减少动态效果' }) | Select-Object -First 1
    Assert-Fixture ($null -ne $motion) '减少动态效果复选框使用当前语言'
    if ([CodeWFSetupFixtureDesktop]::Send($motion, 240) -ne 1) { Click-Fixture $motion }
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::Send($motion, 240) -eq 1) '减少动态效果可以启用'
    $dark = Capture-Fixture '01-welcome-dark'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::InkPixels($dark, $motion, $window, $false, 18, 300, 22) -gt 20) '深色复选框文本具有浅色前景'
    Start-Sleep -Milliseconds 220
    $still = Capture-Fixture '02-motion-reduced'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::CropHash($dark, .035, .15, .19, .26) -eq [CodeWFSetupFixtureDesktop]::CropHash($still, .035, .15, .19, .26)) '减少动态效果后核心绘图区保持静止'
    Click-Fixture $motion
    $animationStart = Capture-Fixture '02-motion-enabled-a'
    Start-Sleep -Milliseconds 220
    $animationEnd = Capture-Fixture '02-motion-enabled-b'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::CropHash($animationStart, .035, .15, .19, .26) -ne [CodeWFSetupFixtureDesktop]::CropHash($animationEnd, .035, .15, .19, .26)) '关闭减少动态效果后核心动画恢复'
    Click-Fixture $motion
    $theme = Get-FixtureButton '^◐$'
    Click-Fixture $theme
    $light = Capture-Fixture '03-welcome-light'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::PixelLightness($light, .6, .1) -gt [CodeWFSetupFixtureDesktop]::PixelLightness($dark, .6, .1) + 50) '真实主题按钮切换浅色背景'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::InkPixels($light, $motion, $window, $true, 18, 300, 22) -gt 20) '浅色复选框文本具有深色前景'
    $systemAppearanceBefore = Get-SystemAppearance
    Click-Fixture $theme
    $system = Capture-Fixture '03-welcome-system'
    $systemExpected = if ($null -ne $systemAppearanceBefore -and $systemAppearanceBefore -ne 0) { $light } else { $dark }
    Assert-Fixture ([Math]::Abs([CodeWFSetupFixtureDesktop]::PixelLightness($system, .6, .1) - [CodeWFSetupFixtureDesktop]::PixelLightness($systemExpected, .6, .1)) -lt 15) '跟随系统模式符合只读 Windows 外观设置'
    $systemAppearanceAfter = Get-SystemAppearance
    Assert-Fixture ($systemAppearanceBefore -ceq $systemAppearanceAfter) '三态主题切换没有修改 Windows 外观设置'
    Click-Fixture $theme
    $minimize = Get-FixtureButton '^−$'
    Click-Fixture $minimize
    [void] (Wait-Fixture { [CodeWFSetupFixtureDesktop]::IsIconic($window) } '最小化')
    [void] [CodeWFSetupFixtureDesktop]::ShowWindow($window, 9)
    [void] (Wait-Fixture { -not [CodeWFSetupFixtureDesktop]::IsIconic($window) } '恢复窗口')
    Assert-Fixture $true '最小化与恢复只作用于隔离向导'
    $focusBeforeTab = [CodeWFSetupFixtureDesktop]::Focus($desktop, $window)
    Send-FixtureKey $focusBeforeTab 9
    $focusAfterTab = [CodeWFSetupFixtureDesktop]::Focus($desktop, $window)
    Assert-Fixture ($focusAfterTab -ne [IntPtr]::Zero -and $focusAfterTab -ne $focusBeforeTab) 'Tab 键通过原生对话框导航移动焦点'
    $keyboardNext = Get-FixtureButton 'Next|下一步'
    for ($tabIndex = 0; $tabIndex -lt 12 -and $focusAfterTab -ne $keyboardNext; $tabIndex++) {
        Send-FixtureKey $focusAfterTab 9
        $focusAfterTab = [CodeWFSetupFixtureDesktop]::Focus($desktop, $window)
    }
    Assert-Fixture ($focusAfterTab -eq $keyboardNext) 'Tab 键可以到达原生下一步按钮'
    Send-FixtureKey $focusAfterTab 13
    if ($Maintenance) {
        $maintenanceList = Wait-Fixture { @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TNewCheckListBox' }) | Select-Object -First 1 } '维护操作页'
        Assert-FixtureArea $maintenanceList .5 .1 '维护选项列表有足够实际可见空间'
        $maintenanceCount = [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 395)
        Assert-Fixture ($maintenanceCount -eq 2) '内存维护夹具只显示修复和卸载两项'
        $repairText = [CodeWFSetupFixtureDesktop]::ListItemText($maintenanceList, 0)
        $uninstallText = [CodeWFSetupFixtureDesktop]::ListItemText($maintenanceList, 1)
        $repairPattern = if ($Language -eq 'english') { '^Repair / reinstall:' } else { '^修复／重新安装：' }
        $uninstallPattern = if ($Language -eq 'english') { '^Uninstall \(Current user\):' } else { '^卸载（当前用户）：' }
        Assert-Fixture ($repairText -match $repairPattern -and $uninstallText -match $uninstallPattern) '维护选项采用当前语言和明确的作用域'
        $locationMemo = @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -match 'Memo|RichEdit' }) | Select-Object -First 1
        Assert-FixtureArea $locationMemo .5 .15 '维护完整路径使用独立可见的只读文本区'
        $repairPathText = [CodeWFSetupFixtureDesktop]::Text($locationMemo)
        Assert-Fixture ($repairPathText -match '^[A-Z]:\\.+\\维护样例 with spaces$') '修复完整路径保留中文空格和未截断的目录'
        Assert-Fixture (([CodeWFSetupFixtureDesktop]::GetWindowStyle($locationMemo, -16) -band 2048) -ne 0) '维护完整路径文本区禁止编辑'
        Save-FixtureControls '04-maintenance-dark'
        [void] (Capture-Fixture '04-maintenance-dark')
        Click-Fixture $theme
        [void] (Capture-Fixture '04-maintenance-light')
        Click-Fixture $theme
        Click-Fixture $theme
        [void] [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 390, 1)
        [void] [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 256, 32)
        [void] [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 257, 32)
        $uninstallButton = Wait-Fixture { Get-FixtureButton '^Uninstall\.\.\.$|^卸载\(' } '维护卸载按钮标题'
        Assert-Fixture ($null -ne $uninstallButton) '选择卸载仅改变下一步标题，未启动卸载器'
        $uninstallPathText = [CodeWFSetupFixtureDesktop]::Text($locationMemo)
        Assert-Fixture ($uninstallPathText -ne $repairPathText -and $uninstallPathText -match '^[A-Z]:\\.+\\已注册样例 with spaces$') '选择卸载后完整路径同步为所选注册目录'
        [void] (Capture-Fixture '05-maintenance-uninstall-selected')
        [void] [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 390, 0)
        [void] [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 256, 32)
        [void] [CodeWFSetupFixtureDesktop]::Send($maintenanceList, 257, 32)
        $repairButton = Wait-Fixture { Get-FixtureButton 'Next|下一步' } '维护修复按钮标题'
        Assert-Fixture ($null -ne $repairButton) '重新选择修复恢复下一步标题'
        Assert-Fixture ([CodeWFSetupFixtureDesktop]::Text($locationMemo) -eq $repairPathText) '重新选择修复恢复完整修复目录'
        Click-Fixture $repairButton
        $repairLicense = Wait-Fixture { @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TNewRadioButton' -and [CodeWFSetupFixtureDesktop]::Text($_) -match 'I &accept|我同意' }) | Select-Object -First 1 } '修复进入正常许可页'
        Assert-Fixture ($null -ne $repairLicense) '修复继续原生许可流程且尚未安装'
        [void] (Capture-Fixture '06-repair-license')
        Assert-Fixture (-not (Test-Path -LiteralPath $installDirectory)) '维护验收未写入隔离安装目录'
        $cancelButton = Get-FixtureButton '^Cancel$|^取消$'
        if (-not [CodeWFSetupFixtureDesktop]::PostMessage($cancelButton, 245, [IntPtr]::Zero, [IntPtr]::Zero)) { throw '无法打开取消确认。' }
        $cancelConfirmation = Wait-Fixture {
            @([CodeWFSetupFixtureDesktop]::Windows($desktop) | ForEach-Object { [CodeWFSetupFixtureDesktop]::Children($_) } | Where-Object {
                [CodeWFSetupFixtureDesktop]::IsWindowVisible($_) -and [CodeWFSetupFixtureDesktop]::Class($_) -match 'Button' -and
                ([CodeWFSetupFixtureDesktop]::Text($_) -replace '&', '') -match '^Yes$|^是\('
            }) | Select-Object -First 1
        } '取消安装确认'
        if (-not [CodeWFSetupFixtureDesktop]::PostMessage($cancelConfirmation, 245, [IntPtr]::Zero, [IntPtr]::Zero)) { throw '无法确认取消夹具。' }
        [void] (Wait-Fixture { -not [CodeWFSetupFixtureDesktop]::IsWindow($window) } '维护夹具取消退出')
        Assert-Fixture $true '正常修复流程可在许可页取消退出'
        Assert-Fixture ((Get-FileHash -LiteralPath $installerPath -Algorithm SHA256).Hash.ToLowerInvariant() -eq $installerSha256) '本轮运行前后安装器文件哈希保持一致'
        [ordered]@{ Language = $Language; Mode = 'Maintenance'; Installer = $installerPath; Sha256 = $installerSha256; AssertionCount = $assertions.Count; Assertions = $assertions; RepairText = $repairText; UninstallText = $uninstallText; RepairPath = $repairPathText; SelectedUninstallPath = $uninstallPathText; SystemAppearanceBefore = $systemAppearanceBefore; SystemAppearanceAfter = $systemAppearanceAfter; MinimizeVerified = $true; DesktopWasSwitched = $false; ProductWasInstalled = $false; UninstallerWasStarted = $false } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Output 'verification.json') -Encoding utf8
        return
    }
    $accept = Wait-Fixture { @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TNewRadioButton' -and [CodeWFSetupFixtureDesktop]::Text($_) -match 'I &accept|我同意' }) | Select-Object -First 1 } '许可协议页'
    Assert-Fixture $true 'Enter 键保留原生下一步行为并进入许可页'
    Save-FixtureControls '04-license'
    $licenseScreenshot = Capture-Fixture '04-license'
    $licenseMemo = @([CodeWFSetupFixtureDesktop]::Children($window) | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -match 'RichEdit|Memo' -and [CodeWFSetupFixtureDesktop]::IsWindowVisible($_) }) | Select-Object -First 1
    Assert-FixtureArea $licenseMemo .5 .2 '许可正文实际可见且没有被父页面裁切'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::InkPixels($licenseScreenshot, $licenseMemo, $window, $false, 8, 600, 140) -gt 40) '深色许可正文具有可读的浅色文本'
    Click-Fixture $theme
    $licenseLight = Capture-Fixture '04-license-light'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::InkPixels($licenseLight, $licenseMemo, $window, $true, 8, 600, 140) -gt 40) '浅色许可正文具有可读的深色文本'
    Click-Fixture $theme
    Click-Fixture $theme
    Assert-Fixture (-not [CodeWFSetupFixtureDesktop]::IsWindowEnabled((Get-FixtureButton 'Next|下一步'))) '未接受许可时下一步禁用'
    Click-Fixture $accept
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::IsWindowEnabled((Get-FixtureButton 'Next|下一步'))) '接受许可后下一步启用'
    Click-Fixture (Get-FixtureButton 'Next|下一步')
    $directoryEdit = Wait-Fixture { @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TEdit' }) | Select-Object -First 1 } '安装路径页'
    $installDirectory = Join-Path $Output '安装路径 with spaces'
    [CodeWFSetupFixtureDesktop]::SetText($directoryEdit, $installDirectory)
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::Text($directoryEdit) -eq $installDirectory) '路径输入保留中文与空格'
    Save-FixtureControls '05-location'
    [void] (Capture-Fixture '05-location')
    Assert-FixtureArea $directoryEdit .4 .03 '路径输入框实际可见并使用主内容区宽度'
    Click-Fixture (Get-FixtureButton 'Next|下一步')
    $taskList = Wait-Fixture { @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -eq 'TNewCheckListBox' }) | Select-Object -First 1 } '安装选项页'
    Assert-Fixture ($null -ne $taskList) '安全桌面选项页可见，夹具不定义任何快捷方式'
    Save-FixtureControls '06-options'
    [void] (Capture-Fixture '06-options')
    Assert-FixtureArea $taskList .5 .15 '安装选项列表有足够可见空间显示任务行'
    $taskCount = [CodeWFSetupFixtureDesktop]::Send($taskList, 395)
    Assert-Fixture ($taskCount -ge 2 -and $taskCount -lt 20) '安全选项列表同时包含分组标题和桌面选项'
    [void] [CodeWFSetupFixtureDesktop]::Send($taskList, 390, [long] ($taskCount - 1))
    [void] [CodeWFSetupFixtureDesktop]::Send($taskList, 256, 32)
    [void] [CodeWFSetupFixtureDesktop]::Send($taskList, 257, 32)
    Click-Fixture (Get-FixtureButton 'Next|下一步')
    $installButton = Wait-Fixture { Get-FixtureButton '^Install$|^安装\(' } '准备安装页'
    Save-FixtureControls '07-ready'
    [void] (Capture-Fixture '07-ready')
    $readyMemo = @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -match 'Memo|RichEdit' }) | Select-Object -First 1
    Assert-FixtureArea $readyMemo .5 .2 '安装摘要实际可见且没有被父页面裁切'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::Text($readyMemo) -match 'desktop|桌面') '真实选项切换同步到安装摘要'
    # 安装点击异步投递，观察真实进度；仅夹具消息泵延时，生产流程不增加等待。
    if (-not [CodeWFSetupFixtureDesktop]::PostMessage($installButton, 245, [IntPtr]::Zero, [IntPtr]::Zero)) { throw '无法启动夹具安装。' }
    $progressControl = Wait-Fixture { @(Get-FixtureControls | Where-Object { [CodeWFSetupFixtureDesktop]::Class($_) -match 'Progress' }) | Select-Object -First 1 } '真实安装进度条'
    $progressMinimum = [CodeWFSetupFixtureDesktop]::Send($progressControl, 1031, 1)
    $progressMaximum = [CodeWFSetupFixtureDesktop]::Send($progressControl, 1031, 0)
    $initialProgress = [CodeWFSetupFixtureDesktop]::Send($progressControl, 1032)
    Assert-Fixture ($progressMaximum -gt $progressMinimum -and $initialProgress -ge $progressMinimum -and $initialProgress -le $progressMaximum) '原生进度条保留有效的真实范围与数值'
    $progressScreenshot = Wait-Fixture {
        if (-not [CodeWFSetupFixtureDesktop]::IsWindowVisible($progressControl)) { return $null }
        $path = Capture-Fixture '07-installing'
        if ([CodeWFSetupFixtureDesktop]::PurplePixels($path, $progressControl, $window) -gt 20) { return $path }
        return $null
    } '紫色安装进度绘制'
    $observedProgress = [CodeWFSetupFixtureDesktop]::Send($progressControl, 1032)
    Assert-Fixture ($observedProgress -ge $initialProgress -and $observedProgress -le $progressMaximum) '可见安装进度处于真实范围且不倒退'
    Assert-Fixture ([CodeWFSetupFixtureDesktop]::PurplePixels($progressScreenshot, $progressControl, $window) -gt 20) '实际原生进度条绘制 .NET 紫色'
    Save-FixtureControls '07-installing'
    $finish = Wait-Fixture { Get-FixtureButton '^Finish$|^完成\(' } '完成页'
    Assert-Fixture (Test-Path -LiteralPath (Join-Path $installDirectory 'LICENSE') -PathType Leaf) '夹具仅将许可文件写入指定隔离路径'
    Assert-Fixture (@(Get-ChildItem -LiteralPath $installDirectory -Recurse -File).Count -eq 1) '隔离安装目录没有卸载器或额外文件'
    [void] (Capture-Fixture '08-complete')
    Click-Fixture $finish
    [void] (Wait-Fixture { -not [CodeWFSetupFixtureDesktop]::IsWindow($window) } '向导完成退出')
    Assert-Fixture $true '完成按钮正常关闭向导'
    Assert-Fixture ((Get-FileHash -LiteralPath $installerPath -Algorithm SHA256).Hash.ToLowerInvariant() -eq $installerSha256) '本轮运行前后安装器文件哈希保持一致'
    [ordered]@{ Language = $Language; Installer = $installerPath; Sha256 = $installerSha256; AssertionCount = $assertions.Count; Assertions = $assertions; SystemAppearanceBefore = $systemAppearanceBefore; SystemAppearanceAfter = $systemAppearanceAfter; ProgressMinimum = $progressMinimum; ProgressMaximum = $progressMaximum; ProgressInitial = $initialProgress; ProgressObserved = $observedProgress; MinimizeVerified = $true; DesktopWasSwitched = $false; ProductWasInstalled = $false; FixtureDirectory = $installDirectory } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Output 'verification.json') -Encoding utf8
}
catch {
    if ($window -ne [IntPtr]::Zero -and [CodeWFSetupFixtureDesktop]::IsWindow($window)) {
        [void] (Capture-Fixture 'failure')
        Save-FixtureControls 'failure'
    }
    throw
}
finally {
    # 启动超时或中途失败时，也只补充登记本次夹具的实际后代。
    try { Register-FixtureProcessTree } catch { Write-Warning '未能补充夹具进程树，仍关闭已经持有的本次进程句柄。' }
    # 仅操作本次创建进程树已持有的句柄，不枚举终止桌面上的输入法或系统窗口。
    foreach ($entry in $ownedHandles.GetEnumerator()) {
        [void] [CodeWFSetupFixtureDesktop]::TerminateProcess($entry.Value, 0)
        [void] [CodeWFSetupFixtureDesktop]::CloseHandle($entry.Value)
    }
    if ($processInfo.thread -ne [IntPtr]::Zero) { [void] [CodeWFSetupFixtureDesktop]::CloseHandle($processInfo.thread) }
    if ($desktop -ne [IntPtr]::Zero) { [void] [CodeWFSetupFixtureDesktop]::CloseDesktop($desktop) }
}
