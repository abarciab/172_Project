using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SingleButtonSelector : MonoBehaviour
{
    List<SelectableItem> _buttons = new List<SelectableItem>();

    private void Start()
    {
        _buttons = GetComponentsInChildren<SelectableItem>().ToList();
        foreach (var b in _buttons) b.OnSelect.AddListener(() => DeselectOthers(b));
    }

    private void DeselectOthers(SelectableItem selected)
    {
        foreach (var b in _buttons) if (selected != b) b.Deselect();
    }
}
