# NPR 后处理重构记录（2026-07-05）

## 参考来源

- 参考项目：Sanqi-normal/CelLookPostProcess
- GitHub：`https://github.com/Sanqi-normal/CelLookPostProcess`
- 本地参考副本：`work/refs/CelLookPostProcess`
- 参考提交：`e0e61af7a74e1c171bce50fe8f1002b4a7331d4a`
- 授权状态：参考仓库当前副本未发现 `LICENSE` 文件；本次没有整包复制，也没有直接搬运完整实现，只参考其后处理结构、参数边界和 Pattern Shading（图案化阴影）思路后在本项目中重写。

## 问题

上一版 `NPRStylePostProcess.shader` 的半调网点（Halftone）实现过于简单：它只在暗部区域用圆点遮罩对颜色做乘法压暗，表现更像固定噪点/暗化层，而不是漫画或波普风格里的图案化阴影。用户反馈“网点和半调不是这样的”，并要求参考 `CelLookPostProcess` 进行后处理重构。

旧实现的主要限制：

- 只有单一圆点模式，没有排线（Hatching）或类型切换。
- 参数只有强度和尺度，缺少图案角度、缝隙颜色、暗部亮度阈值等关键控制。
- 网点逻辑是 `color * mask` 式压暗，没有“网点内保留原色、网点缝隙使用 Pattern Color”的图案化阴影模型。
- UI（User Interface，用户界面）里的 `Halftone Scale` 仍按旧像素格尺寸设计，不适合参考项目那种屏幕空间图案密度控制。

## 解决思路

保留本项目已有的独立 Render Feature（渲染特性）结构，不整包引入 `CelLookPostProcess`。原因是参考项目采用 Volume Component（体积组件）和多 Pass（多通道）架构，本项目现阶段已经有可开关的附加功能和运行时 UI，直接搬入会引入重复控制面。

本次采用“局部重构”：

- 保留 `NPRStylePostProcessFeature` 作为独立附加功能，继续由 `settings.enabled` 与 Renderer Feature active 状态控制。
- 将旧 `HalftoneMask` 替换为 `ApplyPatternShading`。
- 新增 Pattern Shading 参数：
  - `patternType`：`Off / Dots / Hatching`
  - `patternAngle`：图案旋转角度
  - `patternIntensity`：图案半径/线宽响应强度
  - `patternColor`：网点或排线缝隙颜色
  - `patternLumaThreshold`：只在指定暗部亮度以下生成图案
- 保留 `halftoneEnabled / halftoneStrength / halftoneScale` 作为兼容入口：
  - `halftoneEnabled`：图案化阴影总开关
  - `halftoneStrength`：图案混合强度
  - `halftoneScale`：图案密度，范围改为 `1-50`
- 同步更新运行时 UI：`Halftone` 改为 `Pattern Blend`，`Halftone Scale` 改为 `Pattern Scale`。
- 同步更新 `BoldInkComic` 与 `GraphicActionAnime` 预设资源，避免旧的 `halftoneScale: 68/84` 继续污染新图案密度。

## 实现内容

核心文件：

- `Assets/CustomShaders/NPRStylePostProcess.shader`
  - 删除旧 `HalftoneMask`。
  - 新增 `ApplyPatternShading`。
  - 支持 Dots（圆点）与 Hatching（排线）。
  - 采用暗部阈值 `patternRegion` 限制图案区域。
  - 采用 `Pattern Color` 作为缝隙颜色，图案内部保留原始风格化颜色。

- `Assets/CustomShaders/NPRStylePostProcessFeature.cs`
  - 新增 `PatternShadingType` 枚举。
  - 新增 Pattern Shading 参数并写入材质。
  - `HasAnyActiveEffect()` 改为通过 `HasActivePattern()` 判断图案通道是否真正启用。

- `Assets/CustomShaders/NPRStylePreset.cs`
  - 预设数据新增 `patternType / patternAngle / patternIntensity / patternColor / patternLumaThreshold`。
  - `BoldInkComic` 默认启用 Dots。
  - `GraphicActionAnime` 保留图案通道关闭，但预置 Hatching 参数，便于后续打开。

- `Assets/CustomShaders/NPRRuntimeUIPanel.cs`
  - `SetHalftoneStrength` 在强度大于 0 时自动启用图案通道。
  - `SetHalftoneScale` 新范围为 `1-50`。
  - UI 文案改为 `Pattern Blend` 与 `Pattern Scale`。

测试与资源：

- `Assets/CustomShaders/Editor/ToonMaterialPresetTests.cs`
  - 新增 Pattern Shading 设置字段测试。
  - 新增 shader 输入和旧 `HalftoneMask` 移除测试。
  - 更新预设断言。

- `Assets/CustomShaders/Editor/NPRRuntimeUIPanelTests.cs`
  - 新增运行时 UI 对 Pattern 参数的夹取和自动启用测试。

- `Assets/CustomShaders/Presets/BoldInkComic.asset`
- `Assets/CustomShaders/Presets/GraphicActionAnime.asset`
- `Assets/Settings/URP-HighFidelity-Renderer.asset`
  - 同步新序列化字段和默认值。

## 结果

备份路径：

`F:/Unity_Project/Graphic/Graphic/_codex_backups/NPRPostProcessRefactor_20260705_082828`

测试结果：

- RED 阶段：
  - `StylePostProcessSettingsExposeCelLookPatternControls` 失败，原因是缺少 `patternType` 等字段。
  - `StylePostProcessShaderUsesReferenceStylePatternShadingInputs` 失败，原因是缺少 `_PatternType` 等 shader 参数，且仍包含旧 `HalftoneMask`。
- GREEN 阶段：
  - `ToonMaterialPresetTests + NPRRuntimeUIPanelTests`：`20 passed, 0 failed`
  - `ShaderUtil.GetShaderMessages("Hidden/NPR/StylePostProcess")`：`shader errors=0`
  - Unity Console（控制台）：`0 errors`
  - `dotnet build ./Graphic.sln --no-restore`：`0 errors`，保留既有引用版本和旧 URP API warning。

运行态截图：

- Dots（圆点）：`Assets/Screenshots/NPRPostProcess_Refactor_Dots_20260705_0839.png`
  - 平均亮度：`87.258`
  - 纯黑像素比例：`0`
  - 近黑像素比例（亮度 < 5）：`0`
- Hatching（排线）：`Assets/Screenshots/NPRPostProcess_Refactor_Hatching_20260705_0839.png`
  - 平均亮度：`49.745`
  - 纯黑像素比例：`0`
  - 近黑像素比例（亮度 < 5）：`0.271328`

当前状态：

- Play Mode（运行模式）已退出。
- `NPR Style Post Process` Render Feature 当前保持关闭。
- 运行时开启后处理不再全黑，Dots 与 Hatching 两种图案化效果均可输出。

剩余注意项：

完整 EditMode 测试仍有 3 个既有失败，仍集中在 `NPRAIAssetGenerationTests`，与本次后处理重构无关。
