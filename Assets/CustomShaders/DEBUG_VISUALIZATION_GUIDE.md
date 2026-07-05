# Debug Visualization Guide

`NPR_DebugVisualization` 和 `DebugVisualization.shader` 用于查看渲染管线中的中间结果，帮助调试屏幕空间描边、深度/法线采样和边缘检测阈值。

## 可视化内容

- Depth（深度）：查看场景深度分布，判断深度阈值是否合理。
- Normals（法线）：查看法线纹理是否存在、方向是否连续。
- Sobel Edge（Sobel 边缘）：查看深度/法线边缘检测结果。
- Base Color（基础色）：查看后处理前的颜色输入。

## 使用方式

1. 在 URP Renderer Asset（URP 渲染器资源）中添加或启用 `NPR_DebugVisualization`。
2. 选择需要显示的调试窗口，例如 Depth、Normals 或 Sobel。
3. 进入 Play Mode（播放模式）查看结果。
4. 根据可视化结果调整 `NPRStylePostProcessFeature` 中的 `Depth Threshold`、`Normal Threshold`、`Outline Thickness` 等参数。

## 调参建议

- Depth 视图过黑或过白：调整相机 Near/Far Plane（近裁剪面 / 远裁剪面）或调试视图的深度缩放。
- Normals 视图基本不变化：检查对象是否写入 DepthNormals Pass（深度法线通道）。
- Sobel 边缘过多：提高深度或法线阈值。
- Sobel 边缘过少：降低深度或法线阈值，并确认目标对象在相机深度范围内。

## 性能注意

调试可视化适合开发阶段使用。发布或展示时建议关闭不需要的调试窗口，避免额外 Blit（位块传输）和屏幕叠加开销。
