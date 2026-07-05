# Cel Shading Presets

本文件记录 OneNPR 当前可用的材质预设（Material Presets）和资源化风格预设（Style Preset Assets）。这些预设用于快速建立稳定的卡通角色效果，使用后仍建议按模型材质、光照和相机距离进行微调。

## 材质预设

材质预设由 `ToonMaterialPreset.cs` 提供，只写入材质参数，不会同步后处理参数。

- `Soft Anime Cel（柔和赛璐璐）`：柔和阴影、较轻描边，适合通用动漫角色。
- `Hard Ink Cel（硬边墨线）`：硬边阴影和更强线条，适合需要明确轮廓的模型。
- `High Contrast Mono（黑白高对比）`：黑白漫画方向，适合高对比展示。
- `Warm Comic Cel（暖色漫画）`：暖色阴影和深色描边，适合暖光或纸面漫画方向。
- `Procedural Background Cel（程序化背景）`：弱描边、柔和阴影，适合背景物体或非主体模型。
- `Bold Ink Comic（粗线彩色漫画）`：粗线、强色彩、网点后处理倾向。
- `Graphic Action Anime（热血图形动画）`：高对比暗部、强边缘光、动作动画方向。

## 资源化风格预设

资源化风格预设位于 `Assets/CustomShaders/Presets`。它们会同时写入材质参数，并同步更新已安装的 `NPRStylePostProcessFeature` 后处理参数。

- `BoldInkComic.asset`：粗线彩色漫画方向，默认启用网点图案化效果。
- `GraphicActionAnime.asset`：热血图形动画方向，强调高对比、强边缘光和更直接的暗部压缩。

## 材质替换配置

`MAN.asset` 和 `Suntone.asset` 是 `NPRMaterialReplacementProfile`（材质替换配置）资源，供 `NPRMaterialReplacerWindow` 批量替换模型材质使用。

- `MAN.asset`：默认使用 `Hard Ink Cel（硬边墨线）`。
- `Suntone.asset`：默认使用 `Warm Comic Cel（暖色漫画）`。

## 使用建议

1. 单个材质调试时，优先在材质 Inspector（检查器）中使用预设下拉菜单。
2. 希望材质和后处理一起切换时，使用 `BoldInkComic.asset` 或 `GraphicActionAnime.asset`。
3. 批量转换模型材质时，使用 `Tools > NPR > Material Replacer`，并选择对应的 `NPRMaterialReplacementProfile`。
4. 预设不会替代模型本身的轮廓、UV、法线质量或贴图细节；对硬边模型建议配合 `NPR_NormalsSmoother`。
