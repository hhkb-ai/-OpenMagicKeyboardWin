# OpenMagicKeyboardA2450Filter

Apple Magic Keyboard A2450 的 HID Filter Driver。

## 当前状态

**MVP-A Build Scaffold**。驱动源码和 WDK 项目骨架已就绪，可尝试在 Visual Studio + WDK 环境中编译。**不可安装，不可绑定真实设备。**

## HID Collection 总览（真实设备验证）

| 接口 | UsagePage | Usage | InputReportLen | 说明 |
|------|-----------|-------|---------------|------|
| COL01 | 0x0001 | 0x0006 | 10 | 标准键盘 |
| **COL02** | **0x000C** | 0x0001 | **2** | **Consumer Control（媒体键通道）** |
| COL03 | 0xFF00 | 0x0006 | 65 | Vendor Defined（暂不处理） |

## MVP-A 功能范围

| 功能 | 状态 |
|------|------|
| Fn / Globe → Left Ctrl | ✅ 实现 |
| Physical Left Ctrl → internal FnLayer | ✅ 实现 |
| FnLayer + Backspace → Delete | ✅ 实现 |
| FnLayer + ↑ → PageUp | ✅ 实现 |
| FnLayer + ↓ → PageDown | ✅ 实现 |
| FnLayer + ← → Home | ✅ 实现 |
| FnLayer + → → End | ✅ 实现 |
| F7~F12 媒体键 | ❌ MVP-B |
| 蓝牙模式 | ❌ MVP-D |

## 文件说明

| 文件 | 说明 |
|------|------|
| `A2450Report.h` | HID Report 结构定义和常量 |
| `ReportTransform.h` | 转换函数接口和状态结构 |
| `ReportTransform.c` | 核心转换逻辑实现 |
| `Driver.c` | KMDF DriverEntry 和 DeviceAdd |
| `Device.c` | 设备上下文和初始化 |
| `Filter.c` | HID IOCTL 拦截设计（伪代码） |
| `FnCtrlStateMachine.md` | 状态机设计、伪代码、边界情况、测试用例 |
| `OpenMagicKeyboardA2450Filter.inf.template` | INF 设计模板（不可安装） |
| `OpenMagicKeyboardA2450Filter.vcxproj` | WDK/KMDF 编译项目 |
| `OpenMagicKeyboardA2450Filter.vcxproj.filters` | VS 项目过滤器 |
| `README.md` | 本文件 |

## 构建要求

- Visual Studio 2022（C++ 工作负载）
- Windows Driver Kit (WDK) for Windows 11
- KMDF 1.15+

## 构建方式

```
打开 OpenMagicKeyboardA2450Filter.vcxproj
选择 Release | x64
Build（只编译，不安装）
```

编译产物：`OpenMagicKeyboardA2450Filter.sys`

**此 .sys 文件不得在任何机器上安装。**

## Current WDK Build Status

The project file is recognized by MSBuild, but build currently fails on machines without WDK:

```text
MSB8020: WindowsKernelModeDriver10.0 build tools not found.
```

Install WDK before attempting to build. Do not install or deploy the driver.

## 相关文档

- `docs/a2450-usb-hid-report-structure.md` — HID Report 结构
- `docs/a2450-filter-driver-mvp-plan.md` — 驱动 MVP 设计
- `docs/a2450-transform-parity-check.md` — C#/C 转换逻辑一致性
- `docs/driver-build-and-test-safety.md` — 构建与测试安全规范
- `docs/roadmap.md` — 项目路线图

## 用户态测试

```bash
dotnet test tests/OpenMagicKeyboard.TransformTests/
# 37/37 通过
```

## 安全边界

- 不安装驱动
- 不开启 TESTSIGNING
- 不关闭 Secure Boot
- 不修改注册表
- 不绑定真实设备
