## 配置

设置全局用户信息：

```shell
git config --global user.name "[name]"
git config --global user.email "[email]"
```

## 开始使用

创建 Git 仓库：

```shell
git init
```

克隆已有仓库：

```shell
git clone [url]
```

## 提交

提交所有已跟踪文件的修改：

```shell
git commit -am "[commit message]"
```

把新修改追加到上一次提交：

```shell
git commit --amend --no-edit
```

## 常见回退

修改上一次提交信息：

```shell
git commit --amend
```

撤销最近一次提交并保留修改：

```shell
git reset HEAD~1
```

撤销最近 N 次提交并保留修改：

```shell
git reset HEAD~N
```

撤销最近一次提交并丢弃修改：

```shell
git reset HEAD~1 --hard
```

把本地分支重置到远端状态：

```shell
git fetch origin
git reset --hard origin/[branch-name]
```

## 其他

将本地 master 分支改名为 main：

```shell
git branch -m master main
```
