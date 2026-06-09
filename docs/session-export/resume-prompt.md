# OpenMagicKeyboardWin — 会话恢复提示词

将以下内容作为新会话的开头，可快速恢复项目上下文。

---

```
你现在继续开发这个 Windows 开源项目：

项目路径：D:\miaokongjianp\OpenMagicKeyboardWin
GitHub：https://github.com/hhkb-ai/OpenMagicKeyboardWin

项目目标：让 Apple Magic Keyboard A2450 在 Windows 上支持 Fn/Ctrl 交换和 FnLayer 映射。

当前状态（2026-06-09）：

已确认 HID Report 结构（USBPcap 抓包）：
- COL01 = 标准键盘，10 字节，Byte 9 的 0x02 = Fn，Byte 1 的 0x01 = Left Ctrl
- COL02 = Consumer Control，UsagePage 0x000C，2 字节输入
- COL03 = Vendor Defined，UsagePage 0xFF00，65 字节

已实现：
- A2450ReportTransformer.cs（C# 转换逻辑）
- A2450MediaKeyMapper.cs（媒体键映射模型）
- TransformTests 37/37 通过
- A2450DescriptorDump / A2450ConsumerControlProbe 只读工具
- KMDF 驱动骨架（vcxproj + .c/.h 文件）
- INF 模板（.inf.template，不可安装）

WDK build 状态：MSB8020 缺少 WDK 构建工具，vcxproj 结构正确

转换逻辑核心：
- physicalFnDown = (report[9] & 0x02) != 0
- physicalLeftCtrlDown = (report[1] & 0x01) != 0
- Fn → Ctrl: output[1] |= 0x01
- Ctrl → FnLayer: output[1] &= ~0x01
- Step 2b: 若 Fn 按下，重新设置 output[1] |= 0x01
- FnLayer + Backspace(0x2A) → Delete(0x4C)
- FnLayer + Up(0x52) → PageUp(0x4B)
- FnLayer + Down(0x51) → PageDown(0x4E)
- FnLayer + Left(0x50) → Home(0x4A)
- FnLayer + Right(0x4F) → End(0x4D)

安全边界：不要安装驱动，不要开启 TESTSIGNING，不要修改注册表，不要绑定真实设备。

请先阅读 docs/session-export/session-context-2026-06-09.md 获取完整上下文。
```
