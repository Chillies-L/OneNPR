# Runtime UI 快速配置

本文对应 `NPRRuntimeUIPanel.cs`，用于在 Play Mode（播放模式）中点击选择场景对象，并实时编辑其 NPR shader（着色器）材质参数、切换 NPR style preset（风格预设）、新建运行时 Material（材质），以及显示 FPS（Frames Per Second，帧率）。

## 当前 SampleScene 配置

`Assets/Scenes/SampleScene.unity` 已配置一个 `Runtime UI Canvas`（运行时画布）和 `NPR Runtime UI` 物体。`NPR Runtime UI` 上挂载 `NPRRuntimeUIPanel`，并已接入场景中的可编辑 `Renderer`（渲染器）、`Main Camera`（主摄像机）和两个 preset：

- `Assets/CustomShaders/Presets/BoldInkComic.asset`
- `Assets/CustomShaders/Presets/GraphicActionAnime.asset`

运行场景后，左上角会生成面板。按 `F1` 可显示或隐藏面板。用鼠标左键点击带 Collider（碰撞体）的模型后，面板的 `Selected` 会显示当前对象名；之后滑杆和 preset 默认只作用于当前选中的 Renderer。

## 手动配置方法

1. 在场景中创建或选择一个 `Canvas`（画布），确保它有 `Canvas Scaler`（画布缩放器）和 `Graphic Raycaster`（图形射线检测器）。如果没有 `EventSystem`（事件系统），需要补一个。
2. 在 Canvas 下创建空物体并添加 `NPRRuntimeUIPanel` 组件。
3. 将需要运行时编辑的模型 `Renderer` 拖到 `Target Renderers`。推荐保持 `Use Runtime Material Instances` 勾选，这样 Play Mode 中编辑的是运行时材质实例，不会直接污染项目里的材质资产。
4. 勾选 `Edit Selected Renderer Only`。这样点击选择对象后，`Next Preset`、preset 按钮和材质滑杆只会修改选中的 Renderer；没有选中对象时才会修改 `Target Renderers` 列表中的全部目标。
5. 将 `Main Camera` 拖到 `Selection Camera`。被点击对象需要有 `Collider`，否则射线检测（Raycast）无法命中。普通 MeshRenderer（网格渲染器）可以用 `BoxCollider` 或 `MeshCollider`；SkinnedMeshRenderer（蒙皮网格渲染器）建议用 `BoxCollider`。
6. 将 preset 资产拖到 `Style Presets`。如果要控制 halftone（网点）和 Post FX（后处理），还要把 URP Renderer（通用渲染管线渲染器）里的 `NPRStylePostProcessFeature` 引用到 `Style Post Process Feature`。

## 面板控件

| UI 控件 | 功能 |
|---|---|
| Selected | 显示当前点击选中的对象 |
| Next Preset | 切换到下一个 `NPRStylePreset` 并应用到当前编辑目标 |
| Preset 按钮 | 直接应用对应 preset |
| New Material | 为当前选中 Renderer 克隆一个运行时 NPR Material，并把后续编辑写到新材质 |
| Base Shadow | 写入 `_BaseStep` |
| Shadow Feather | 写入 `_BaseFeather` 与 `_ShadeFeather` |
| Outline Width | 写入 `_OutlineWidth` |
| Rim Intensity | 写入 `_RimIntensity` |
| Specular | 写入 `_SpecularIntensity` |
| Halftone | 写入 `NPRStylePostProcessFeature.settings.halftoneStrength` |
| Post FX | 修改 `NPRStylePostProcessFeature.settings.enabled` 与 Renderer Feature active 状态 |

## 使用注意

`New Material` 只在运行时创建内存中的材质实例，不会自动保存成 `.mat` 资产。这个设计适合课堂演示和参数调试：你可以在 Play Mode 中快速比较对象效果；如果要把某个结果固化为项目资产，需要退出运行后手动创建或保存材质。

如果点击 UI 时同时选中了后面的模型，检查 `Ignore Clicks Over UI` 是否勾选，并确认场景中只有一个可用 `EventSystem`。
