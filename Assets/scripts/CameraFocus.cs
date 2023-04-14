using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraFocus : MonoBehaviour
{
    public int ID;
    bool added;
    CameraFocusManager man;

    CameraFocusManager FindManager()
    {
        if (CameraFocusManager.instance != null) return CameraFocusManager.instance;
        else if (FindObjectOfType<CameraFocusManager>() != null) return FindObjectOfType<CameraFocusManager>();
        return null;
    }

    private void Update()
    {
        if (!man) man = FindManager();
        added = man.cameraFocuses.Contains(this);

        if (added) return;
        added = true;
        if (man == null) added = false;
        else if (man.cameraFocuses.Contains(this)) return;
        else man.cameraFocuses.Add(this);
    }

    void RemoveFromManager()
    {
        var manager = FindManager();
        if (manager) manager.cameraFocuses.Remove(this);
    }

    private void OnDisable()
    {
        RemoveFromManager();
    }

    private void OnDestroy()
    {
        RemoveFromManager();
    }
}
