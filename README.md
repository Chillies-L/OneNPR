# OneNPR：Unity URP 三渲二 NPR Shader 工具包

OneNPR 是一个面向 Unity URP（Universal Render Pipeline，通用渲染管线）的 NPR（Non-Photorealistic Rendering，非真实感渲染）角色渲染工具包，核心目标是用较少美术资源实现稳定的“三渲二”卡通角色效果。仓库内容来自 Unity 项目 `F:\Unity_Project\Graphic\Graphic` 中整理出的可移植插件资源。

## 功能概览

- 角色 Toon Shader（卡通着色器）：包含三段色阶、硬高光（Specular Highlight）、边缘光（Rim Light）、法线外扩描边（Normal Extrusion Outline）等效果。
- 屏幕空间描边（Screen-Space Outline）：包含 `NPR_OutlineFeature.cs` 与 `SobelOutline.shader`，通过深度/法线纹理进行 Sobel 边缘检测。
- NPR 后处理（Post Processing）：包含 `NPRStylePostProcessFeature.cs` 与 `NPRStylePostProcess.shader`，可在 URP Renderer Feature（渲染器特性）中启用。
- 材质与预设（Material / Preset）：提供可直接测试的示例材质、NPR 风格预设和生成材质。
- 编辑器工具（Editor Tool）：包含自定义 ShaderGUI、材质替换窗口、AI 资产生成窗口、法线平滑工具等。
- 运行时辅助（Runtime Helper）：包含运行时 UI 面板、展示旋转脚本、对象组切换脚本等。

## 仓库结构

```text
Assets/
  CustomShaders/   核心 Shader、HLSL、Renderer Feature、Editor 工具、预设和说明文档
  Materials/       示例材质
  Scripts/         展示与场景辅助脚本
  Settings/        URP Asset、URP Renderer Asset、Volume Profile 等配置
```

Unity `.meta` 文件已经保留，用于维持 GUID（Globally Unique Identifier，全局唯一标识符）引用，导入其他 Unity 项目时不要删除。

## 环境要求

- Unity 2022.3 LTS；源项目使用 `2022.3.60f1c1`。
- URP（Universal Render Pipeline，通用渲染管线）14.x；源项目使用 `com.unity.render-pipelines.universal` `14.0.12`。
- 如果使用运行时 UI 或对象组切换脚本，需要启用 UGUI（Unity UI）。

本仓库没有提交 Unity 的 `Library/`、缓存文件、课程文档、模型、截图、字体缓存或 `Packages/com.unity...` 内部包文件。目标项目需要自行安装 URP。

## 推荐使用方式

首选方式是在 GitHub Releases（发布版本）中下载 `.unitypackage`：

- Release：<https://github.com/Chillies-L/OneNPR/releases/tag/v1.0.0>
- 文件：`NPRShaderPlugin_20260705.unitypackage`

导入步骤：

1. 打开目标 Unity 项目。
2. 确认项目已经安装 URP。
3. 选择 `Assets > Import Package > Custom Package...`。
4. 选择下载的 `NPRShaderPlugin_20260705.unitypackage`。
5. 保持默认勾选并点击 `Import`。
6. 在 `Project Settings > Graphics` 或 `Quality` 中指定 URP Asset（URP 资源）。
7. 可以直接使用 `Assets/Settings/URP-HighFidelity.asset`，也可以将 `NPR Style Post Process` 或 `NPR Outline Feature` 添加到现有 URP Renderer Asset（URP 渲染器资源）中。

## 手动复制方式

如果不使用 `.unitypackage`，也可以直接复制仓库中的 `Assets` 内容：

1. 将本仓库的 `Assets/CustomShaders`、`Assets/Materials`、`Assets/Scripts`、`Assets/Settings` 复制到目标项目的 `Assets/` 下。
2. 同时复制对应 `.meta` 文件。
3. 回到 Unity，等待 Asset Database（资源数据库）刷新和脚本编译完成。

## 重新导出 UnityPackage

如果需要从 Unity 中重新导出：

1. 在 Project（项目）窗口中选中 `Assets/CustomShaders`、`Assets/Materials`、`Assets/Scripts`、`Assets/Settings`。
2. 右键选择 `Export Package...`。
3. 不建议勾选 `Include Dependencies`（包含依赖），否则 Unity 会把 URP 内部 `Packages/com.unity...` 文件也打进包内，导致包体变大且不适合作为插件交付。
4. 点击 `Export...` 并保存 `.unitypackage`。

## 版本说明

`v1.0.0` 是首个整理发布版本，包含当前项目中的完整 NPR Shader 工具包、URP 配置、材质示例和运行时/编辑器辅助脚本。
