# HDRtoggle-on-Windows

![Windows 11](https://img.shields.io/badge/Windows-11%2024H2-blue?logo=windows)
![C#](https://img.shields.io/badge/Language-C%23-green)
![License](https://img.shields.io/badge/License-MIT-lightgrey)

一个极其轻量化的 Windows HDR 一键切换工具。无需 Xbox Game Bar，无需后台驻留，直接调用 Win32 底层 API 实现。

## 💡 为什么选择本项目？

在 Windows 11 中，即使关闭了 HDR，系统为了 **Auto Color Management (ACM)** 往往会让底层管道保持激活状态。本项目通过逆向分析内存中的位掩码（Bitmask），解决了传统脚本“只能关、不能开”的 Bug，实现了真正的双向切换。

---

## 🚀 快速开始

### 1. 编译 (Compilation)
本项目利用 Windows 系统内置的 C# 编译器，无需安装任何开发环境（如 Visual Studio）。

打开 **命令提示符 (CMD)**，执行以下命令：
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:D:\ToggleHDR.exe D:\ToggleHDR.cs
```
*编译完成后，你将在 D 盘根目录获得一个仅约 4KB 的 `ToggleHDR.exe`。*

### 2. 部署 (Deployment)
1. 将生成的 `ToggleHDR.exe` 放置在固定文件夹。
2. 右键点击它 -> **发送到桌面快捷方式**。
3. 在快捷方式属性中，设置一个全局快捷键（推荐 `Ctrl + Alt + H`）。

---

## ⚠️ 重要说明

> [!WARNING]
> **驱动程序行为：** 本工具直接向显卡驱动发送状态变更指令。切换时屏幕会闪烁（黑屏约 1s），属于显卡重新协商带宽的正常现象。

> [!CAUTION]
> **硬件限制：** 请确保你的显示器和显卡物理支持 HDR10。在不支持的硬件上强制写入寄存器可能导致驱动程序重置或显示异常。

> [!IMPORTANT]
> **系统环境：** > - 建议在 **x64 架构** 下编译运行。
> - 在 Windows 11 24H2 环境下经过 SANC G52 MAX 显示器实测通过。
> - 如果你的环境无法使用，请检查是否开启了某些第三方颜色管理软件。

---

## 🛠 技术原理 (For Developers)

本项目通过 `User32.dll` 导出的以下 API 进行操作：
- `GetDisplayConfigBufferSizes`
- `QueryDisplayConfig`
- `DisplayConfigGetDeviceInfo` (关键：分析 `Raw Value` 判断 ACM 状态)
- `DisplayConfigSetDeviceInfo`
