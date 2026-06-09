# OpenMagicKeyboardWin — 会话上下文导出

日期：2026-06-09

---

## 一、项目基本信息

- 项目名称：OpenMagicKeyboardWin
- 本地路径：`D:\miaokongjianp\OpenMagicKeyboardWin`
- GitHub：`https://github.com/hhkb-ai/OpenMagicKeyboardWin`
- 目标设备：Apple Magic Keyboard A2450
- 目标平台：Windows 10/11

## 二、项目目标

让 Apple Magic Keyboard A2450 在 Windows 上支持：

1. Fn / Globe → Left Ctrl
2. Physical Left Ctrl → internal FnLayer
3. FnLayer + Backspace → Delete
4. FnLayer + ↑ → PageUp, ↓ → PageDown, ← → Home, → → End
5. FnLayer + F7~F12 → 媒体键（MVP-B）

## 三、USBPcap 抓包确认的关键发现

### HID Report 结构（COL01，10 字节）

| Byte | 字段 | 说明 |
|------|------|------|
| 0 | Report ID | 固定 0x01 |
| 1 | Modifier | bit 0 = Left Ctrl（A2450 实测，符合标准 HID） |
| 2 | Reserved | 0x00 |
| 3-8 | Key slots | HID Usage Codes (Usage Page 0x07) |
| 9 | Apple Fn | 0x00 = 释放, 0x02 = 按下 |

### Modifier 位定义（A2450 实测）

| Bit | 掩码 | 含义 |
|-----|------|------|
| 0 | 0x01 | Left Ctrl |
| 1 | 0x02 | Left Shift |
| 2 | 0x04 | Left Alt (Option) |
| 3 | 0x08 | Left GUI (Command) |
| 4 | 0x10 | Right Ctrl |
| 5 | 0x20 | Right Shift |
| 6 | 0x40 | Right Alt |
| 7 | 0x80 | Right GUI |

### HID Collection 总览（真实设备）

| 接口 | UsagePage | Usage | InputReportLen | 说明 |
|------|-----------|-------|---------------|------|
| COL01 | 0x0001 | 0x0006 | 10 | 标准键盘 |
| COL02 | 0x000C | 0x0001 | 2 | Consumer Control（媒体键通道） |
| COL03 | 0xFF00 | 0x0006 | 65 | Vendor Defined |
| MI_00 COL01 | 0xFF00 | 0x000B | 5 | Vendor Defined |
| MI_00 COL02 | 0xFF00 | 0x0014 | 3 | Vendor Defined |

**关键：COL02 才是 Consumer Control，不是 COL03。**

### USBPcap 原始报文样本

```
Idle:       01 00 00 00 00 00 00 00 00 00
Fn:         01 00 00 00 00 00 00 00 00 02
Left Ctrl:  01 01 00 00 00 00 00 00 00 00
Fn+Ctrl:    01 01 00 00 00 00 00 00 00 02
Backspace:  01 00 00 2A 00 00 00 00 00 00
Fn+F1:      01 00 00 3A 00 00 00 00 00 02
Fn+Up:      01 00 00 52 00 00 00 00 00 02
```

### Windows 用户态 API 不可见的原因

`kbdhid.sys` 只解析 Byte 1~8，丢弃 Byte 9。Raw Input / HID API / HidSharp 都拿不到 Fn 状态。

## 四、转换逻辑

### 核心规则

```
1. physicalFnDown = (report[9] & 0x02) != 0
2. physicalLeftCtrlDown = (report[1] & 0x01) != 0
3. Fn → Ctrl: output[1] |= 0x01
4. 清除 Byte 9: output[9] &= ~0x02
5. Ctrl → FnLayer: output[1] &= ~0x01, internalFnLayer = true
6. Step 2b: 若 Fn 按下，重新设置 output[1] |= 0x01（防止被 Step 5 清掉）
7. FnLayer 重映射 key slots 3~8
```

### FnLayer 映射表

| 原始 | 映射 | 键名 |
|------|------|------|
| 0x2A | 0x4C | Backspace → Delete |
| 0x52 | 0x4B | Up → PageUp |
| 0x51 | 0x4E | Down → PageDown |
| 0x50 | 0x4A | Left → Home |
| 0x4F | 0x4D | Right → End |

### 媒体键映射（MVP-B，通过 COL02）

| FnLayer + | Key Usage | Consumer Usage |
|-----------|-----------|---------------|
| F7 | 0x40 | 0x00B6 (Previous Track) |
| F8 | 0x41 | 0x00CD (Play/Pause) |
| F9 | 0x42 | 0x00B5 (Next Track) |
| F10 | 0x43 | 0x00E2 (Mute) |
| F11 | 0x44 | 0x00EA (Volume Down) |
| F12 | 0x45 | 0x00E9 (Volume Up) |

触发条件：Physical Left Ctrl（FnLayer）+ F7~F12。Physical Fn + F7~F12 不触发。

## 五、代码结构

### C# 用户态

| 文件 | 说明 |
|------|------|
| `src/OpenMagicKeyboard.Shared/A2450ReportTransformer.cs` | 核心转换逻辑 |
| `src/OpenMagicKeyboard.Shared/A2450MediaKeyMapper.cs` | 媒体键映射模型 |
| `tests/OpenMagicKeyboard.TransformTests/A2450ReportTransformerTests.cs` | 20 个转换测试 |
| `tests/OpenMagicKeyboard.TransformTests/A2450MediaKeyMapperTests.cs` | 17 个媒体键测试 |

### C 驱动骨架

| 文件 | 说明 |
|------|------|
| `driver/OpenMagicKeyboardA2450Filter/A2450Report.h` | HID Report 定义 |
| `driver/OpenMagicKeyboardA2450Filter/ReportTransform.h` | 转换接口 |
| `driver/OpenMagicKeyboardA2450Filter/ReportTransform.c` | 转换实现 |
| `driver/OpenMagicKeyboardA2450Filter/Driver.c` | KMDF 入口 |
| `driver/OpenMagicKeyboardA2450Filter/Device.c` | 设备上下文 |
| `driver/OpenMagicKeyboardA2450Filter/Filter.c` | IOCTL 拦截设计 |

### 工具

| 工具 | 说明 |
|------|------|
| `tools/A2450HidLogger/` | Raw Input 键盘日志 |
| `tools/A2450ReportLogger/` | HID Report 读取尝试 |
| `tools/A2450DescriptorDump/` | HID Collection 描述符分析 |
| `tools/A2450ConsumerControlProbe/` | COL02 Consumer Control 验证 |

### 文档

| 文件 | 说明 |
|------|------|
| `docs/a2450-usb-hid-report-structure.md` | HID Report 结构 |
| `docs/a2450-filter-driver-mvp-plan.md` | 驱动 MVP 设计 |
| `docs/a2450-transform-parity-check.md` | C#/C 一致性检查 |
| `docs/driver-build-and-test-safety.md` | 构建安全规范 |
| `docs/roadmap.md` | 项目路线图 |
| `docs/session-export/` | 本目录 |

## 六、测试状态

- TransformTests: 37/37 通过
- DescriptorDump --simulate: 成功
- ConsumerControlProbe --simulate: 成功
- WDK build: 失败（缺少 WDK 构建工具）

## 七、WDK 构建状态

```
MSB8020: WindowsKernelModeDriver10.0 build tools not found.
```

原因：本机有 Windows SDK 10.0.22621.0，但没有安装 WDK。
vcxproj 结构正确，MSBuild 能识别项目。

## 八、安全边界

当前阶段禁止：

- 安装驱动
- 开启 TESTSIGNING
- 关闭 Secure Boot
- 修改注册表
- 运行 pnputil / devcon
- 绑定真实设备
- 向 COL02 写入 Output Report

## 九、路线图

| 阶段 | 内容 | 状态 |
|------|------|------|
| MVP-A | COL01 Fn/Ctrl + 导航键 + WDK 骨架 | Build scaffold ready |
| MVP-B | COL02 媒体键 | 设计完成，待实现 |
| MVP-C | 安装程序 / 签名 / 托盘 | 未开始 |
| MVP-D | 蓝牙支持 | 未开始 |

## 十、Git 提交历史（本次会话）

```
3e34813 Document WDK build toolchain requirement
5175f40 Fix A2450 driver scaffold review issues
92cf7b3 Prepare A2450 filter driver MVP-A build scaffold
df0010f Add A2450 consumer control media key model and probe
76202a2 Update simulated descriptor dump to match real A2450 device
f2f1330 Fix A2450 descriptor dump: HIDP_STATUS_SUCCESS, CreateFile fallback
897694f Merge branch 'main' of https://github.com/hhkb-ai/OpenMagicKeyboardWin
4c46f4d Add A2450 report transformer, unit tests, descriptor dump tool, and KMDF driver skeleton
```

## 十一、下一步建议

1. 安装 WDK + WDK VSIX，编译 .sys
2. 在 VM 中测试驱动加载
3. MVP-B 媒体键实现（COL02 Consumer Control）
4. 蓝牙模式 HID Report 抓包验证
