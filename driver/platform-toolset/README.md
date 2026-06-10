# Platform Toolset: WindowsKernelModeDriver10.0

WDK 10.0.26100 的独立安装器不向 VS 2022 注册 `WindowsKernelModeDriver10.0` 平台工具集。旧版 WDK.vsix (22621) 被 VS 17.11 拒绝（版本过低）。

## 手动安装

以管理员身份将这两个文件复制到：

```
C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\VC\v170\Platforms\x64\PlatformToolsets\WindowsKernelModeDriver10.0\
```

| 文件 | 作用 |
|------|------|
| `Toolset.props` | 设置 WDK 属性、MSVC 工具链路径、内核编译标志 |
| `Toolset.targets` | 导入 WDK 构建目标和 C++ 通用目标 |

## 安装命令（PowerShell 管理员）

```powershell
$dest = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\VC\v170\Platforms\x64\PlatformToolsets\WindowsKernelModeDriver10.0'
New-Item -Path $dest -ItemType Directory -Force
Copy-Item "$PSScriptRoot\Toolset.props" "$dest\Toolset.props" -Force
Copy-Item "$PSScriptRoot\Toolset.targets" "$dest\Toolset.targets" -Force
```

## 验证

```powershell
msbuild OpenMagicKeyboardA2450Filter.vcxproj /p:Configuration=Debug /p:Platform=x64
# 应输出: 已成功生成。0 个警告 0 个错误
```
