# NPR Toon Character Shader 使用文档

## 概述

这是一套为Unity URP开发的自定义NPR（非真实感渲染）"三渲二"卡通角色着色器工具包，包含：

1. **Toon_Character.shader** - 核心卡通着色器（三段色阶 + 硬高光 + 边缘光 + 法线外扩描边）
2. **NPR_OutlineFeature.cs + SobelOutline.shader** - 屏幕空间Sobel边缘检测描边
3. **ToonShaderGUI.cs** - 自定义Material Inspector界面，支持参数分组和快速预设

---

## 功能特性

### ✅ 核心功能（已实现）

#### 1. 三段色阶卡通光照
- **Base Color**: 亮部/非阴影区域
- **1st Shade Color**: 主阴影区域
- **2nd Shade Color**: 深阴影区域
- 使用Half-Lambert和smoothstep实现平滑或硬边阴影过渡
- 支持主光源和附加光源

#### 2. 两种描边方式

**方式A: 法线外扩描边（Outline Pass）**
- 在Shader内通过额外的Pass实现
- 沿顶点法线方向外扩顶点
- 自动根据相机距离缩放描边宽度
- 支持描边宽度贴图（可选）
- 优点：单物体效果好，性能高
- 缺点：硬表面模型易出现描边断裂

**方式B: 屏幕空间Sobel描边（Render Feature）**
- 基于深度和法线的边缘检测
- 使用Sobel算子检测梯度
- 全屏统一描边风格
- 可独立控制深度/法线检测
- 优点：轮廓完整，风格统一
- 缺点：需要额外的后处理开销

#### 3. 其他渲染特性
- **硬高光（Specular）**: 基于Blinn-Phong的卡通化高光
- **边缘光（Rim Light）**: 类Fresnel效果的轮廓高亮
- **阴影投射**: ShadowCaster Pass支持正确的阴影投射
- **深度法线**: DepthNormals Pass为屏幕空间描边提供数据

---

## 快速开始

### 步骤1: 创建材质

1. 在Project窗口右键 → Create → Material
2. 在Inspector中将Shader改为 `Custom/NPR/Toon_Character`
3. 材质Inspector会显示自定义的分组界面

### 步骤2: 配置材质参数

**基础颜色 (Base Colors)**
- `Base Texture`: 角色的漫反射贴图
- `Base Color`: 亮部颜色（白色或角色主色调）
- `1st Shade Color`: 一级阴影颜色（通常为Base的0.7-0.8倍）
- `2nd Shade Color`: 二级阴影颜色（通常为Base的0.4-0.5倍）

**光照控制 (Shading Control)**
- `Base Step`: 控制一级阴影出现的位置（0.5-0.6推荐）
- `Base Feather`: 一级阴影边界的羽化程度（0=硬边，0.05=软边）
- `Shade Step`: 控制二级阴影出现的位置（0.2-0.3推荐）
- `Shade Feather`: 二级阴影边界的羽化程度

**高光 (Specular)**
- `Specular Color`: 高光颜色（通常为白色）
- `Specular Size`: 高光大小（0.05-0.15）
- `Specular Intensity`: 高光强度（0.5-2.0）

**边缘光 (Rim Light)**
- `Rim Color`: 边缘光颜色
- `Rim Power`: 边缘光宽度（2-5）
- `Rim Intensity`: 边缘光强度（0.3-0.8）

**描边 (Outline)**
- `Outline Width`: 描边宽度（0.003-0.01）
- `Outline Color`: 描边颜色（通常为黑色）
- `Outline Width Map`: 可选，控制不同部位的描边粗细

### 步骤3: 使用快速预设

Inspector底部提供三个快速预设按钮：

1. **普通漫画风格**: 标准日式动漫卡通效果，适中的阴影对比
2. **JOJO黑白风格**: 高对比黑白效果，强边缘光，更粗描边
3. **美漫网点风格**: 硬边阴影，有限配色（需配合屏幕空间网点后处理）

点击按钮后可在此基础上微调参数。

---

## 屏幕空间Sobel描边配置

### 步骤1: 添加Render Feature

1. 选中你的URP Renderer Asset（通常在 `Settings/URP-*-Renderer`）
2. 在Inspector底部点击 `Add Renderer Feature`
3. 选择 `NPR Outline Feature`

### 步骤2: 配置描边参数

**Outline Settings**
- `Outline Color`: 描边颜色
- `Outline Thickness`: 描边粗细（1.0-3.0）

**Edge Detection**
- `Depth Threshold`: 深度梯度阈值（0.05-0.2），控制深度差异的敏感度
- `Normal Threshold`: 法线梯度阈值（0.2-0.5），控制法线变化的敏感度
- `Use Depth`: 是否使用深度检测边缘
- `Use Normals`: 是否使用法线检测边缘

**Advanced**
- `Render Pass Event`: 渲染时机（默认Before Rendering Post Processing）

> **注意**: 屏幕空间描边会对场景中所有物体生效。如需只描边特定对象，需要使用Layer Mask或Stencil Buffer（L2拓展功能）。

---

## 三种风格实现要点

### 1. 普通漫画三渲二
- 色阶量化：`BaseStep=0.55, BaseFeather=0.05`
- 中等高光：`SpecularSize=0.08, Intensity=1.0`
- 轻边缘光：`RimPower=3.0, Intensity=0.3`
- 中等描边：`OutlineWidth=0.005`

### 2. JOJO黑白漫画
- 高对比色阶：`BaseStep=0.6, ShadeStep=0.35`
- 硬边界：`Feather=0.01-0.02`
- 强高光：`SpecularIntensity=1.5`
- 强边缘光：`RimIntensity=0.8`（白色或淡蓝色）
- 粗描边：`OutlineWidth=0.008`
- 可选：在后处理中添加去色效果

### 3. 美漫网点
- 有限配色：暖色调Base Color
- 硬边阴影：`Feather=0.01`
- 粗描边：`OutlineWidth=0.007`，深棕色或深红色
- **关键**: 需配合屏幕空间Halftone网点后处理（L1未实现，L2/L3拓展）

---

## 算法原理说明（报告用）

### 三段色阶光照量化

**原理**:
传统Lambert光照产生连续渐变（`diffuse = NdotL`），不符合漫画风格。卡通渲染通过阈值函数将连续明暗量化为离散色阶。

**实现**:
```hlsl
float halfLambert = NdotL * 0.5 + 0.5;  // 将[-1,1]映射到[0,1]
float toShade1 = 1.0 - smoothstep(BaseStep - Feather, BaseStep + Feather, halfLambert);
```

- `smoothstep`: 平滑阶跃函数，在[a, b]区间内从0过渡到1
- `BaseStep`: 阴影阈值，控制何时出现阴影
- `Feather`: 羽化值，控制过渡硬度（0=硬切，>0=软过渡）

**为什么有效**:
将连续的光照强度（0-1）通过阈值划分为3个区间，每个区间对应一个预设颜色，产生漫画中常见的块面阴影。

---

### 法线外扩描边

**原理**:
在物体周围绘制一圈外扩的壳层，只渲染背面（Cull Front），形成描边效果。

**实现**:
```hlsl
// 在顶点着色器中
float3 positionWS = TransformObjectToWorld(positionOS);
float3 normalWS = TransformObjectToWorldNormal(normalOS);
positionWS += normalize(normalWS) * outlineWidth;  // 沿法线外扩
output.positionHCS = TransformWorldToHClip(positionWS);
```

**相机距离缩放**:
```hlsl
float distance = length(positionWS - cameraPos);
float distanceScale = saturate(distance * 0.2);
float finalWidth = baseWidth * (0.5 + distanceScale * 0.5);
```

防止近处描边过粗、远处描边消失。

**优点与缺点**:
- ✅ 性能好，单物体效果佳
- ❌ 法线质量差的硬表面模型会出现断线
- ❌ 无法处理物体间的轮廓线

---

### Sobel边缘检测描边

**原理**:
Sobel算子是图像处理中的经典边缘检测算法，通过计算像素邻域的梯度来识别边缘。

**Sobel算子**:
```
水平核 Gx:        垂直核 Gy:
[-1  0  1]        [-1 -2 -1]
[-2  0  2]        [ 0  0  0]
[-1  0  1]        [ 1  2  1]
```

对深度或法线应用卷积核，计算梯度幅值：
```hlsl
float Gx = -d00 + d20 - 2*d01 + 2*d21 - d02 + d22;
float Gy = -d00 - 2*d10 - d20 + d02 + 2*d12 + d22;
float edge = sqrt(Gx*Gx + Gy*Gy);
```

**为什么使用深度+法线**:
- **深度**: 检测物体轮廓（深度突变处）
- **法线**: 检测表面折痕（法线变化处）
- 两者结合：既有外轮廓又有内部结构线

**优点与缺点**:
- ✅ 全局统一风格，轮廓完整
- ✅ 可检测物体间遮挡边缘
- ❌ 需要额外的后处理Pass，有性能开销
- ❌ 阈值调整需要经验

---

## 常见问题

### Q1: 描边出现断裂或缺失
**原因**: 法线外扩描边依赖模型法线质量。硬表面、低精度法线会导致描边不连续。

**解决方案**:
1. 在建模软件中平滑法线（Smooth Normals）
2. 使用`MeshFilterNormalsFix.cs`脚本修复法线（参考YSA Toon）
3. 改用屏幕空间Sobel描边

### Q2: 描边太粗/太细
**调整**:
- 法线外扩：调整`Outline Width`（0.003-0.01）
- 屏幕空间：调整`Outline Thickness`（1.0-3.0）
- 检查相机FOV和角色距离

### Q3: 阴影边界太硬/太软
**调整**: `Base Feather`和`Shade Feather`
- 硬边漫画风格：Feather=0.01-0.02
- 软边动漫风格：Feather=0.05-0.1

### Q4: 屏幕空间描边检测不到边缘
**检查**:
1. URP Renderer中是否添加了`NPR Outline Feature`
2. `Use Depth`和`Use Normals`是否开启
3. 阈值是否过高（降低Threshold值）
4. 相机是否开启了DepthTexture（URP自动管理，一般无需手动设置）

### Q5: 角色投影异常或全黑
**检查**:
1. Shader中的`ShadowCaster` Pass是否正常
2. 场景中的平行光是否开启了阴影（Shadow Type不为None）
3. 材质的Rendering Mode是否正确

---

## 文件结构

```
CustomShaders/
├── Toon_Character.shader          # 核心卡通角色着色器
├── SobelOutline.shader             # 屏幕空间Sobel描边着色器
├── NPR_OutlineFeature.cs           # URP Render Feature脚本
└── Editor/
    └── ToonShaderGUI.cs            # 自定义Material Inspector GUI
```

---

## 技术依赖

- **Unity版本**: 2022.3 LTS 及以上
- **渲染管线**: Universal Render Pipeline (URP) 14.x
- **Shader语言**: HLSL (ShaderLab)
- **C# Framework**: .NET Standard 2.1

---

## 报告撰写建议

### 第三章「系统设计与实现」可以这样写：

#### 3.1 总体架构
绘制模块框图：
- 核心Toon Shader（ForwardLit + Outline Pass）
- 屏幕空间后处理（Render Feature + Blit Pass）
- 材质参数管理（ShaderGUI + ScriptableObject预设）

#### 3.2 核心算法实现

**3.2.1 三段色阶卡通光照**
- 理论推导：Lambert → Half-Lambert → Smoothstep量化
- 代码片段：`ApplyThreeColorToon`函数
- 参数分析：Step和Feather如何影响视觉效果

**3.2.2 描边对比研究**
- 法线外扩法：原理、优缺点、适用场景
- Sobel边缘检测：算子公式、深度法线双重检测、阈值调优
- 对比表格：性能、效果、适用模型类型

**3.2.3 其他渲染特性**
- 硬高光：Blinn-Phong → Step函数硬化
- 边缘光：Fresnel近似 → Rim Light公式

#### 3.3 工程化设计
- 自定义ShaderGUI的参数分组策略
- 快速预设系统的设计思路
- URP多Pass架构（ForwardLit, Outline, ShadowCaster, DepthNormals）

#### 3.4 Bug解决记录
列举实际遇到的问题和解决过程：
- 描边断裂 → 法线修复
- 远近描边不均 → 相机距离缩放
- Z-fighting → Offset调整
- 背面剔除导致描边消失 → Cull Front

---

## 下一步扩展（L2/L3）

### L2功能
- [ ] 运行时UI面板（滑块实时调参）
- [ ] ScriptableObject预设系统
- [ ] 屏幕空间Halftone网点后处理
- [ ] FPS显示

### L3功能
- [ ] 新海诚环境风格（HDR + Bloom + ToneMapping）
- [ ] 骨骼动画播放
- [ ] 性能优化（GPU Instancing, SRP Batcher）

---

## 版权与引用声明

本项目是计算机图形学课程结课作业，核心算法为自主实现。参考了以下开源项目的思想（未直接复制代码）：

1. **Unity Toon Shader (UTS3)** - Unity Technologies
   - 参考：三段色阶参数结构、法线外扩描边思路
   - 链接：https://github.com/Unity-Technologies/com.unity.toonshader

2. **YSA Toon Shader** - Shader Graph实现的卡通着色器
   - 参考：相机距离缩放公式、Shader Graph节点组织
   - 链接：Unity Asset Store

3. **算法理论**：
   - Sobel算子：经典图像处理算法
   - Blinn-Phong光照模型：《Real-Time Rendering》第三版

所有核心算法均有详细的中英文注释，体现对原理的理解。

---

## 作者信息

- **项目名称**: URP 自定义 NPR 三渲二 Shader 工具包
- **课程**: 计算机图形学
- **开发周期**: 2026年6月
- **引擎**: Unity 2022 LTS + URP 14.0.12

---

*本README对应项目L1阶段交付。更新内容请参见版本历史。*
