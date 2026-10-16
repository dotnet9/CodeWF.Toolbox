"""编译并运行无向导的维护逻辑夹具，生产及完整引擎仅编译。"""

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile


HERE = Path(__file__).resolve().parent
ASSERTIONS_PER_FIXTURE = 57


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def definition(name, value):
    # 所有生成路径只经过 ISPP 字符串，不交给 shell 解释。
    require('"' not in value and "\n" not in value and "\r" not in value,
            f"预处理参数包含非法字符：{name}")
    return f'#define {name} "{value}"\n'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def compile_fixture(compiler, installer_root, output, name, app_id,
                    expected_app_id="", execute=False, integrated=False):
    report = output / f"{name}.txt"
    preprocessed = output / f"{name}-preprocessed.iss"
    executable = output / f"{name}.exe"
    # 重用输出目录时移除本夹具的旧收据，禁止旧成功结果掩盖本轮失败。
    for stale in (report, preprocessed, executable):
        stale.unlink(missing_ok=True)

    source = "#define AppVersion \"1.0.0\"\n"
    if execute or integrated:
        source += "#define CodeWFSetupFixture\n"
    source += definition("ExpectedAppId", expected_app_id)
    # 报告路径进入 Pascal 单引号字面量，目录中的单引号必须双写。
    source += definition("TestReport", report.as_posix().replace("'", "''"))
    source += definition("CodeWFSetupAssetPath", installer_root.as_posix())
    source += f"""[Setup]
AppId={app_id}
AppName=CodeWF Maintenance Safe Fixture
AppVersion=1.0.0
DefaultDirName={{tmp}}\\CodeWF.Maintenance.NonInstallingFixture
PrivilegesRequired=lowest
Uninstallable=no
CreateUninstallRegKey=no
OutputDir={output.as_posix()}
OutputBaseFilename={name}
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "{(installer_root.parent / 'Languages/ChineseSimplified.isl').as_posix()}"
"""
    if integrated:
        source += f'#include "{(installer_root / "CodeWF.Setup.iss").as_posix()}"\n'
    source += f'#include "{(installer_root / "CodeWF.Setup.Maintenance.iss").as_posix()}"\n'
    if execute:
        source += f'#include "{(HERE / "Fixture.Maintenance.Assertions.iss").as_posix()}"\n'
    source += f'#expr SaveToFile("{preprocessed.as_posix()}")\n'
    script = output / f"{name}.iss"
    script.write_text(source, encoding="utf-8-sig")
    require(not re.search(r"(?im)^\[(Files|Registry|Icons|Run|UninstallRun|UninstallDelete)\]", source),
            "纯逻辑夹具不得声明安装或启动项目。")

    result = subprocess.run([str(compiler), "/Q", str(script)],
                            capture_output=True, text=True, encoding="utf-8",
                            errors="replace", timeout=60)
    (output / f"{name}-compile.log").write_text(result.stdout + result.stderr, encoding="utf-8")
    require(result.returncode == 0, f"{name} 编译失败：\n{result.stdout}{result.stderr}")
    require(executable.is_file() and preprocessed.is_file(), f"{name} 缺少编译或预处理产物。")
    processed = preprocessed.read_text(encoding="utf-8-sig")
    receipt = {"name": name, "appId": app_id, "compiled": True, "executed": False,
               "executableSha256": digest(executable), "preprocessedSha256": digest(preprocessed)}

    if execute:
        # 在执行之前检查实际预处理结果；不以源文件中存在 guard 代替证明。
        for pattern in (r"\bReg\w+\s*\(", r"\bShellExec\s*\(", r"\bExec\s*\(", r"\bexternal\s+'"):
            require(not re.search(pattern, processed, re.IGNORECASE),
                    f"{name} 的预处理结果仍含系统注册、进程或外部调用：{pattern}")
        body = processed[processed.index("function TestInitialize: Boolean;"):]
        require("Result := False;" in body and not re.search(r"Result\s*:=\s*True", body, re.IGNORECASE),
                "InitializeSetup 必须无条件返回 False，阻止创建向导。")
        require(not re.search(r"\b(CWMCreatePage|CWMInitializeWizard|CWInitializeWizard)\s*;", body),
                "纯逻辑断言不得调用页面创建或初始化。")
        require(not re.search(r"(?im)^\[(Files|Registry|Icons|Run|UninstallRun|UninstallDelete)\]", processed),
                "纯逻辑预处理结果不得声明安装或启动项目。")
        result = subprocess.run([str(executable), "/VERYSILENT", "/SUPPRESSMSGBOXES",
                                 "/NORESTART", "/LANG=english"],
                                capture_output=True, timeout=45,
                                creationflags=subprocess.CREATE_NO_WINDOW)
        require(report.is_file(), f"{name} 未生成本轮断言收据。")
        lines = report.read_text(encoding="utf-8-sig").splitlines()
        require(lines and lines[0] == f"PASS {ASSERTIONS_PER_FIXTURE}",
                f"{name} 断言失败：\n" + "\n".join(lines))
        require(len(lines[1:]) == ASSERTIONS_PER_FIXTURE and all(line.startswith("PASS: ") for line in lines[1:]),
                f"{name} 的断言数量或结果不完整。")
        receipt.update({"executed": True, "exitCode": result.returncode,
                        "assertions": ASSERTIONS_PER_FIXTURE, "report": str(report),
                        "reportSha256": digest(report),
                        "preprocessorProof": "No registry, ShellExec, Exec or external calls; no installation sections; InitializeSetup returns False before Wizard creation."})
    elif not integrated:
        # 反向证明生产分支仍保留原生读取及启动路径，只编译、绝不执行。
        require(all(token in processed for token in ("RegKeyExists(", "RegQueryStringValue(", "ShellExec(")),
                "生产分支未包含预期的注册及卸载启动代码。")
        require("TestInitialize" not in processed, "生产分支不应包含测试入口。")
    print(f"PASS {name}: " + (f"{ASSERTIONS_PER_FIXTURE} assertions" if execute else "compile only"), flush=True)
    return receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", default=os.environ.get("INNO_ISCC") or shutil.which("ISCC"),
                        help="ISCC.exe 路径；默认 INNO_ISCC 环境变量或 PATH。")
    parser.add_argument("--output", type=Path, help="输出目录；默认创建系统临时目录。")
    parser.add_argument("--installer-root", type=Path, default=HERE.parent,
                        help="安装引擎目录；默认使用本仓 scripts/installer。")
    args = parser.parse_args()
    require(sys.platform == "win32", "此验证器在 Windows 执行 Inno 编译产物。")
    require(args.compiler, "未找到 ISCC；请指定 --compiler，或设置 INNO_ISCC / PATH。")
    compiler = Path(args.compiler).resolve(strict=True)
    installer_root = args.installer_root.resolve(strict=True)
    require((installer_root / "CodeWF.Setup.Maintenance.iss").is_file(), "缺少维护模板。")
    output = args.output.resolve() if args.output else Path(tempfile.mkdtemp(
        prefix="codewf-maintenance-logic-", dir=os.environ.get("CODEWF_TEST_TMP")))
    output.mkdir(parents=True, exist_ok=True)
    # 只覆盖本测试自己的收据，不清空输出目录。
    (output / "receipt.json").unlink(missing_ok=True)
    source_files = [installer_root / name for name in
                    ("CodeWF.Setup.Maintenance.iss", "CodeWF.Setup.iss", "CodeWF.Setup.Progress.iss", "CodeWF.Setup.Messages.iss")]
    source_files += [Path(__file__).resolve(), HERE / "Fixture.Maintenance.Assertions.iss"]
    source_hashes = {str(path): digest(path) for path in source_files}
    cases = [("maintenance-plain", "CodeWF.Maintenance.Fixture.Only", "CodeWF.Maintenance.Fixture.Only"),
             ("maintenance-guid", "{{74EF99B5-9EB4-4D02-8164-8F78D465D08C}", "{74EF99B5-9EB4-4D02-8164-8F78D465D08C}"),
             ("maintenance-existing-double-close", "{{E7A1C2D4-3B5F-4A6E-9C8D-0B1A2C3D4E5F}}", "{E7A1C2D4-3B5F-4A6E-9C8D-0B1A2C3D4E5F}}")]
    rows = [compile_fixture(compiler, installer_root, output, name, app_id, expected, execute=True)
            for name, app_id, expected in cases]
    rows.append(compile_fixture(compiler, installer_root, output, "maintenance-production-compile-only",
                                cases[1][1]))
    rows.append(compile_fixture(compiler, installer_root, output, "maintenance-integrated-compile-only",
                                "CodeWF.Maintenance.Integrated.Fixture", integrated=True))
    require(all(digest(Path(path)) == expected for path, expected in source_hashes.items()),
            "验证过程中安装器或测试输入发生变化，请重新执行。")
    receipt = {"compiler": str(compiler), "compilerSha256": digest(compiler),
               "sourceSha256": source_hashes, "pureAssertions": sum(row.get("assertions", 0) for row in rows),
               "tests": rows,
               "limits": "Three pure fixtures executed before Wizard creation. Production maintenance and integrated visual engine compiled only. No real installation, product registration or uninstaller was used."}
    (output / "receipt.json").write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"PASS {receipt['pureAssertions']} pure assertions; receipt: {output / 'receipt.json'}", flush=True)


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError, subprocess.SubprocessError, ValueError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        sys.exit(1)
