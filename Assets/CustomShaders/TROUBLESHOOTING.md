# 故障排除指南 (Troubleshooting)

## 已知问题和解决方案

### ✅ 已修复：ShadowCaster编译错误

**错误信息**:
```
Shader error: undeclared identifier 'LerpWhiteTo'
```

**原因**: URP 14.0版本中`Shadows.hlsl`的`ApplyShadowBias`函数依赖可能不存在的函数。

**解决方案**: 已修改为自定义的简化阴影偏移实现（`ApplyShadowBiasSimple`），不依赖URP的内部函数。

**影响**: 阴影投射正常工作，可能在极端角度下有轻微的阴影失真（shadow acne），但对于卡通渲染影响极小。

---

## 常见编译错误

### 错误1: Shader未找到

**症状**: 
```
Shader 'Custom/NPR/Toon_Character' not found
```

**解决方案**:
1. 确认Shader文件在正确的路径：`Assets/CustomShaders/Toon_Character.shader`
2. 在Unity中右键Shader文件 → Reimport
3. 检查Console是否有编译错误

---

### 错误2: Render Feature未显示

**症状**: Add Renderer Feature列表中没有NPR相关选项

**解决方案**:
1. 检查C#脚本是否有编译错误（Console中查看）
2. 确认文件名与类名一致
3. 重启Unity Editor
4. 检查脚本是否在Editor文件夹外（Render Feature不应在Editor文件夹）

---

### 错误3: 材质显示全粉色/品红色

**症状**: 应用Toon Shader的材质显示为亮粉色

**原因**: Shader编译失败

**解决方案**:
1. 查看Console中的Shader错误
2. 选中材质，查看Inspector中的错误提示
3. 确认URP版本是14.x
4. 检查Shader中的Include路径是否正确

---

### 错误4: 描边不显示

**症状**: 材质应用后没有描边效果

**可能原因**:
1. Outline Width设置太小
2. Outline Color是透明的
3. 模型法线问题

**解决方案**:
```
步骤1: 检查参数
- Outline Width: 增大到0.01
- Outline Color: 确认Alpha=1.0（不透明）

步骤2: 测试简单模型
- 创建一个Unity Sphere
- 应用材质
- 如果Sphere有描边，说明是模型问题

步骤3: 法线修复
- 如果模型法线有问题，使用法线修复脚本
- 或在建模软件中重新导出
```

---

### 错误5: 屏幕空间描边无效

**症状**: 添加NPR Outline Feature后没有效果

**解决方案**:
```
步骤1: 检查Shader
- 确认SobelOutline.shader编译无错误
- 路径：Assets/CustomShaders/SobelOutline.shader

步骤2: 检查Feature设置
- Outline Thickness > 0
- 至少启用Use Depth或Use Normals
- Render Pass Event: Before Rendering Post Processing

步骤3: 检查DepthNormals Pass
- 确认Toon_Character.shader中的DepthNormals Pass无错误
- 场景中至少有一个使用Toon Shader的物体
```

---

### 错误6: Debug窗口不显示

**症状**: 添加Debug Visualization Feature后窗口不显示

**解决方案**:
```
步骤1: 检查开关
- 至少勾选一个Show窗口（Depth/Normals/Sobel/BaseColor）

步骤2: 检查Shader
- 确认DebugVisualization.shader编译无错误

步骤3: 检查设置
- Window Size不要设置为0
- Position选择合适的位置

步骤4: 运行游戏
- 必须在Play模式下才能看到Debug窗口
- Editor模式下不显示
```

---

### 错误7: 深度图全黑/全白

**症状**: Debug窗口的深度图显示不正常

**原因**: Depth Scale或相机设置问题

**解决方案**:
```
1. 调整Depth Scale（1.0-10.0）
2. 开启Linearize Depth
3. 检查相机的Near/Far Plane设置
   - Near: 0.1-0.3
   - Far: 100-1000
4. 确认场景中有物体渲染
```

---

### 错误8: 法线图全灰色

**症状**: Debug窗口的法线图颜色单一

**原因**: 法线数据未生成或全部朝向一致

**解决方案**:
```
1. 确认Toon Shader的DepthNormals Pass正常
2. 检查场景中物体是否正确渲染
3. 旋转相机角度，观察法线图变化
4. 尝试使用不同的模型（如Sphere、Capsule）测试
```

---

## URP版本兼容性

### 推荐版本
- Unity: 2022.3 LTS
- URP: 14.0.x

### 不同版本的调整

**URP 12.x**:
```csharp
// 在NPR_OutlineFeature.cs中
// 将 ScriptableRenderPassInput 改为旧版API
ConfigureInput(ScriptableRenderPassInput.Depth);
```

**URP 17.x (Unity 6)**:
```hlsl
// 可能需要使用新的Render Graph API
// 建议查阅Unity 6文档
```

---

## 性能问题

### 问题: 帧率下降严重

**症状**: 开启Debug窗口后FPS明显下降

**解决方案**:
```
1. 减小Window Size（推荐0.2-0.25）
2. 关闭不需要的窗口
3. 降低游戏分辨率
4. 在Release版本中禁用Debug Feature
```

---

## Unity编辑器问题

### 问题: Shader修改后不生效

**解决方案**:
```
1. 保存Shader文件（Ctrl+S）
2. 等待Unity自动重新编译
3. 如果还不行，手动重新导入：
   - 右键Shader文件
   - Reimport
4. 清除Shader缓存：
   - Edit → Preferences → GI Cache → Clean Cache
```

### 问题: 脚本修改后不生效

**解决方案**:
```
1. 检查Console是否有编译错误
2. 停止Play模式
3. 等待编译完成（右下角进度条）
4. 如果还不行，重启Unity
```

---

## 报告常见问题

### 问题: 截图分辨率太低

**解决方案**:
```
方案1: 使用Game窗口截图
- 设置较高的分辨率（1920x1080或更高）
- 使用Windows截图工具（Win+Shift+S）

方案2: 使用Unity Recorder
- Window → General → Recorder → Recorder Window
- 设置输出分辨率
- 录制单帧截图
```

### 问题: Debug窗口位置不对

**解决方案**:
```
在NPR_DebugVisualization Feature中调整：
- Position: Top Right / Top Left / Bottom Right / Bottom Left
- Window Size: 调整大小避免遮挡重要内容
```

---

## 快速诊断流程

遇到问题时，按以下顺序检查：

1. **Console检查**（最重要）
   - 是否有编译错误？
   - 是否有警告信息？

2. **Shader检查**
   - 选中材质，Inspector中是否显示Shader错误？
   - 重新导入Shader

3. **参数检查**
   - 所有相关参数是否合理？
   - 颜色的Alpha是否为1.0？
   - 宽度/阈值是否太小？

4. **场景检查**
   - 是否有光源？
   - 相机设置是否正确？
   - 是否在Play模式下？

5. **重启大法**
   - 重新导入资源
   - 重启Unity Editor
   - 清除缓存

---

## 联系与反馈

如果遇到文档中未提及的问题：

1. 记录错误信息（Console完整输出）
2. 记录Unity版本和URP版本
3. 记录复现步骤
4. 截图错误现象

---

## 版本历史

### v1.0 (2026-06-18)
- 初始版本
- 修复ShadowCaster编译错误
- 添加常见问题解答

---

*大多数问题都可以通过查看Console错误信息和重新导入资源解决。*
