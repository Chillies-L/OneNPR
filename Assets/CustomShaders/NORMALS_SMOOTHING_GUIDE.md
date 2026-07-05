# NPR Normals Smoother Guide

`NPR_NormalsSmoother` 用于改善法线外扩描边在 UV 接缝、硬边或顶点分裂处出现的断裂问题。它会把平滑后的轮廓法线编码到顶点色（Vertex Color）中，Shader 再读取这些数据用于描边。

## 适用场景

- 模型外轮廓局部断线。
- 角色发梢、衣角、角、蹄、硬边装饰等位置描边不连续。
- 同一空间位置存在多个顶点，但这些顶点的法线方向不同。

## 基本用法

1. 选中需要修复描边的模型根节点或包含 `MeshFilter` 的对象。
2. 添加 `NPR_NormalsSmoother` 组件。
3. 保持默认顶点色格式 `YSADirect`。
4. 进入 Play Mode（播放模式），组件会在启动时处理网格。
5. 材质中确认 `Use Vertex Color Normals` 已启用。

## 工作原理

模型在 UV 接缝或硬边处可能存在位置相同但法线不同的顶点。法线外扩描边会沿每个顶点自己的法线方向扩展，因此这些顶点会向不同方向移动并形成间隙。

`NPR_NormalsSmoother` 会按顶点位置分组，计算每组的平均法线，再将平滑法线编码到顶点色：

```hlsl
encoded = normal * 0.5 + 0.5;
decoded = color.rgb * 2.0 - 1.0;
```

这样同一位置的分裂顶点会使用一致的描边法线，轮廓线更连续。

## 注意事项

- 如果模型本身已经使用顶点色存储 AO（Ambient Occlusion，环境遮蔽）、遮罩或其他数据，使用前需要确认通道不会冲突。
- SkinnedMeshRenderer（蒙皮网格渲染器）需要根据项目管线单独验证；静态 MeshFilter 是最直接的使用场景。
- 处理通常只需在启动或导入后执行一次，运行时持续开销很低。

## 排查

- 添加组件后无变化：确认材质使用 `Custom/NPR/Toon_Character_ImprovedOutline`，并启用了顶点色法线相关参数。
- 描边仍断裂：检查模型是否存在非流形结构、极薄面或过度硬边。
- 顶点色被覆盖：关闭该组件或改用外部建模工具烘焙平滑法线。
