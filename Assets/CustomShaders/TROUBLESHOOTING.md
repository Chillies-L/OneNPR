# 故障排除指南 (Troubleshooting)

## 基础检查

遇到问题时建议先按以下顺序检查：

1. Console（控制台）是否有 C# 或 Shader 编译错误。
2. 项目是否使用 URP（Universal Render Pipeline，通用渲染管线）14.x。
3. 材质是否使用 `Custom/NPR/Toon_Character_ImprovedOutline`。
4. URP Renderer Asset（URP 渲染器资源）中是否添加了需要的 Renderer Feature（渲染器特性）。
5. 修改 Shader 或脚本后是否已重新导入资源或等待 Unity 编译完成。

## Shader 未找到

症状：

```text
Shader 'Custom/NPR/Toon_Character_ImprovedOutline' not found
```

处理：

1. 确认 Shader 文件位于 `Assets/CustomShaders/Toon_Character_ImprovedOutline.shader`。
2. 在 Unity 中右键 Shader 文件并选择 `Reimport`。
3. 查看 Console 是否还有编译错误。

## 材质显示品红色

品红色通常表示 Shader 编译失败或当前渲染管线不兼容。

处理：

1. 检查 Console 中的 Shader 错误。
2. 确认项目正在使用 URP，而不是 Built-in Render Pipeline（内置渲染管线）。
3. 确认 URP 版本接近 14.x。
4. 重新导入 `Assets/CustomShaders`。

## Render Feature 不显示

如果 Add Renderer Feature 列表中没有 NPR 相关选项：

1. 确认 C# 脚本没有编译错误。
2. 确认 `NPRStylePostProcessFeature.cs` 和 `NPR_OutlineFeature.cs` 不在 `Editor` 文件夹内。
3. 重启 Unity Editor（Unity 编辑器）。

## 描边不显示

法线外扩描边：

- 增大 `Outline Width`。
- 确认 `Outline Color` 的 Alpha 为 1。
- 在简单 Sphere（球体）上测试，排除模型法线问题。

屏幕空间描边：

- 确认 `NPR Style Post Process` 已添加到 URP Renderer Asset。
- 确认 `Outline Enabled` 已开启。
- 至少开启 `Use Depth` 或 `Use Normals`。
- 降低 `Depth Threshold` 或 `Normal Threshold`。

## Debug 视图异常

Depth（深度）全黑或全白：

- 调整相机 Near/Far Plane（近裁剪面 / 远裁剪面）。
- 调整调试视图深度缩放。

Normals（法线）几乎不变化：

- 确认目标 Shader 写入 DepthNormals Pass（深度法线通道）。
- 更换模型或旋转相机验证法线视图。

Sobel Edge（Sobel 边缘）过多或过少：

- 过多：提高阈值。
- 过少：降低阈值，并确认深度/法线输入可用。

## 后处理没有效果

1. 确认 `NPRStylePostProcessFeature.settings.enabled` 为 true。
2. 至少开启 Outline、Color Grading 或 Pattern Shading 中的一项。
3. 确认 `Hidden/NPR/StylePostProcess` Shader 可以被找到。
4. 如果使用资源化风格预设，确认 `Active Preset` 指向 `BoldInkComic.asset` 或 `GraphicActionAnime.asset`。

## 性能问题

- 开发时可以打开 Debug Visualization（调试可视化），发布或展示时建议关闭。
- 屏幕空间描边和图案化后处理会增加一次或多次 Blit（位块传输）成本。
- 运行时 UI 面板会生成 UGUI（Unity UI）控件，展示时可折叠或关闭。

## 兼容性

推荐环境：

- Unity 2022.3 LTS
- URP 14.x

Unity 6 / URP 17.x 的 Render Graph（渲染图）路径可能需要额外适配。若迁移到新版 URP，优先检查 `ScriptableRendererFeature`、`RTHandle` 和 Blitter API（应用程序接口）的变化。
