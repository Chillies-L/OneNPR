using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Runtime UI controller for NPR Debug Visualization
/// NPR调试可视化的运行时UI控制器
///
/// Usage: Attach to a Canvas GameObject
/// 用法：附加到Canvas游戏对象上
/// </summary>
public class NPR_DebugUIController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your URP Renderer Asset here")]
    public UnityEngine.Rendering.Universal.ScriptableRendererData rendererData;

    [Header("UI Layout")]
    public float buttonWidth = 200f;
    public float buttonHeight = 40f;
    public float spacing = 10f;
    public Vector2 panelPosition = new Vector2(10, 10);

    [Header("UI Styling")]
    public Color enabledColor = new Color(0.3f, 0.8f, 0.3f, 0.8f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    public int fontSize = 16;

    private NPR_DebugVisualization debugFeature;
    private Dictionary<string, Toggle> toggleButtons = new Dictionary<string, Toggle>();
    private GameObject uiPanel;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeUI();
        FindDebugFeature();
    }

    /// <summary>
    /// Find NPR_DebugVisualization feature in the renderer
    /// 在渲染器中查找NPR_DebugVisualization功能
    /// </summary>
    private void FindDebugFeature()
    {
        if (rendererData == null)
        {
            Debug.LogError("NPR_DebugUIController: Renderer Data is not assigned!");
            return;
        }

        // This will be set up when the feature is found
        // 当找到功能时将设置此项
        Debug.Log("NPR_DebugUIController: Looking for NPR_DebugVisualization feature...");
    }

    /// <summary>
    /// Create UI panel and buttons
    /// 创建UI面板和按钮
    /// </summary>
    private void InitializeUI()
    {
        // Create main panel
        // 创建主面板
        uiPanel = new GameObject("NPR_DebugPanel");
        uiPanel.transform.SetParent(transform, false);

        RectTransform panelRect = uiPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = panelPosition;

        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create vertical layout group
        // 创建垂直布局组
        VerticalLayoutGroup layoutGroup = uiPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.spacing = spacing;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Create title
        // 创建标题
        CreateLabel("NPR Debug Visualization", 18, FontStyle.Bold);

        // Create toggle buttons
        // 创建切换按钮
        CreateToggleButton("showDepthBuffer", "深度缓冲 (Depth Buffer)", Color.cyan);
        CreateToggleButton("showNormalsBuffer", "法线缓冲 (Normals Buffer)", Color.green);
        CreateToggleButton("showSobelEdges", "Sobel边缘 (Sobel Edges)", Color.yellow);
        CreateToggleButton("showBaseColor", "基础颜色 (Base Color)", Color.white);

        // Add separator
        // 添加分隔符
        CreateSeparator();

        // Window size slider
        // 窗口大小滑块
        CreateSlider("Window Size", 0.1f, 0.5f, 0.25f, OnWindowSizeChanged);

        // Position dropdown
        // 位置下拉菜单
        CreateDropdown("Position", new List<string> { "Top Right", "Top Left", "Bottom Right", "Bottom Left" }, OnPositionChanged);

        // Add close button
        // 添加关闭按钮
        CreateButton("关闭面板 (Hide Panel)", OnHidePanel, new Color(0.8f, 0.3f, 0.3f, 0.8f));

        // Adjust panel size
        // 调整面板大小
        panelRect.sizeDelta = new Vector2(buttonWidth + 20, 0);

        isInitialized = true;
        Debug.Log("NPR_DebugUIController: UI initialized successfully");
    }

    /// <summary>
    /// Create a label text
    /// 创建标签文本
    /// </summary>
    private void CreateLabel(string text, int size, FontStyle style)
    {
        GameObject labelObj = new GameObject("Label_" + text);
        labelObj.transform.SetParent(uiPanel.transform, false);

        Text label = labelObj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = size;
        label.fontStyle = style;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = labelObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(buttonWidth, 30);
    }

    /// <summary>
    /// Create a separator line
    /// 创建分隔线
    /// </summary>
    private void CreateSeparator()
    {
        GameObject separatorObj = new GameObject("Separator");
        separatorObj.transform.SetParent(uiPanel.transform, false);

        Image separator = separatorObj.AddComponent<Image>();
        separator.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        RectTransform rect = separatorObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(buttonWidth, 2);
    }

    /// <summary>
    /// Create a toggle button
    /// 创建切换按钮
    /// </summary>
    private void CreateToggleButton(string id, string labelText, Color indicatorColor)
    {
        GameObject toggleObj = new GameObject("Toggle_" + id);
        toggleObj.transform.SetParent(uiPanel.transform, false);

        // Add toggle component
        // 添加切换组件
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = false;

        // Background
        // 背景
        Image bgImage = toggleObj.AddComponent<Image>();
        bgImage.color = disabledColor;

        // Checkmark (indicator)
        // 勾选标记（指示器）
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(toggleObj.transform, false);
        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = indicatorColor;

        RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0, 0.5f);
        checkmarkRect.pivot = new Vector2(0, 0.5f);
        checkmarkRect.anchoredPosition = new Vector2(10, 0);
        checkmarkRect.sizeDelta = new Vector2(20, 20);

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkmarkImage;

        // Label
        // 标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(40, 0);
        labelRect.offsetMax = new Vector2(-10, 0);

        // Set toggle size
        // 设置切换按钮大小
        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

        // Add listener
        // 添加监听器
        toggle.onValueChanged.AddListener((isOn) => OnToggleChanged(id, isOn, bgImage));

        toggleButtons[id] = toggle;
    }

    /// <summary>
    /// Create a button
    /// 创建按钮
    /// </summary>
    private void CreateButton(string labelText, UnityEngine.Events.UnityAction onClick, Color color)
    {
        GameObject buttonObj = new GameObject("Button_" + labelText);
        buttonObj.transform.SetParent(uiPanel.transform, false);

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        button.targetGraphic = buttonImage;
        button.onClick.AddListener(onClick);

        // Label
        // 标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
    }

    /// <summary>
    /// Create a slider
    /// 创建滑块
    /// </summary>
    private void CreateSlider(string labelText, float minValue, float maxValue, float defaultValue, UnityEngine.Events.UnityAction<float> onValueChanged)
    {
        GameObject sliderObj = new GameObject("Slider_" + labelText);
        sliderObj.transform.SetParent(uiPanel.transform, false);

        // Background
        // 背景
        Image bgImage = sliderObj.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = defaultValue;
        slider.onValueChanged.AddListener(onValueChanged);

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(buttonWidth, 30);

        // Add label
        // 添加标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize - 2;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.offsetMin = new Vector2(5, 0);
        labelRect.offsetMax = new Vector2(-5, 0);
    }

    /// <summary>
    /// Create a dropdown
    /// 创建下拉菜单
    /// </summary>
    private void CreateDropdown(string labelText, List<string> options, UnityEngine.Events.UnityAction<int> onValueChanged)
    {
        GameObject dropdownObj = new GameObject("Dropdown_" + labelText);
        dropdownObj.transform.SetParent(uiPanel.transform, false);

        Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
        Image dropdownImage = dropdownObj.AddComponent<Image>();
        dropdownImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        dropdown.options.Clear();
        foreach (string option in options)
        {
            dropdown.options.Add(new Dropdown.OptionData(option));
        }

        dropdown.onValueChanged.AddListener(onValueChanged);

        RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
        dropdownRect.sizeDelta = new Vector2(buttonWidth, 30);
    }

    /// <summary>
    /// Toggle changed callback
    /// 切换按钮改变回调
    /// </summary>
    private void OnToggleChanged(string id, bool isOn, Image bgImage)
    {
        bgImage.color = isOn ? enabledColor : disabledColor;
        Debug.Log($"NPR_DebugUIController: {id} = {isOn}");

        // Here you would update the NPR_DebugVisualization.settings
        // 这里你需要更新NPR_DebugVisualization.settings
        // This requires accessing the render feature at runtime
        // 这需要在运行时访问渲染功能
    }

    private void OnWindowSizeChanged(float value)
    {
        Debug.Log($"NPR_DebugUIController: Window Size = {value}");
    }

    private void OnPositionChanged(int index)
    {
        Debug.Log($"NPR_DebugUIController: Position = {index}");
    }

    private void OnHidePanel()
    {
        uiPanel.SetActive(false);
    }

    /// <summary>
    /// Show panel (call from external script or key press)
    /// 显示面板（从外部脚本或按键调用）
    /// </summary>
    public void ShowPanel()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Toggle panel visibility
    /// 切换面板可见性
    /// </summary>
    public void TogglePanel()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(!uiPanel.activeSelf);
        }
    }

    private void Update()
    {
        // Press F1 to toggle debug panel
        // 按F1切换调试面板
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TogglePanel();
        }
    }
}
