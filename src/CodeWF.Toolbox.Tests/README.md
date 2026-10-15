# 测试与系统配置边界

默认 `dotnet test` 执行六项隔离测试，并跳过 `ModifyStartupOnDifferentSystemsTestAsync`。该旧用例会修改当前用户的开机启动配置，仅允许在一次性测试系统中手动验收；保留 `SystemIntegration` 分类，CI 继续使用 `--filter 'Category!=SystemIntegration'` 排除它。

2026-10-16 的 recheck24 验收中，临时验证脚本漏用了 CI 的过滤条件，执行了该系统用例。Windows 路径为 `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`，值名为 `CodeWF.Toolbox`。用例启用时仅在值不存在时写入，禁用时删除已有值；通过结果无法区分测试前该值不存在还是已存在。

本次未采集运行前的注册表值或快照，也未收集可恢复备份。随后只读检查确认该值当前不存在，未猜测恢复。原始未过滤日志、按 CI 条件重跑的六项测试结果及独立事故回执均已保留；这些通过结果不能证明运行前的启动配置未改变。

本次修正仅增加测试默认跳过条件及验收记录，不修改产品运行逻辑、安装器或注册表。
