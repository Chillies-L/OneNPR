# 完整测试流程 - Step by Step

**测试时间**: 约30-40分钟  
**目标**: 验证所有功能 + 截取报告用图片

---

## 准备工作（5分钟）

### Step 0.1: 打开Unity项目
1. 启动Unity Hub
2. 打开你的项目：`Unity_Project/Graphic`
3. 等待Unity加载完成

### Step 0.2: 检查编译状态
1. 查看Console窗口（Window → General → Console）
2. **必须0错误**（允许有警告）
3. 如果有错误，参考`TROUBLESHOOTING.md`

### Step 0.3: 准备场景
```
1. 创建新场景：
   - File → New Scene
   - 选择 Basic (Built-in) 模板
   - 保存为：TestScene

2. 场景基本设置：
   - 保留默认的 Main Camera
   - 保留默认的 Directional Light
   - 删除默认的Plane（如果有）
```

### Step 0.4: 添加测试物体
```
1. 创建测试平面（地面）：
   - GameObject → 3D Object → Plane
   - 命名：Ground
   - Transform:
     - Position: (0, 0, 0)
     - Rotation: (0, 0, 0)
     - Scale: (2, 1, 2)

2. 创建测试球体（主角色）：
   - GameObject → 3D Object → Sphere
   - 命名：TestCharacter
   - Transform:
     - Position: (0, 1, 0)
     - Rotation: (0, 0, 0)
     - Scale: (1, 1, 1)

3. 创建背景物体（测试深度）：
   - GameObject → 3D Object → Cube
   - 命名：Background
   - Transform:
     - Position: (0, 0.5, -3)
     - Scale: (3, 1, 0.2)
```

### Step 0.5: 调整相机和光照
```
相机设置：
- 选中 Main Camera
- Position: (0, 2, -5)
- Rotation: (10, 0, 0)
- 点击 Align With View（如果需要调整视角）

光照设置：
- 选中 Directional Light
- Rotation: (50, -30, 0)
- Intensity: 1
- Shadow Type: Soft Shadows
- ✅ 确保阴影已开启！
```

---

## 测试Part 1: 基础Toon Shader（10分钟）

### Step 1.1: 创建Toon材质
```
1. Project窗口：
   - 右键 → Create → Material
   - 命名：Mat_ToonBasic

2. Inspector设置：
   - Shader: Custom/NPR/Toon_Character
   - ✅ 检查是否显示自定义界面（分组折叠面板）
   - ✅ 如果显示"Shader not found"，参考故障排除文档
```

### Step 1.2: 配置基础参数
```
在 Mat_ToonBasic 的Inspector中：

【基础颜色】
- Base Color: 白色 (1, 1, 1, 1)
- 1st Shade Color: 浅灰 (0.8, 0.8, 0.8, 1)
- 2nd Shade Color: 深灰 (0.5, 0.5, 0.5, 1)

【光照控制】
- Base Step: 0.55
- Base Feather: 0.05
- Shade Step: 0.3
- Shade Feather: 0.05

【高光】
- Specular Color: 白色 (1, 1, 1, 1)
- Specular Size: 0.08
- Specular Intensity: 1.0

【边缘光】
- Rim Color: 白色 (1, 1, 1, 1)
- Rim Power: 3.0
- Rim Intensity: 0.3

【描边】
- Outline Width: 0.006
- Outline Color: 黑色 (0, 0, 0, 1)
```

### Step 1.3: 应用材质并测试
```
1. 将 Mat_ToonBasic 拖到 TestCharacter 上
2. 点击 Play ▶️
3. 观察效果：
   ✅ 球体有明显的明暗分界线（三段色阶）
   ✅ 球体周围有黑色描边
   ✅ 亮部有白色高光点
   ✅ 边缘有轻微的白色光晕（Rim Light）
   ✅ 地面上有球体的阴影
```

### Step 1.4: 参数调试测试
```
保持Play模式，在Inspector中调整参数（实时预览）：

测试1：调整阴影阈值
- Base Step: 0.3 → 0.7
- 观察：阴影区域变化

测试2：调整阴影硬度
- Base Feather: 0 → 0.2
- 观察：硬边 → 软边

测试3：调整描边宽度
- Outline Width: 0.003 → 0.015
- 观察：描边变粗

测试4：调整高光大小
- Specular Size: 0.05 → 0.2
- 观察：高光范围变化
```

### Step 1.5: 截图1 - 基础三渲二效果
```
📸 截图要求：
- 停止Play模式，调整最佳参数
- 点击Play
- Game窗口最大化
- 确保球体清晰可见
- 可以看到明暗分界、描边、高光
- 截图保存为：01_基础三渲二效果.png

💡 截图技巧：
- 使用 Windows 截图工具：Win + Shift + S
- 或者 Unity 右上角 Stats → Screenshot
```

---

## 测试Part 2: 法线外扩描边（5分钟）

### Step 2.1: 验证法线外扩描边
```
当前状态：Mat_ToonBasic已经启用了法线外扩描边
（Outline Pass是Shader内置的）

验证步骤：
1. Play模式下，观察球体边缘
2. 应该看到清晰的黑色轮廓线

测试描边特性：
1. 旋转相机（Scene窗口中拖拽）
2. 观察描边是否始终存在
3. 描边宽度应该相对稳定
```

### Step 2.2: 测试描边宽度调整
```
在Inspector中实时调整：

测试1：极小描边
- Outline Width: 0.001
- 观察：描边几乎看不见

测试2：正常描边
- Outline Width: 0.006
- 观察：描边清晰可见

测试3：粗描边（JOJO风格）
- Outline Width: 0.012
- 观察：描边明显变粗

最终设置：0.006（适中）
```

### Step 2.3: 测试描边颜色
```
尝试不同的描边颜色：

1. 黑色描边（默认）
   - Outline Color: (0, 0, 0, 1)

2. 深蓝色描边（动漫风格）
   - Outline Color: (0.1, 0.1, 0.3, 1)

3. 深棕色描边（美漫风格）
   - Outline Color: (0.2, 0.1, 0.05, 1)

恢复为黑色：(0, 0, 0, 1)
```

### Step 2.4: 截图2 - 法线外扩描边特写
```
📸 截图要求：
- 相机拉近到球体（看清描边细节）
- Outline Width: 0.008（稍粗以便看清）
- 截图保存为：02_法线外扩描边.png

📝 观察要点：
- 描边是否连续
- 描边宽度是否均匀
- 是否有断线（Sphere不应该有）
```

---

## 测试Part 3: 屏幕空间Sobel描边（10分钟）

### Step 3.1: 配置URP Renderer
```
1. 找到 URP Renderer Asset：
   - Project窗口搜索：t:ScriptableRendererData
   - 或者在：Assets/Settings/URP-*-Renderer
   - 双击打开

2. 添加 Render Feature：
   - Inspector底部：Add Renderer Feature
   - 选择：NPR Outline Feature
   - ✅ 应该看到新的Feature面板

如果找不到NPR Outline Feature：
   - 检查Console是否有脚本编译错误
   - 确认 NPR_OutlineFeature.cs 在CustomShaders文件夹
   - 重启Unity
```

### Step 3.2: 配置Sobel描边参数
```
在 NPR Outline Feature 面板中：

【Outline Settings】
- Outline Color: 黑色 (0, 0, 0, 1)
- Outline Thickness: 1.5

【Edge Detection】
- Depth Threshold: 0.1
- Normal Threshold: 0.4
- ✅ Use Depth: 勾选
- ✅ Use Normals: 勾选

【Advanced】
- Render Pass Event: Before Rendering Post Processing
```

### Step 3.3: 对比两种描边
```
测试场景：TestCharacter (球体) + Ground (地面)

测试1：只有法线外扩描边
- 禁用NPR Outline Feature（取消勾选）
- Play
- 观察：只有球体边缘有描边，球体和地面接触处无描边

测试2：只有Sobel描边
- 启用NPR Outline Feature
- 将Mat_ToonBasic的Outline Width设为0（关闭法线外扩）
- Play
- 观察：球体边缘和接触处都有描边，但可能更粗

测试3：双描边叠加
- 启用NPR Outline Feature
- Outline Width恢复为0.006
- Play
- 观察：描边更加完整和浓重

📝 记录观察结果，用于报告对比分析
```

### Step 3.4: 调整Sobel阈值
```
保持Play模式，调整参数：

测试Depth Threshold：
- 0.05：边缘检测非常敏感，可能有噪点
- 0.1：标准设置，边缘清晰
- 0.2：只检测大的深度变化

测试Normal Threshold：
- 0.2：检测细微的法线变化
- 0.4：标准设置
- 0.6：只检测明显的法线变化

最佳设置（推荐）：
- Depth Threshold: 0.08-0.12
- Normal Threshold: 0.35-0.45
```

### Step 3.5: 截图3 - 对比图（重要！）
```
📸 需要截取3张对比图：

图A：只有法线外扩描边
- NPR Outline Feature: 禁用
- Outline Width: 0.006
- 截图保存：03A_法线外扩描边.png

图B：只有Sobel描边
- NPR Outline Feature: 启用
- Outline Width: 0
- 截图保存：03B_Sobel描边.png

图C：双描边叠加
- NPR Outline Feature: 启用
- Outline Width: 0.006
- 截图保存：03C_双描边叠加.png

💡 报告用途：
这三张图并排放置，展示两种描边方法的差异
是"算法对比分析"的核心采分点！
```

---

## 测试Part 4: Debug可视化系统（10分钟）

### Step 4.1: 添加Debug Render Feature
```
1. 返回 URP Renderer Asset
2. Add Renderer Feature → NPR Debug Visualization
3. ✅ 确认添加成功
```

### Step 4.2: 配置Debug窗口
```
在 NPR Debug Visualization 面板中：

【Debug Windows】
- ☑ Show Depth Buffer
- ☑ Show Normals Buffer
- ☑ Show Sobel Edges
- ☐ Show Base Color（暂时不勾选）

【Window Layout】
- Window Size: 0.25
- Position: Top Right

【Depth Visualization】
- Depth Scale: 1.0
- ✅ Linearize Depth

【Normal Visualization】
- ✅ World Space Normals
```

### Step 4.3: 测试深度缓冲可视化
```
1. 禁用其他Debug窗口，只启用：
   - ☑ Show Depth Buffer
   - ☐ 其他全部取消

2. 点击Play

3. 观察深度窗口（右上角）：
   ✅ 球体应该显示为蓝色/绿色（近处）
   ✅ 地面应该显示为绿色/黄色
   ✅ 背景Cube应该显示为黄色/红色（远处）
   ✅ 颜色从蓝→绿→黄→红渐变

4. 测试Depth Scale：
   - 调整1.0 → 3.0 → 10.0
   - 观察颜色范围变化

5. 截图4A - 深度缓冲
   📸 保存为：04A_深度缓冲可视化.png
```

### Step 4.4: 测试法线缓冲可视化
```
1. 切换窗口：
   - ☐ Show Depth Buffer
   - ☑ Show Normals Buffer
   - ☐ 其他取消

2. 点击Play（或已在Play模式）

3. 观察法线窗口：
   ✅ 球体不同面朝向不同，颜色丰富
   ✅ 顶部（朝上）应该偏绿色（+Y）
   ✅ 朝向相机的面应该偏蓝色（+Z）
   ✅ 边缘颜色渐变平滑

4. 旋转相机观察法线变化：
   - Scene窗口中按住Alt+鼠标左键旋转
   - Game窗口中观察法线窗口颜色实时变化

5. 截图4B - 法线缓冲
   📸 保存为：04B_法线缓冲可视化.png
```

### Step 4.5: 测试Sobel边缘检测可视化
```
1. 切换窗口：
   - ☐ Show Depth Buffer
   - ☐ Show Normals Buffer
   - ☑ Show Sobel Edges
   - ☐ Show Base Color

2. 点击Play

3. 观察Sobel窗口：
   ✅ 红色线条：深度边缘（轮廓）
   ✅ 绿色线条：法线边缘（表面折痕）
   ✅ 黄色区域：深度+法线重叠

4. 分析边缘类型：
   - 球体外轮廓：主要是红色（深度突变）
   - 球体表面：应该较少（Sphere是平滑的）
   - 球体与地面接触：黄色（深度+法线都变化）

5. 调整NPR Outline Feature的阈值：
   - Depth Threshold: 0.05 → 0.15
   - 观察Sobel窗口红色边缘变化
   - Normal Threshold: 0.2 → 0.6
   - 观察绿色边缘变化

6. 截图4C - Sobel边缘检测（重要！）
   📸 保存为：04C_Sobel边缘检测可视化.png
   
   📝 标注建议：
   - 用图像编辑器标注：红色=深度，绿色=法线
   - 这是报告核心技术图！
```

### Step 4.6: 四窗口完整对比（最重要！）
```
1. 启用所有窗口：
   - ☑ Show Depth Buffer
   - ☑ Show Normals Buffer
   - ☑ Show Sobel Edges
   - ☑ Show Base Color

2. 调整窗口大小：
   - Window Size: 0.22（稍小，避免遮挡太多）

3. 点击Play

4. 调整相机角度，找到最佳展示视角：
   - 球体清晰可见
   - 阴影明显
   - 描边清晰
   - 四个窗口都不遮挡重要内容

5. 截图4D - 四窗口完整对比（最重要！）
   📸 保存为：04D_四窗口完整对比.png
   
   💡 这是报告最佳技术展示图！
   用于第三章"系统架构"或第四章"渲染管线数据流"
   
   📝 报告图注建议：
   图X-X 渲染管线数据流可视化。
   左上：基础颜色（原始渲染）
   右上：深度缓冲（蓝=近，红=远）
   左下：法线缓冲（RGB=XYZ）
   右下：Sobel边缘检测（红=深度边缘，绿=法线边缘）
```

---

## 测试Part 5: 快速预设测试（5分钟）

### Step 5.1: 测试普通漫画预设
```
1. 停止Play
2. 选中 Mat_ToonBasic
3. Inspector底部点击：普通漫画风格
4. 观察参数自动调整
5. Play查看效果
6. 截图5A：05A_普通漫画风格.png
```

### Step 5.2: 测试JOJO黑白预设
```
1. 停止Play
2. Inspector底部点击：JOJO黑白风格
3. 观察参数变化：
   - 对比度更高
   - 描边更粗
   - 边缘光更强
4. Play查看效果
5. 截图5B：05B_JOJO黑白风格.png
```

### Step 5.3: 测试美漫网点预设
```
1. 停止Play
2. Inspector底部点击：美漫网点风格
3. 观察参数：
   - 暖色调
   - 硬边阴影
   - 粗描边
4. Play查看效果
5. 截图5C：05C_美漫网点风格.png

📝 注意：
完整的网点效果需要Halftone后处理（L2功能）
当前只是基础色调和参数设置
```

### Step 5.4: 三种风格并排对比
```
📸 制作对比图（在图像编辑器中）：
- 将5A、5B、5C三张图横向拼接
- 下方标注：普通漫画 | JOJO黑白 | 美漫网点
- 保存为：05D_三种风格对比.png

💡 报告用途：
展示预设系统和多风格切换能力
体现"创新性"评分项
```

---

## 测试Part 6: 最终验证（5分钟）

### Step 6.1: 功能检查清单
```
✅ 核心功能：
   □ 三段色阶光照正常
   □ 高光显示正常
   □ 边缘光显示正常
   □ 阴影投射正常

✅ 描边功能：
   □ 法线外扩描边正常
   □ 屏幕空间Sobel描边正常
   □ 两种描边可独立开关

✅ Debug可视化：
   □ 深度缓冲显示正常
   □ 法线缓冲显示正常
   □ Sobel边缘检测显示正常
   □ 四窗口同时显示正常

✅ 快速预设：
   □ 普通漫画预设正常
   □ JOJO黑白预设正常
   □ 美漫网点预设正常

✅ 用户界面：
   □ ShaderGUI分组显示正常
   □ 参数可实时调整
   □ 快速预设按钮可用
```

### Step 6.2: 性能测试
```
1. 打开Stats面板：
   - Game窗口右上角 Stats按钮

2. 记录基准FPS：
   - 只有Toon Shader，无Debug窗口
   - 记录FPS：_______

3. 记录Debug开销：
   - 开启全部4个Debug窗口
   - 记录FPS：_______
   - 计算开销：_______ ms

📝 报告数据：
记录在第四章"性能分析"中
```

### Step 6.3: 问题记录
```
如果测试中遇到任何问题，记录：

问题1：___________________________
现象：___________________________
解决方案：_______________________

问题2：___________________________
...

📝 用于报告第三章"问题解决记录"
```

---

## 截图整理与标注（额外10分钟）

### 必需截图清单（10张）

```
✅ 01_基础三渲二效果.png
✅ 02_法线外扩描边.png
✅ 03A_法线外扩描边.png
✅ 03B_Sobel描边.png
✅ 03C_双描边叠加.png
✅ 04A_深度缓冲可视化.png
✅ 04B_法线缓冲可视化.png
✅ 04C_Sobel边缘检测可视化.png
✅ 04D_四窗口完整对比.png         ⭐最重要
✅ 05D_三种风格对比.png

可选截图（加分项）：
□ 不同光照角度对比
□ 不同阴影阈值对比
□ Inspector界面截图
□ 运行时参数调整GIF
```

### 图片后期处理建议

使用图像编辑软件（Photoshop / GIMP / Paint.NET）：

```
1. 调整亮度/对比度（如果需要）
2. 添加文字标注：
   - 04C Sobel图：标注"红色=深度边缘，绿色=法线边缘"
   - 04D 四窗口图：标注A/B/C/D区域
   - 对比图：标注不同方法的名称

3. 统一分辨率：
   - 建议1920x1080或1280x720
   - 确保图片清晰

4. 文件命名规范：
   - 按顺序编号
   - 名称清晰易懂
   - 便于报告中引用
```

---

## 测试报告模板

完成测试后，填写以下测试报告：

```markdown
# NPR Toon Shader 测试报告

## 测试环境
- Unity版本：2022.3.x
- URP版本：14.0.x
- 测试日期：2026-06-18
- 测试时长：40分钟

## 测试结果总结

### 1. 基础Toon Shader
- 状态：✅ 通过
- 三段色阶：正常
- 高光：正常
- 边缘光：正常
- 阴影投射：正常
- 问题：无

### 2. 法线外扩描边
- 状态：✅ 通过
- 描边连续性：良好
- 宽度调整：正常
- 颜色调整：正常
- 问题：[如有问题记录在此]

### 3. 屏幕空间Sobel描边
- 状态：✅ 通过
- 深度检测：正常
- 法线检测：正常
- 阈值调整：响应正常
- 问题：[如有问题记录在此]

### 4. Debug可视化系统
- 状态：✅ 通过
- 深度缓冲：显示正常
- 法线缓冲：显示正常
- Sobel可视化：显示正常
- 四窗口模式：正常
- 问题：[如有问题记录在此]

### 5. 快速预设系统
- 状态：✅ 通过
- 普通漫画：正常
- JOJO黑白：正常
- 美漫网点：正常
- 问题：无

## 性能数据
- 基准FPS（无Debug）：____
- Debug开销FPS（4窗口）：____
- 性能影响：约 ____ ms

## 发现的问题
1. [问题描述]
   - 解决方案：[...]
2. [问题描述]
   - 解决方案：[...]

## 改进建议
1. [...]
2. [...]

## 结论
[总体评价，是否满足项目要求]
```

---

## 测试完成后的下一步

### 立即操作
1. ✅ 整理所有截图
2. ✅ 填写测试报告
3. ✅ 备份项目文件

### 本周内
1. 📝 开始撰写技术报告第三章（有了截图就好写）
2. 🎨 优化参数（如果有需要）
3. 🚀 开始L2功能开发（相机控制、运行时UI）

### 下周
1. 📄 完成技术报告
2. 📦 打包Release版本
3. 🎬 准备答辩演示

---

## 常见测试问题快速解决

### Q: 截图时Game窗口太小
**A**: 
1. 双击Game标签，使其最大化
2. 或者设置固定分辨率：Game窗口上方选择1920x1080

### Q: 测试时遇到编译错误
**A**: 
1. 立即停止Play
2. 查看Console完整错误信息
3. 参考TROUBLESHOOTING.md
4. 修复后重新测试

### Q: 找不到Render Feature选项
**A**:
1. 确认没有脚本编译错误
2. 检查C#文件位置（不应在Editor文件夹）
3. 重启Unity
4. 清除脚本缓存

### Q: Debug窗口不显示
**A**:
1. 确认至少勾选一个窗口
2. 确认在Play模式（Editor模式不显示）
3. 检查Window Size不为0
4. 查看Console是否有错误

---

## 完成测试！🎉

恭喜你完成了完整的测试流程！

**你现在拥有**:
- ✅ 10张高质量技术截图
- ✅ 完整的功能验证
- ✅ 详细的测试记录
- ✅ 性能数据
- ✅ 问题解决经验

**这些材料足以支撑**:
- 📝 技术报告第三章（系统设计与实现）
- 📝 技术报告第四章（测试与结果分析）
- 🎓 答辩演示（实时操作展示）
- 💯 高分评价（专业的测试流程）

---

**预计得分提升**: +3-5分（体现了严谨的测试过程）

**下一步建议**: 
开始撰写报告第三章，趁热打铁！所有截图和测试数据都已准备好。

---

*测试是项目开发的重要环节，详细的测试记录体现了工程能力！*
