-- 只配置刚挂载的临时镜像窗口，不改 Finder 全局偏好。
on run argv
    if (count of argv) is not 2 then error "Expected mount path and app bundle name."
    set mountPath to item 1 of argv
    set appName to item 2 of argv
    set mountedFolder to POSIX file mountPath as alias
    set backgroundFile to POSIX file (mountPath & "/.background/codewf.png") as alias
    tell application "Finder"
        open mountedFolder
        set imageWindow to container window of mountedFolder
        set current view of imageWindow to icon view
        set toolbar visible of imageWindow to false
        set statusbar visible of imageWindow to false
        set sidebar width of imageWindow to 0
        set bounds of imageWindow to {120, 120, 840, 600}
        set imageOptions to icon view options of imageWindow
        set arrangement of imageOptions to not arranged
        set icon size of imageOptions to 100
        set text size of imageOptions to 14
        set background picture of imageOptions to backgroundFile
        set position of item appName of mountedFolder to {180, 235}
        set position of item "Applications" of mountedFolder to {540, 235}
        close imageWindow
        open mountedFolder
        update mountedFolder without registering applications
        delay 1
        close container window of mountedFolder
    end tell
end run
