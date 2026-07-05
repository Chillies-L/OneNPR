# OneNPR Shader 使用说明

`Assets/CustomShaders` 是 OneNPR 的核心目录，包含角色 Shader（着色器）、HLSL（High-Level Shading Language，高级着色语言）包含文件、URP Renderer Feature（渲染器特性）、编辑器工具、运行时调参面板、预设和技术说明。

## 推荐入口

推荐优先使用：

- Shader：`Custom/NPR/Toon_Character_ImprovedOutline`
- URP Renderer Feature（URP 渲染器特性）：`NPRStylePostProcessFeature`
- 材质 Inspector（材质检查器）：`ToonShaderGUI`
- 资源化风格预设（Style Preset Assets）：`BoldInkComic.asset`、`GraphicActionAnime.asset`

`Custom/NPR/Toon_Character` 和 `NPR_OutlineFeature + SobelOutline.shader` 仍保留在包内，主要用于兼容早期材质或单独调试屏幕空间 Sobel 描边。

## 已实现效果

### 角色材质

- 三段色阶卡通光照：Base、1st Shade、2nd Shade。
- 自动阴影配色：根据基础色生成带色相偏移的阴影色，减少白模灰阶感。
- 虚拟主光：稳定阴影方向，避免场景光破坏二次元块面阴影。
- 硬高光（Specular Highlight）：基于 Blinn-Phong（布林-冯）思路做硬边卡通化。
- 边缘光（Rim Light）：用于强化轮廓和热血动画风格。
- Shade Map（阴影色贴图）、Control Map（控制图）和 Ramp Texture（阶调贴图）：支持按贴图控制局部阴影、羽化、高光、边缘光和色阶过渡。
- ShadowCaster Pass（阴影投射通道）与 DepthNormals Pass（深度法线通道）：用于阴影、深度/法线采样和后处理描边。

### 描边

- 法线外扩描边（Normal Extrusion Outline）：适合角色外轮廓。
- 顶点色平滑法线：通过 `NPR_NormalsSmoother` 将平滑法线写入顶点色，减少 UV 接缝或硬边处的描边断裂。
- 反向 Pass（Reverse Pass）：改善部分视角下的线条连续性。
- 世界空间壳层（World Shell）、斜接控制（Miter Control）和方向偏置（Directional Bias）：用于控制不同模型上的描边厚度和拐角表现。
- 屏幕空间 Sobel 描边：基于深度、法线和可选颜色阈值检测外轮廓与结构边。

### 后处理

`NPRStylePostProcessFeature` 当前包含：

- Screen Space Outline（屏幕空间描边）
- Posterize（色阶压缩）
- Contrast / Saturation（对比度 / 饱和度）
- Shadow Crush（暗部压缩）
- Pattern Shading（图案化明暗）：`Dots`（网点）和 `Hatching`（排线）

### 工具

- `ToonShaderGUI`：自定义材质面板，提供分组参数、资源化风格预设和兼容材质预设。
- `NPRMaterialReplacerWindow`：将模型上的普通材质批量替换为 NPR 材质，并保留原材质贴图、颜色和 UV 缩放。
- `NPRMaterialReplacementProfile`：资源化材质替换配置，用于统一管理替换时的预设、输出目录、贴图保留和法线处理选项。
- `NPRRuntimeUIPanel`：运行时选择对象、应用预设、实时调节材质与后处理参数，并显示 FPS（Frames Per Second，帧率）。
- `NPR_DebugVisualization` / `DebugVisualization.shader`：用于查看深度、法线、边缘检测等调试视图。

## 预设

### 材质预设（Material Presets）

`ToonMaterialPreset.cs` 提供 7 个材质预设：

- `Soft Anime Cel（柔和赛璐璐）`：柔和阴影、较轻描边，适合通用动漫角色。
- `Hard Ink Cel（硬边墨线）`：阴影边界更硬，线条更明确。
- `High Contrast Mono（黑白高对比）`：黑白漫画方向，高对比阴影与强描边。
- `Warm Comic Cel（暖色漫画）`：暖色主调，适合印刷漫画或暖光角色。
- `Procedural Background Cel（程序化背景）`：弱描边、柔和阴影，适合简单背景或非主体模型。
- `Bold Ink Comic（粗线彩色漫画）`：粗线、强色彩、网点后处理倾向。
- `Graphic Action Anime（热血图形动画）`：高对比暗部、强边缘光、偏动作动画效果。

### 资源化风格预设（Style Preset Assets）

`Assets/CustomShaders/Presets` 中的 `BoldInkComic.asset` 和 `GraphicActionAnime.asset` 会同时控制材质参数与 `NPRStylePostProcessFeature` 参数。材质 Inspector 中的“资源化预设（Style Preset Asset）”按钮会把两部分同步更新到已安装的 URP Renderer Asset（URP 渲染器资源）。

`MAN.asset` 和 `Suntone.asset` 是材质替换配置（Material Replacement Profile），分别用于批量套用偏硬边墨线和暖色漫画方向的材质替换流程。

## 快速开始

### 创建单个 NPR 材质

1. 在 Project（项目）窗口中右键创建 Material（材质）。
2. 将 Shader 改为 `Custom/NPR/Toon_Character_ImprovedOutline`。
3. 在材质 Inspector 中选择一个材质预设或资源化风格预设。
4. 根据模型调整 `Base Color`、`Base Step`、`Shade Step`、`Outline Width`、`Rim Intensity` 等参数。

### 启用后处理

1. 选中项目中的 URP Renderer Asset（例如 `Assets/Settings/URP-HighFidelity-Renderer.asset`）。
2. 添加或启用 `NPR Style Post Process`。
3. 在 `Active Preset` 中选择 `BoldInkComic.asset` 或 `GraphicActionAnime.asset`，或手动调整描边、色阶压缩、网点/排线参数。

### 批量替换模型材质

1. 打开 `Tools > NPR > Material Replacer`。
2. 选择模型根节点。
3. 创建或选择 `NPRMaterialReplacementProfile`。
4. 点击 `Apply NPR To Root`。
5. 替换结果会输出到 `Assets/CustomShaders/GeneratedMaterials`。

### 运行时调参

1. 在场景对象上挂载 `NPRRuntimeUIPanel`。
2. 指定目标 Renderer（渲染器）、Style Preset（风格预设）和 `NPRStylePostProcessFeature`。
3. 进入 Play Mode（播放模式）后，可选择对象并实时调节阴影、描边、高光、边缘光和后处理参数。

## 算法原理说明

### 三段色阶光照量化

传统 Lambert（朗伯）光照会产生连续明暗渐变。卡通渲染通常需要更明确的块面阴影，因此 OneNPR 使用 Half-Lambert（半朗伯）把 `NdotL` 映射到 0-1 区间，再通过 `BaseStep`、`ShadeStep` 和 `smoothstep` 将连续光照切分成亮部、一级阴影和二级阴影。

```hlsl
float halfLambert = NdotL * 0.5 + 0.5;
float toShade1 = 1.0 - smoothstep(BaseStep - Feather, BaseStep + Feather, halfLambert);
```

`Step` 控制阴影面积，`Feather` 控制边界硬度。较小的 `Feather` 会得到更硬的漫画阴影边界，较大的 `Feather` 会得到更柔和的动画阴影过渡。

### 法线外扩描边

法线外扩描边通过额外 Pass（渲染通道）绘制一个略微放大的背面壳层。顶点沿法线方向外扩，再使用 `Cull Front`（剔除正面）只显示轮廓外缘。

```hlsl
float3 positionWS = TransformObjectToWorld(positionOS);
float3 normalWS = TransformObjectToWorldNormal(normalOS);
positionWS += normalize(normalWS) * outlineWidth;
output.positionHCS = TransformWorldToHClip(positionWS);
```

如果模型在 UV 接缝或硬边处分裂顶点，原始法线方向可能不连续。`NPR_NormalsSmoother` 会把平滑法线编码到顶点色，Shader 再读取顶点色法线用于描边，从而改善断线问题。

### 屏幕空间描边

屏幕空间描边通过 Sobel（索贝尔）边缘检测读取深度、法线和可选颜色差异。深度用于识别物体外轮廓，法线用于识别表面折线，颜色阈值可辅助识别风格化块面边界。

### 图案化后处理

`NPRStylePostProcess.shader` 在同一个后处理 Pass 中处理描边、色阶压缩、对比度、饱和度、暗部压缩和图案化明暗。`Dots` 用于漫画网点效果，`Hatching` 用于排线效果；两者都可通过强度、缩放、角度、颜色和亮度阈值控制。

## 常见问题

### 描边断裂

优先检查模型法线和顶点分裂。对硬边、UV 接缝明显的模型，可添加 `NPR_NormalsSmoother`，或在建模软件中重新处理法线。

### 后处理没有效果

确认 URP Renderer Asset 中已经添加 `NPR Style Post Process`，并且 `enabled`、`outlineEnabled`、`colorGradingEnabled` 或 `halftoneEnabled` 中至少有一个效果处于启用状态。

### 材质显示为品红色

这通常表示 Shader 编译失败。请检查 Console（控制台）错误、URP 版本和 Shader Include（包含文件）路径。

### 预设没有同步后处理

兼容材质预设只会改材质参数。需要同步后处理时，请使用资源化风格预设 `BoldInkComic.asset` 或 `GraphicActionAnime.asset`。

## 依赖

- Unity 2022.3 LTS
- URP（Universal Render Pipeline，通用渲染管线）14.x
- UGUI（Unity UI）：仅运行时调参面板需要
