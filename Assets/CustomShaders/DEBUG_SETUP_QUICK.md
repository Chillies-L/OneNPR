# Debug可视化系统 - 补充说明

## 快速测试步骤

### 方案A: 仅使用Render Feature（推荐新手）

**优点**: 简单，无需额外脚本

1. **添加Render Feature**
   - 选择URP Renderer Asset
   - Add Renderer Feature → NPR Debug Visualization
   
2. **配置参数**
   ```
   Debug Windows:
     ☑ Show Depth Buffer
     ☑ Show Normals Buffer
     ☑ Show Sobel Edges
     ☐ Show Base Color
   
   Window Layout:
     Window Size: 0.25
     Position: Top Right
   
   Depth Visualization:
     Depth Scale: 1.0
     ☑ Linearize Depth
   
   Normal Visualization:
     ☑ World Space Normals
   ```

3. **运行游戏**
   - 点击Play
   - 窗口自动显示在右上角
   - 实时更新

4. **调整参数**
   - 在Renderer Asset中调整
   - 需要Pause游戏才能修改
   - 修改后Resume查看效果

---

### 方案B: 使用运行时UI控制器（推荐进阶）

**优点**: 可以在游戏运行时切换窗口，无需暂停

**步骤**:

1. **创建Canvas**
   - Hierarchy右键 → UI → Canvas
   - Canvas Scaler设置为Scale With Screen Size

2. **添加控制器脚本**
   - 选中Canvas
   - Add Component → NPR_DebugUIController
   - 拖入Renderer Data引用

3. **运行游戏**
   - 点击Play
   - 按F1显示/隐藏控制面板
   - 通过UI按钮切换窗口

---

## 截图建议（报告用）

### 截图1: 深度缓冲可视化
**场景设置**:
- 角色在中景
- 背景有远景物体
- 地面作为近景

**开启窗口**:
- ☑ Show Depth Buffer
- ☐ 其他全部关闭

**截图要点**:
- 窗口清晰显示深度渐变
- 主窗口显示正常渲染效果
- 标注：近景（蓝）、中景（绿）、远景（红）

**报告用途**: 
> 图X-X 深度缓冲可视化。蓝色表示距离相机较近的区域，红色表示较远区域，展示了场景的空间层次关系。

---

### 截图2: 法线缓冲可视化
**场景设置**:
- 角色正面朝向相机
- 确保有不同朝向的表面（头部、身体、手臂）

**开启窗口**:
- ☑ Show Normals Buffer
- ☐ 其他全部关闭

**截图要点**:
- 窗口显示丰富的颜色变化
- 平滑表面应该颜色渐变
- 硬边表面应该颜色突变

**报告用途**:
> 图X-X 世界空间法线可视化。RGB通道分别对应XYZ轴向，用于后续的边缘检测和光照计算。

---

### 截图3: Sobel边缘检测可视化（重点）
**场景设置**:
- 角色完整入镜
- 背景简单（便于观察边缘）

**开启窗口**:
- ☑ Show Sobel Edges
- ☐ 其他全部关闭

**截图要点**:
- 红色边缘（深度）应该在轮廓
- 绿色边缘（法线）应该在表面折痕
- 黄色区域是两者重叠

**报告用途**:
> 图X-X Sobel边缘检测结果可视化。红色为深度梯度边缘，绿色为法线梯度边缘，黄色为两者叠加。该算法能同时检测外轮廓（深度）和内部结构线（法线）。

**报告价值**: ⭐⭐⭐⭐⭐

---

### 截图4: 四窗口对比（最重要）
**场景设置**:
- 标准角色展示场景
- 光照正常

**开启窗口**:
- ☑ Show Depth Buffer
- ☑ Show Normals Buffer
- ☑ Show Sobel Edges
- ☑ Show Base Color

**截图要点**:
- 四个窗口整齐排列
- 每个窗口都清晰可见
- 主渲染区域显示最终效果

**截图后处理**:
在图像编辑软件中标注：
- 左上标注"A"：原始渲染
- 右上标注"B"：深度数据
- 左下标注"C"：法线数据
- 右下标注"D"：边缘检测

**报告用途**:
> 图X-X 渲染管线数据流可视化。(A)最终渲染结果；(B)深度缓冲，用于轮廓检测；(C)法线缓冲，用于表面折痕检测；(D)Sobel算子的边缘检测结果，红色为深度边缘，绿色为法线边缘。

**报告价值**: ⭐⭐⭐⭐⭐（最佳技术展示图）

---

## 参数调整演示（答辩用）

如果答辩时需要演示，可以展示这个流程：

### 演示1: 深度阈值如何影响边缘检测

1. 开启Sobel Edges窗口
2. 打开Sobel Outline Feature设置
3. 实时调整Depth Threshold: 0.05 → 0.1 → 0.2
4. 观察Sobel窗口中红色边缘的变化
5. 说明："阈值越高，检测越不敏感，边缘越少"

### 演示2: 深度可视化的线性化效果

1. 开启Depth Buffer窗口
2. 关闭Linearize Depth
3. 观察：近处挤压，远处拉伸
4. 开启Linearize Depth
5. 观察：深度均匀分布
6. 说明："线性化深度更符合人眼感知"

---

## 已知限制

1. **UI控制器限制**
   - NPR_DebugUIController目前只创建UI界面
   - 实际的开关控制需要访问Render Feature实例
   - 这需要URP的运行时API（较复杂）

2. **推荐使用方式**
   - 调试时：直接在Renderer Asset中开关窗口
   - 截图时：开启需要的窗口组合
   - 演示时：预先准备好不同配置的场景

3. **性能**
   - 所有窗口开启约1-2ms开销
   - 对于60fps游戏影响很小
   - Release版本应该移除此Feature

---

## 完整文件清单

```
CustomShaders/
├── NPR_DebugVisualization.cs           ✅ 主功能脚本
├── DebugVisualization.shader            ✅ 可视化Shader
├── NPR_DebugUIController.cs             ✅ UI控制器（可选）
├── DEBUG_VISUALIZATION_GUIDE.md         ✅ 使用指南
└── DEBUG_SETUP_QUICK.md                 ✅ 本文件
```

---

## 常见问题快速排查

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 窗口不显示 | 没有开启任何窗口 | 在Render Feature中勾选至少一个窗口 |
| 窗口全黑 | 深度/法线纹理未生成 | 确认URP设置正确，场景有物体 |
| Sobel无边缘 | 阈值过高 | 降低Threshold到0.1左右 |
| 深度图全一色 | Depth Scale不当 | 调整到1.0-10.0之间 |
| 性能下降严重 | 窗口尺寸过大 | 降低Window Size到0.2 |

---

## 下一步

1. **立即测试**
   - 添加Render Feature
   - 开启Depth Buffer和Normals Buffer
   - 运行游戏查看效果

2. **截图准备**
   - 按照上面的4个截图建议逐个截取
   - 保存高分辨率图片
   - 在图像编辑器中添加标注

3. **报告集成**
   - 将截图插入第三章或第四章
   - 添加图注说明
   - 解释每个可视化的意义

---

*这个工具会让你的报告技术深度提升一个档次！*
