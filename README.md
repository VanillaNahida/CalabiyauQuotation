# 喵语生成器 / Calabiyau Quotation Generator

## 本文档由AI生成

## 项目介绍 / Project Introduction

喵语生成器是一个用于生成和快捷粘贴随机喵语的小工具，专为喜欢可爱喵语的用户设计。

Calabiyau Quotation Generator is a small tool for generating and quickly pasting random cat-themed phrases, designed for users who love cute cat language.

## 功能特性 / Features

- 🎲 生成随机喵语文本
- ⌨️ 支持全局快捷键粘贴
- 🌐 可从远程URL自动更新词库
- 📋 一键复制功能
- 📱 系统托盘支持
- 🔔 Windows通知功能
- 🖼️ 关于页面图标点击切换效果

- 🎲 Generate random cat language text
- ⌨️ Support global hotkey paste
- 🌐 Auto-update dictionary from remote URL
- 📋 One-click copy function
- 📱 System tray support
- 🔔 Windows notification feature
- 🖼️ Icon switching effect on about page

## 安装方法 / Installation

1. 从GitHub仓库下载最新的发布版本
2. 解压到任意文件夹
3. 运行 `CalabiyauQuotation.exe` 即可

1. Download the latest release from the GitHub repository
2. Extract to any folder
3. Run `CalabiyauQuotation.exe`

## 使用说明 / Usage

### 主界面 / Main Interface
- 点击「换一个喵」按钮生成新的喵语文本
- 点击「一键复制喵」按钮复制当前文本到剪贴板
- 使用设置的全局快捷键快速生成并粘贴喵语

- Click the "换一个喵" button to generate new cat language text
- Click the "一键复制喵" button to copy the current text to clipboard
- Use the set global hotkey to quickly generate and paste cat language

### 设置界面 / Settings Interface
- 设置全局粘贴快捷键
- 启用/禁用「清空输入框并重新粘贴」功能
- 启用/禁用「自动发送（粘贴后按下Enter）」功能
- 设置词库自动更新选项和词库地址

- Set global paste hotkey
- Enable/disable "Clear input box and paste again" feature
- Enable/disable "Auto send (press Enter after paste)" feature
- Set dictionary auto-update options and dictionary URL

### 关于界面 / About Interface
- 查看程序版本和作者信息
- 点击图标切换显示不同的图标
- 点击链接访问作者的GitHub、B站主页和QQ群

- View program version and author information
- Click the icon to switch between different icons
- Click links to visit author's GitHub, Bilibili homepage, and QQ groups

## 项目结构 / Project Structure

```
CalabiyauQuotation/
├── Assets/              # 资源文件
│   ├── AppIcon.ico      # 应用图标
│   ├── NotifationIcon.png  # 通知图标
│   └── XingHuiNotifation.png  # 星绘通知图标
├── Controls/            # 自定义控件
│   └── HotKeyBox.cs     # 热键输入框
├── Models/              # 数据模型
│   ├── DictionaryManager.cs  # 词库管理
│   └── SettingsManager.cs    # 设置管理
├── Services/            # 服务
│   ├── ClipboardService.cs   # 剪贴板服务
│   ├── HotKeyManager.cs      # 热键管理
│   └── ToastNotificationHelper.cs  # 通知助手
├── Themes/              # 主题
│   └── Generic.xaml     # 通用主题
├── App.xaml             # 应用入口
├── App.xaml.cs          # 应用代码
├── Calabiyau_text.yml   # 词库文件
├── MainWindow.xaml      # 主窗口
├── MainWindow.xaml.cs   # 主窗口代码
└── CalabiyauQuotation.csproj  # 项目文件
```

## 技术栈 / Technology Stack

- C#
- WPF (Windows Presentation Foundation)
- Hardcodet.NotifyIcon.Wpf (系统托盘)
- Microsoft.Toolkit.Uwp.Notifications (通知)
- YamlDotNet (YAML解析)

## 贡献指南 / Contribution Guide

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

1. Fork this repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 许可证 / License

本项目采用 GPL 3.0 许可证 - 详情见 LICENSE 文件

This project is licensed under the GPL-3.0 License - see the LICENSE file for details

## 联系方式 / Contact

- 作者：香草味的纳西妲喵 (VanillaNahida)
- GitHub：[VanillaNahida/CalabiyauQuotation](https://github.com/VanillaNahida/CalabiyauQuotation)
- B站：[香草味的纳西妲喵](https://space.bilibili.com/1347891621)
- QQ群：见程序关于页面

- Author: 香草味的纳西妲喵 (VanillaNahida)
- GitHub: [VanillaNahida/CalabiyauQuotation](https://github.com/VanillaNahida/CalabiyauQuotation)
- Bilibili: [香草味的纳西妲喵](https://space.bilibili.com/1347891621)
- QQ Groups: See about page in the program

---

感谢使用喵语生成器！希望它能给你带来快乐～

Thank you for using Calabiyau Quotation Generator! Hope it brings you joy～
