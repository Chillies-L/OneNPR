# Outline Troubleshooting Guide

本指南用于排查 OneNPR 中的描边问题。OneNPR 目前包含两条描边路径：材质内的法线外扩描边（Normal Extrusion Outline）和后处理中的屏幕空间描边（Screen-Space Outline）。

## 描边方式选择

### 法线外扩描边

适合角色主体外轮廓，优点是线条跟随模型、性能稳定。缺点是依赖模型法线和顶点结构，硬边或 UV 接缝处可能出现断裂。

### 屏幕空间描边

适合统一场景线条、遮挡边和结构边。它基于深度、法线和可选颜色阈值进行 Sobel（索贝尔）边缘检测。缺点是属于后处理开销，参数需要按镜头和场景调节。

## 常见问题

### 法线外扩描边断裂

优先检查模型顶点结构和法线连续性。对硬边模型可尝试：

1. 添加 `NPR_NormalsSmoother`。
2. 开启材质中的 `Use Vertex Color Normals`。
3. 增大 `Outline Min Width`，限制过细线条。
4. 调整 `Miter Strength` 和 `Miter Max Scale`，改善拐角处线条。

### 描边过粗或过细

- 调整 `Outline Width`。
- 设置 `Outline Min Width` 和 `Outline Max Width` 限制变化范围。
- 检查相机距离、FOV（Field of View，视野角）和模型缩放。

### 某些角度描边消失

- 开启 `Render Reverse Pass`。
- 适当提高 `Reverse Pass Width Scale`。
- 调整 `Outline Z Offset`，减少 Z-fighting（深度冲突）。

### 屏幕空间描边线条太多

- 提高 `Depth Threshold` 或 `Normal Threshold`。
- 降低 `Outline Intensity`。
- 对颜色边界过敏时，关闭 `Use Color` 或提高 `Color Threshold`。

### 屏幕空间描边线条太少

- 降低 `Depth Threshold` 和 `Normal Threshold`。
- 确认 Renderer Feature（渲染器特性）请求了 Depth（深度）和 Normal（法线）输入。
- 确认目标对象使用的 Shader 写入了 DepthNormals Pass（深度法线通道）。

## 推荐调试流程

1. 先在简单球体或胶囊体上测试材质，确认 Shader 编译和基础描边正常。
2. 再切换到目标模型，判断问题是否来自模型法线。
3. 使用 `NPR_DebugVisualization` 查看 Depth、Normals 和 Sobel Edge。
4. 对角色外轮廓优先调法线外扩，对整体风格线条优先调后处理描边。
