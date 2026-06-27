# 快速测试指南

## 立即测试步骤

### 1. 在Unity中导入文件

确保以下文件已正确放置：
```
Assets/CustomShaders/
├── Toon_Character.shader           ✅ 已创建
├── SobelOutline.shader              ✅ 已创建
├── NPR_OutlineFeature.cs            ✅ 已创建
└── Editor/
    └── ToonShaderGUI.cs             ✅ 已创建
```

### 2. 创建测试材质

1. 在Project窗口右键 → `Create → Material`
2. 命名为 `Mat_ToonTest`
3. 在Inspector中，将Shader改为 `Custom/NPR/Toon_Character`
4. 你应该会看到自定义的分组界面

### 3. 快速验证法线外扩描边

**最简测试（5分钟）**：

1. **场景准备**：
   - 创建场景，添加Directional Light
   - 添加Plane作为地面

2. **测试物体**：
   - 添加一个Sphere或Unity默认模型
   - 将`Mat_ToonTest`材质拖到模型上

3. **预期效果**：
   - 模型应该显示三段色阶的卡通光照
   - 边缘应该有黑色描边
   - 亮部有白色高光点

4. **参数调整测试**：
   ```
   调整 Outline Width: 0.003 → 0.01 → 观察描边变粗
   调整 Base Step: 0.3 → 0.7 → 观察阴影位置变化
   调整 Specular Size: 0.05 → 0.2 → 观察高光范围变化
   ```

### 4. 测试屏幕空间Sobel描边

1. **配置URP Renderer**：
   - 找到你的URP Renderer Asset（通常在`Settings/`文件夹）
   - Inspector底部点击 `Add Renderer Feature`
   - 选择 `NPR Outline Feature`

2. **配置参数**：
   ```
   Outline Color: Black (0,0,0,1)
   Outline Thickness: 1.5
   Depth Threshold: 0.1
   Normal Threshold: 0.4
   Use Depth: ✓
   Use Normals: ✓
   ```

3. **预期效果**：
   - 场景中所有物体都有统一的黑色描边
   - 物体间遮挡处有清晰边缘
   - 可以实时调整Thickness观察效果

### 5. 验证快速预设

在材质Inspector底部点击三个预设按钮，观察参数变化：
- **普通漫画风格**: 中等对比，平衡的光影
- **JOJO黑白风格**: 高对比，强边缘光，粗描边
- **美漫网点风格**: 硬边阴影，暖色调

### 6. 常见问题快速排查

| 问题 | 检查项 | 解决方案 |
|------|--------|----------|
| 材质全黑 | 场景中是否有光源？ | 添加Directional Light |
| 看不到描边 | Outline Width是否太小？ | 增大到0.008 |
| Shader编译错误 | Console是否有错误？ | 检查URP包版本是否≥14.0 |
| 屏幕空间描边无效 | Render Feature是否添加？ | 重新添加到Renderer |

---

## 带角色模型测试（推荐）

### 使用VRoid角色测试

1. **获取模型**：
   - 打开VRoid Studio（免费）
   - 快速创建一个角色
   - 导出为`.vrm`格式

2. **导入Unity**：
   - 安装UniVRM插件（Unity Package Manager）
   - 拖入`.vrm`文件到Project窗口

3. **应用材质**：
   - 将角色的所有材质改为`Mat_ToonTest`
   - 点击"普通漫画风格"预设
   - 调整Outline Width到0.005

4. **预期效果**：
   - 角色呈现典型的二次元动漫效果
   - 脸部、身体有清晰的明暗分界
   - 轮廓线流畅
   - 头发、衣服有高光

---

## 截图建议（用于报告）

### 必须截图：
1. **对比图**：同一角色，普通光照 vs 三渲二
2. **描边对比**：法线外扩 vs 屏幕空间Sobel
3. **参数对比**：不同BaseStep值的效果（0.3, 0.5, 0.7）
4. **风格对比**：三种预设的并排对比
5. **Inspector界面**：展示自定义ShaderGUI的分组面板

### 加分截图：
6. 不同光照角度下的效果
7. 多个角色同时渲染
8. 开启/关闭各个效果的AB对比（高光、边缘光、描边）

---

## 性能测试

### 简单性能对比

```csharp
// 在场景中添加FPS显示脚本
using UnityEngine;
using UnityEngine.UI;

public class SimpleFPS : MonoBehaviour
{
    public Text fpsText;
    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }
}
```

**测试场景**：
- 5个使用Toon Shader的角色
- 对比：只用法线描边 vs 同时开启屏幕空间描边
- 记录FPS，写入报告

---

## 下一步（完成测试后）

### ✅ L1阶段已完成：
- [x] 三渲二卡通光照（三段色阶）
- [x] 硬高光
- [x] 边缘光
- [x] 法线外扩描边
- [x] 屏幕空间Sobel描边
- [x] 自定义ShaderGUI
- [x] 三种风格快速预设

### 🔄 下一步（L2）：
1. 创建ScriptableObject预设资产
2. 运行时UI面板（滑块调参）
3. 屏幕空间Halftone网点后处理
4. 相机轨道控制脚本

### 📝 报告准备：
- 截图已准备
- 算法原理在README中已整理
- Bug记录（如遇到）
- 性能数据收集

---

## 紧急调试命令

如果Unity中Shader无法识别，在Project窗口右键Shader文件 → `Reimport`

如果Render Feature不显示：
```
1. 检查NPR_OutlineFeature.cs是否在Editor文件夹外
2. 确认没有编译错误
3. 重启Unity Editor
```

---

## 联系与反馈

测试过程中遇到问题，记录以下信息便于调试：
- Unity版本
- URP版本
- 错误信息截图
- Console输出

---

*祝测试顺利！*
