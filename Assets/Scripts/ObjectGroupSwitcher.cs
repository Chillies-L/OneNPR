using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ObjectGroupSwitcher : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Button[] buttons = new Button[4];
    [SerializeField] private GameObject[] groups = new GameObject[4];

    [Header("Startup")]
    [SerializeField] private int defaultGroupIndex;
    [SerializeField] private bool applyDefaultOnAwake = true;
    [SerializeField] private bool warnOnInvalidSelection = true;

    private UnityAction[] buttonHandlers;

    public int ActiveIndex { get; private set; } = -1;

    private void Awake()
    {
        BindButtons();

        if (applyDefaultOnAwake && HasValidGroupAt(defaultGroupIndex))
        {
            ShowGroup(defaultGroupIndex);
        }
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void OnValidate()
    {
        if (defaultGroupIndex < 0)
        {
            defaultGroupIndex = 0;
        }

        if (groups != null && groups.Length > 0 && defaultGroupIndex >= groups.Length)
        {
            defaultGroupIndex = groups.Length - 1;
        }
    }

    public void ConfigureForRuntime(Button[] runtimeButtons, GameObject[] runtimeGroups, int initialIndex = 0, bool showInitialGroup = true)
    {
        UnbindButtons();

        buttons = runtimeButtons;
        groups = runtimeGroups;
        defaultGroupIndex = initialIndex;

        BindButtons();

        if (showInitialGroup)
        {
            ShowGroup(initialIndex);
        }
    }

    public void ShowGroup(int index)
    {
        if (!HasValidGroupAt(index))
        {
            if (warnOnInvalidSelection)
            {
                Debug.LogWarning($"Cannot show group {index}: assign a valid GameObject at that index.", this);
            }

            return;
        }

        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] != null)
            {
                groups[i].SetActive(i == index);
            }
        }

        ActiveIndex = index;
    }

    public void ShowFirstGroup()
    {
        ShowGroup(0);
    }

    public void ShowSecondGroup()
    {
        ShowGroup(1);
    }

    public void ShowThirdGroup()
    {
        ShowGroup(2);
    }

    public void ShowFourthGroup()
    {
        ShowGroup(3);
    }

    private void BindButtons()
    {
        if (buttons == null)
        {
            buttonHandlers = null;
            return;
        }

        buttonHandlers = new UnityAction[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            int groupIndex = i;
            buttonHandlers[i] = () => ShowGroup(groupIndex);
            button.onClick.AddListener(buttonHandlers[i]);
        }
    }

    private void UnbindButtons()
    {
        if (buttons == null || buttonHandlers == null)
        {
            return;
        }

        int count = Mathf.Min(buttons.Length, buttonHandlers.Length);
        for (int i = 0; i < count; i++)
        {
            if (buttons[i] != null && buttonHandlers[i] != null)
            {
                buttons[i].onClick.RemoveListener(buttonHandlers[i]);
            }
        }

        buttonHandlers = null;
    }

    private bool HasValidGroupAt(int index)
    {
        return groups != null && index >= 0 && index < groups.Length && groups[index] != null;
    }
}
