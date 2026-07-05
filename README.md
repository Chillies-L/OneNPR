# OneNPR：Unity URP 三渲二 NPR Shader 工具包

OneNPR 是一套面向 Unity URP（Universal Render Pipeline，通用渲染管线）的 NPR（Non-Photorealistic Rendering，非真实感渲染）角色渲染工具包。它以“三渲二”卡通角色材质为核心，同时提供屏幕空间描边、图案化后处理、材质批量替换、运行时调参面板和可复用预设，适合在低美术资源条件下快速搭建动漫/漫画风格角色展示。

## 当前实现

- 角色 Toon Shader（卡通着色器）：三段色阶、自动阴影配色、虚拟主光、硬高光（Specular Highlight）、边缘光（Rim Light）、Ramp Texture（阶调贴图）、Shade Map（阴影色贴图）和 Control Map（控制图）。
- 描边系统（Outline）：支持法线外扩描边、顶点色平滑法线、反向 Pass（Reverse Pass）、世界空间壳层（World Shell）、斜接控制（Miter Control）和屏幕空间 Sobel 描边。
- 风格化后处理（Stylized Post Processing）：提供屏幕空间描边、色阶压缩、对比度/饱和度调整、暗部压缩，以及 Dots（网点）和 Hatching（排线）两种图案化效果。
- 预设系统（Preset System）：包含 7 个材质预设和 2 个资源化风格预设，可同时同步材质参数和后处理参数。
- 编辑器工具（Editor Tools）：自定义 ShaderGUI、材质批量替换窗口、材质替换配置、法线平滑工具。
- 运行时工具（Runtime Tools）：运行时 UI 调参面板、对象选择、FPS（Frames Per Second，帧率）显示、运行时材质创建、展示旋转和对象组切换脚本。

## 仓库结构

```text
Assets/
  CustomShaders/   Shader、HLSL、Renderer Feature、编辑器工具、运行时工具、预设和说明文档
  Materials/       示例材质
  Scripts/         展示与场景辅助脚本
  Settings/        URP Asset、URP Renderer Asset、Volume Profile 等配置
```

Unity `.meta` 文件已经保留，用于维持 GUID（Globally Unique Identifier，全局唯一标识符）引用。复制或导入资源时请同时保留 `.meta` 文件。

## 环境要求

- Unity 2022.3 LTS；源项目使用 `2022.3.60f1c1`。
- URP（Universal Render Pipeline，通用渲染管线）14.x；源项目使用 `com.unity.render-pipelines.universal` `14.0.12`。
- 使用运行时 UI 或对象组切换脚本时，需要启用 UGUI（Unity UI）。

本仓库不包含 Unity `Library/`、缓存文件、模型、截图、字体缓存或 `Packages/com.unity...` 内部包文件。目标项目需要自行安装 URP。

## 推荐使用方式

首选方式是在 GitHub Releases（发布版本）中下载 `.unitypackage`：

- Release：<https://github.com/Chillies-L/OneNPR/releases/tag/v1.0.1>
- 文件：`OneNPR_v1.0.1.unitypackage`

导入步骤：

1. 打开目标 Unity 项目。
2. 确认项目已经安装 URP。
3. 选择 `Assets > Import Package > Custom Package...`。
4. 选择下载的 `.unitypackage` 文件。
5. 保持默认勾选并点击 `Import`。
6. 在 `Project Settings > Graphics` 或 `Quality` 中指定 URP Asset（URP 资源）。
7. 可以直接使用 `Assets/Settings/URP-HighFidelity.asset`，也可以将 `NPR Style Post Process` 或 `NPR Outline Feature` 添加到现有 URP Renderer Asset（URP 渲染器资源）中。

## 手动复制方式

如果不使用 `.unitypackage`，也可以直接复制仓库中的 `Assets` 内容：

1. 将 `Assets/CustomShaders`、`Assets/Materials`、`Assets/Scripts`、`Assets/Settings` 复制到目标项目的 `Assets/` 下。
2. 同时复制对应 `.meta` 文件。
3. 回到 Unity，等待 Asset Database（资源数据库）刷新和脚本编译完成。

## 预设说明

材质预设（Material Presets）位于 `ToonMaterialPreset.cs`：

- `Soft Anime Cel（柔和赛璐璐）`
- `Hard Ink Cel（硬边墨线）`
- `High Contrast Mono（黑白高对比）`
- `Warm Comic Cel（暖色漫画）`
- `Procedural Background Cel（程序化背景）`
- `Bold Ink Comic（粗线彩色漫画）`
- `Graphic Action Anime（热血图形动画）`

资源化风格预设（Style Preset Assets）位于 `Assets/CustomShaders/Presets`：

- `BoldInkComic.asset`
- `GraphicActionAnime.asset`

资源化风格预设会同时写入材质参数，并同步更新已安装的 `NPRStylePostProcessFeature` 后处理参数；兼容材质预设只写入材质参数。

## 重新导出 UnityPackage

如果需要从 Unity 中重新导出：

1. 在 Project（项目）窗口中选中 `Assets/CustomShaders`、`Assets/Materials`、`Assets/Scripts`、`Assets/Settings`。
2. 右键选择 `Export Package...`。
3. 不建议勾选 `Include Dependencies`（包含依赖），否则 Unity 会把 URP 内部 `Packages/com.unity...` 文件也打进包内，导致包体变大且不适合作为插件交付。
4. 点击 `Export...` 并保存 `.unitypackage`。
