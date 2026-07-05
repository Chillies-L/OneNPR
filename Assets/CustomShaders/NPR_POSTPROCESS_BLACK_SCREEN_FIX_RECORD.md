# NPR 后处理黑屏修复记录（2026-07-05）

## 问题

运行时打开 NPR 后处理（Post Processing）附加功能后，Game View 画面会接近全黑。该附加功能由 URP Render Feature（渲染特性）`NPRStylePostProcessFeature` 驱动，包含屏幕空间描边（Screen-Space Outline）、半调网点（Halftone）和整体风格化调色（Stylized Color Grading）。

修复前的关键证据：

- `ShaderUtil.GetShaderMessages` 报告 `Assets/CustomShaders/NPRStylePostProcess.shader:55` 存在 `redefinition of 'Luminance'`。
- 修复前的运行截图统计显示，即使接近直通配置，画面仍有约 84% 近黑像素，说明问题不只是参数调暗，而是 Shader（着色器）编译失败导致后处理输出异常。

## 解决思路

先备份，再用回归测试锁定根因，最后做最小修改。

备份路径：

`F:/Unity_Project/Graphic/Graphic/_codex_backups/NPRPostProcessBlackScreenFix_20260705_072701`

备份文件：

- `Assets/CustomShaders/NPRStylePostProcess.shader`
- `Assets/CustomShaders/NPRStylePostProcess.shader.meta`
- `Assets/CustomShaders/Editor/ToonMaterialPresetTests.cs`

新增回归测试：

- `ToonMaterialPresetTests.StylePostProcessShaderCompilesWithoutErrors`
- 测试通过 `UnityEditor.ShaderUtil` 检查 `Hidden/NPR/StylePostProcess` 的 Shader 编译消息。
- 修复前该测试失败，失败信息为 `redefinition of 'Luminance' (Assets/CustomShaders/NPRStylePostProcess.shader:55)`。

根因判断：

`NPRStylePostProcess.shader` 自定义了一个全局 `Luminance(float3 color)` 函数；但 URP/Core include 链路中也提供了同名 `Luminance` 函数，造成 HLSL（High-Level Shader Language，高级着色器语言）符号重定义。运行时开启后处理后，该 Shader 无法正常编译/执行，最终表现为画面全黑或接近全黑。

修复方式：

- 将本地函数 `Luminance` 改名为 `NPRStyleLuminance`。
- 将该 Shader 内部所有调用同步替换为 `NPRStyleLuminance`。
- 不改动后处理开关结构、预设参数、Render Feature 安装方式或运行时 UI（User Interface，用户界面）逻辑。

## 结果

验证结果：

- 新增单项回归测试修复前失败，修复后通过：`1 passed, 0 failed`。
- `ToonMaterialPresetTests` 全部通过：`12 passed, 0 failed`。
- `dotnet build ./Graphic.sln --no-restore` 通过：`0 errors`，保留既有 `System.Net.Http` / `System.IO.Compression` 版本冲突警告。
- `ShaderUtil.GetShaderMessages` 对 `Hidden/NPR/StylePostProcess` 检查结果：`shader errors=0`。
- Unity Console（控制台）错误：`0`。

运行态截图验证：

- 基线关闭截图：`Assets/Screenshots/NPRPostProcess_AfterFix_BaselineDisabled_20260705_0739.png`
  - 平均亮度：`106.697`
  - 纯黑像素比例：`0`
  - 近黑像素比例（亮度 < 5）：`0.000474`
- 后处理开启截图：`Assets/Screenshots/NPRPostProcess_AfterFix_Enabled_20260705_0739.png`
  - 平均亮度：`58.965`
  - 纯黑像素比例：`0`
  - 近黑像素比例（亮度 < 5）：`0.013836`

结论：

本次黑屏问题已修复。开启 `NPRStylePostProcessFeature` 后画面不再全黑，屏幕空间描边、半调网点和风格化调色能正常参与输出。

剩余注意项：

完整 EditMode 测试仍有 3 个与本次修改无关的既有失败，集中在 `NPRAIAssetGenerationTests`：

- `PromptBuilderCreatesAssetSpecificPromptsForAllToonTextureKinds`：`AmbiguousMatchException`
- `RecipeApplierOnlyUpdatesTargetNprMaterials`：颜色对象比较失败，但输出值文本相同
- `RecipeBuilderCreatesCategorySpecificSettingsWithoutChangingBaseIdentity`：颜色对象比较失败，但输出值文本相同

这些失败不涉及 `NPRStylePostProcess.shader` 或本次后处理黑屏修复。
