# NPR Debug可视化系统 使用指南

## 概述

这是一个强大的调试工具，可以在运行时实时显示渲染管线的中间结果，帮助你：
- 🔍 调试Shader效果
- 📸 为报告截取专业的技术分析图
- 🎓 理解渲染管线的工作原理
- 🐛 快速定位渲染问题

---

## 功能特性

### 可视化窗口（4种）

1. **深度缓冲 (Depth Buffer)**
   - 显示场景的深度信息
   - 颜色渐变：蓝色（近）→ 红色（远）
   - 可调深度缩放系数
   - 支持线性化深度显示

2. **法线缓冲 (Normals Buffer)**
   - 显示世界空间法线
   - 颜色编码：
     - 红色通道 = X轴方向
     - 绿色通道 = Y轴方向
     - 蓝色通道 = Z轴方向

3. **Sobel边缘检测 (Sobel Edges)**
   - 实时显示边缘检测结果
   - 颜色编码：
     - 红色 = 深度边缘
     - 绿色 = 法线边缘
     - 黄色 = 两者重叠

4. **基础颜色 (Base Color)**
   - 显示未处理的相机颜色缓冲
   - 用于对比原始渲染结果

---

## 快速设置（3步）

### 步骤1: 添加Render Feature

1. 选中你的URP Renderer Asset
   - 路径通常在：`Assets/Settings/URP-*-Renderer`
   
2. 在Inspector底部点击 `Add Renderer Feature`

3. 选择 `NPR Debug Visualization`

4. 配置Debug Windows（勾选你想显示的窗口）：
   ```
   ☑ Show Depth Buffer
   ☑ Show Normals Buffer
   ☑ Show Sobel Edges
   ☐ Show Base Color
   ```

### 步骤2: 配置窗口布局

在Render Feature的设置中：

**Window Layout**
- `Window Size`: 窗口大小（0.1-0.5，推荐0.25）
- `Position`: 窗口位置
  - Top Right（右上角，推荐）
  - Top Left（左上角）
  - Bottom Right（右下角）
  - Bottom Left（左下角）

**Depth Visualization**
- `Depth Scale`: 深度缩放（1.0-100.0）
  - 值越大，近处细节越明显
  - 推荐值：1.0（默认）
- `Linearize Depth`: 线性化深度
  - ☑ 开启：均匀显示深度变化
  - ☐ 关闭：使用原始深度值

**Normal Visualization**
- `World Space Normals`: 使用世界空间法线（推荐开启）

### 步骤3: 运行游戏

1. 点击Unity的Play按钮
2. 你会在屏幕角落看到Debug窗口
3. 实时显示各种渲染数据

---

## 使用场景

### 场景1: 调试三渲二阴影边界

**目的**: 查看阴影Step阈值如何影响深度判断

**操作**:
1. 开启 `Show Depth Buffer`
2. 开启 `Show Normals Buffer`
3. 调整Toon Shader的 `Base Step` 参数
4. 观察深度图中的亮暗分界与阴影的对应关系

**预期效果**: 
- 深度图显示场景的远近关系
- 可以看到哪些区域会被判定为阴影

---

### 场景2: 对比两种描边方法

**目的**: 为报告准备"算法对比"截图

**操作**:
1. 开启 `Show Sobel Edges`
2. 关闭屏幕空间描边Render Feature
3. 截图（只有法线外扩描边）
4. 开启屏幕空间描边Render Feature
5. 截图（法线外扩 + Sobel描边）
6. 对比Sobel窗口显示的边缘与实际描边的差异

**报告价值**: ⭐⭐⭐⭐⭐
- 直观展示两种算法的差异
- Sobel窗口展示算法原理
- 最终效果展示算法应用

---

### 场景3: 调试描边断裂问题

**目的**: 找出法线外扩描边为什么断线

**操作**:
1. 开启 `Show Normals Buffer`
2. 观察断线位置的法线颜色
3. 如果法线突变（颜色跳跃大），说明模型法线有问题
4. 使用法线修复脚本或重新导出模型

**关键点**:
- 法线图应该平滑过渡
- 突然的颜色变化 = 法线不连续 = 描边断裂

---

### 场景4: 验证深度检测阈值

**目的**: 调整Sobel描边的Depth Threshold

**操作**:
1. 开启 `Show Depth Buffer`
2. 开启 `Show Sobel Edges`
3. 调整Sobel Outline Feature的 `Depth Threshold`
4. 观察Sobel窗口中红色边缘的变化
5. 找到最佳阈值（检测到轮廓，但不过度敏感）

**调试技巧**:
- Threshold太低 → Sobel窗口全是红色（过度检测）
- Threshold太高 → Sobel窗口几乎没有红色（漏检）
- 合适的值 → 只在物体轮廓处有红色

---

## 报告截图建议

### 必须截取的技术分析图

#### 1. 深度图展示
```
用途：说明场景的深度布局
标注：近景（蓝色）、中景（绿色）、远景（红色）
报告位置：第二章"相关技术"或第三章"深度采样"
```

#### 2. 法线图展示
```
用途：说明法线编码原理
标注：X轴（红）、Y轴（绿）、Z轴（蓝）
报告位置：第三章"法线采样与边缘检测"
```

#### 3. Sobel边缘检测可视化
```
用途：展示Sobel算子的工作原理
标注：深度边缘（红）、法线边缘（绿）、组合结果（黄）
报告位置：第三章"屏幕空间边缘检测实现"
价值：⭐⭐⭐⭐⭐（核心采分点）
```

#### 4. 四窗口并列对比
```
用途：完整展示渲染管线的数据流
布局：
  左上：Base Color（原始颜色）
  右上：Depth Buffer（深度数据）
  左下：Normals Buffer（法线数据）
  右下：Sobel Edges（边缘检测结果）
报告位置：第三章"系统架构"或第四章"测试结果"
价值：⭐⭐⭐⭐⭐（体现技术深度）
```

---

## 高级技巧

### 技巧1: 自定义窗口布局

在`NPR_DebugVisualization.cs`中，你可以修改窗口排列：

```csharp
// 修改GetWindowPosition方法，实现自定义布局
// 例如：横向排列所有窗口
x = padding + (windowIndex * (windowWidth + padding));
y = padding;
```

### 技巧2: 添加更多可视化

你可以在`DebugVisualization.shader`中添加新的Pass：

```hlsl
// Pass 5: 显示高光遮罩
Pass
{
    Name "Debug Specular Mask"
    // ... 实现代码
}
```

### 技巧3: 录制对比视频

1. 使用Unity Recorder插件
2. 开启想要展示的Debug窗口
3. 实时调整参数（如Base Step）
4. 录制窗口变化过程
5. 用于答辩演示或报告附件

---

## 常见问题

### Q1: 窗口显示全黑
**原因**: 深度或法线纹理未生成

**解决方案**:
1. 确认URP Asset中开启了Depth Texture
2. 确认场景中有物体渲染
3. 检查相机是否正确设置

### Q2: Sobel窗口没有边缘
**原因**: 阈值设置过高

**解决方案**:
1. 降低`Depth Threshold`到0.05-0.1
2. 降低`Normal Threshold`到0.2-0.4
3. 确保场景中有明显的轮廓

### Q3: 深度图全蓝色或全红色
**原因**: Depth Scale设置不当

**解决方案**:
1. 调整`Depth Scale`（推荐1.0-10.0）
2. 开启`Linearize Depth`
3. 调整相机的Near/Far平面

### Q4: 法线图颜色不对
**原因**: 法线空间不匹配

**解决方案**:
1. 确认`World Space Normals`设置正确
2. 检查Shader中的法线输出
3. 验证DepthNormals Pass是否正常工作

### Q5: 窗口位置重叠
**原因**: 窗口尺寸太大或开启窗口太多

**解决方案**:
1. 减小`Window Size`（推荐0.2-0.25）
2. 关闭不需要的窗口
3. 选择合适的`Position`（角落位置）

---

## 性能影响

### 开销分析

| 窗口类型 | GPU开销 | 说明 |
|---------|---------|------|
| Depth Buffer | 极低 | 只读取现有深度纹理 |
| Normals Buffer | 极低 | 只读取现有法线纹理 |
| Sobel Edges | 低 | 需要采样9个像素并计算梯度 |
| Base Color | 极低 | 直接显示颜色缓冲 |

**总体影响**: 所有窗口同时开启约消耗1-2ms（1080p分辨率）

**建议**:
- 调试时随意开启所有窗口
- 截图后关闭不需要的窗口
- Release版本应该禁用此功能

---

## 代码扩展指南

### 添加新的可视化类型

#### Step 1: 在Shader中添加新Pass

编辑`DebugVisualization.shader`：

```hlsl
// Pass 6: 自定义可视化
Pass
{
    Name "Debug Custom"
    // ... 顶点和片元着色器
}
```

#### Step 2: 在C#中添加开关

编辑`NPR_DebugVisualization.cs`：

```csharp
public bool showCustom = false;

// 在Execute方法中添加：
if (settings.showCustom)
{
    DrawDebugWindow(cmd, descriptor, currentWindow, windowCount, "Custom", 5);
    currentWindow++;
}
```

#### Step 3: 测试并调整

1. 重新编译
2. 在Render Feature中勾选新选项
3. 运行测试

---

## 报告撰写建议

### 在报告中如何使用这些截图

#### 第二章：相关技术与理论基础

**2.3 屏幕空间后处理基础**

> 屏幕空间后处理基于对深度缓冲和法线缓冲的采样与计算。图2-1展示了Unity URP管线中深度信息的可视化...
> 
> [插入：Depth Buffer截图]
> 
> 图中蓝色表示距离相机较近的像素，红色表示较远的像素...

#### 第三章：系统设计与实现

**3.2.3 Sobel边缘检测算法实现**

> 为验证Sobel算子的正确性，本项目实现了调试可视化工具。图3-5展示了Sobel算子分别在深度和法线上的检测结果...
>
> [插入：Sobel Edges截图，标注红色=深度边缘，绿色=法线边缘]
>
> 从图中可以看出，深度边缘主要检测物体轮廓，法线边缘检测表面折痕...

#### 第四章：测试与结果分析

**4.2 描边算法对比实验**

> 表4-1对比了法线外扩描边与屏幕空间Sobel描边的特点。通过调试可视化工具，可以直观地看到Sobel算子如何检测边缘...
>
> [插入：四窗口对比图]

---

## 快捷键参考

| 按键 | 功能 |
|------|------|
| F1 | 切换UI控制面板（如果使用了NPR_DebugUIController） |

---

## 文件清单

```
CustomShaders/
├── NPR_DebugVisualization.cs      # Render Feature主脚本
├── DebugVisualization.shader       # 可视化着色器（5个Pass）
└── NPR_DebugUIController.cs        # 运行时UI控制器（可选）
```

---

## 技术细节

### 深度可视化原理

```hlsl
// 原始深度（0-1非线性）
float rawDepth = SampleSceneDepth(uv);

// 转换为线性深度（0=近平面，1=远平面）
float linearDepth = Linear01Depth(rawDepth, _ZBufferParams);

// 应用颜色渐变
float3 color = lerp(nearColor, farColor, linearDepth);
```

### 法线可视化原理

```hlsl
// 法线范围：[-1, 1]
float3 normal = SampleSceneNormals(uv);

// 映射到颜色空间：[0, 1]
float3 color = normal * 0.5 + 0.5;

// RGB通道直接对应XYZ轴
```

### Sobel算子实现

```hlsl
// 3x3采样
float d00, d10, d20, d01, d21, d02, d12, d22;

// 应用Sobel核
float Gx = -d00 + d20 - 2*d01 + 2*d21 - d02 + d22;
float Gy = -d00 - 2*d10 - d20 + d02 + 2*d12 + d22;

// 梯度幅值
float edge = sqrt(Gx*Gx + Gy*Gy);
```

---

## 总结

这个Debug可视化系统是一个强大的工具，可以：
- ✅ 帮助你理解渲染管线
- ✅ 快速定位Shader问题
- ✅ 为报告提供专业的技术分析图
- ✅ 在答辩时展示技术深度

**建议**:
1. 在开发过程中充分利用这个工具
2. 为报告准备高质量的截图
3. 在答辩时演示实时调试过程
4. 理解每个窗口背后的原理

---

*这是计算机图形学课程的调试工具，体现了对渲染管线的深入理解。*
