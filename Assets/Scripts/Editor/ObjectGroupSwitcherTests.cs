using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ObjectGroupSwitcherTests
{
    [Test]
    public void ShowGroupActivatesOnlySelectedGroup()
    {
        GameObject host = new GameObject("Object Group Switcher");
        ObjectGroupSwitcher switcher = host.AddComponent<ObjectGroupSwitcher>();
        GameObject[] groups = CreateGroups(4);
        switcher.ConfigureForRuntime(null, groups, 0, false);

        switcher.ShowGroup(2);

        Assert.That(groups[0].activeSelf, Is.False);
        Assert.That(groups[1].activeSelf, Is.False);
        Assert.That(groups[2].activeSelf, Is.True);
        Assert.That(groups[3].activeSelf, Is.False);
        Assert.That(switcher.ActiveIndex, Is.EqualTo(2));

        DestroyObjects(groups);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void ClickingButtonActivatesMatchingGroup()
    {
        GameObject host = new GameObject("Object Group Switcher");
        ObjectGroupSwitcher switcher = host.AddComponent<ObjectGroupSwitcher>();
        Button[] buttons = CreateButtons(4);
        GameObject[] groups = CreateGroups(4);
        switcher.ConfigureForRuntime(buttons, groups, 0, false);

        buttons[3].onClick.Invoke();

        Assert.That(groups[0].activeSelf, Is.False);
        Assert.That(groups[1].activeSelf, Is.False);
        Assert.That(groups[2].activeSelf, Is.False);
        Assert.That(groups[3].activeSelf, Is.True);
        Assert.That(switcher.ActiveIndex, Is.EqualTo(3));

        DestroyButtons(buttons);
        DestroyObjects(groups);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void InvalidIndexDoesNotChangeCurrentSelection()
    {
        GameObject host = new GameObject("Object Group Switcher");
        ObjectGroupSwitcher switcher = host.AddComponent<ObjectGroupSwitcher>();
        GameObject[] groups = CreateGroups(4);
        switcher.ConfigureForRuntime(null, groups, 1, true);

        LogAssert.Expect(LogType.Warning, "Cannot show group 9: assign a valid GameObject at that index.");
        switcher.ShowGroup(9);

        Assert.That(groups[0].activeSelf, Is.False);
        Assert.That(groups[1].activeSelf, Is.True);
        Assert.That(groups[2].activeSelf, Is.False);
        Assert.That(groups[3].activeSelf, Is.False);
        Assert.That(switcher.ActiveIndex, Is.EqualTo(1));

        DestroyObjects(groups);
        Object.DestroyImmediate(host);
    }

    private static GameObject[] CreateGroups(int count)
    {
        GameObject[] groups = new GameObject[count];
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i] = new GameObject($"Group {i + 1}");
        }

        return groups;
    }

    private static Button[] CreateButtons(int count)
    {
        Button[] buttons = new Button[count];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = new GameObject($"Button {i + 1}").AddComponent<Button>();
        }

        return buttons;
    }

    private static void DestroyObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            Object.DestroyImmediate(obj);
        }
    }

    private static void DestroyButtons(Button[] buttons)
    {
        foreach (Button button in buttons)
        {
            Object.DestroyImmediate(button.gameObject);
        }
    }
}
