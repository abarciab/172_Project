using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraFocusManager : MonoBehaviour
{
    public static CameraFocusManager instance;
    private void Awake() { instance = this; }
    [SerializeField] bool showDebug;
    [SerializeField] Color gizmoColor = Color.white;

    public List<CameraFocus> cameraFocuses = new List<CameraFocus>();

    public GameObject GetFocus(int ID)
    {
        foreach (var c in cameraFocuses) if (c.ID == ID) return c.gameObject;
        return gameObject;
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;
        Gizmos.color = gizmoColor;

        foreach (var c in cameraFocuses) {
            var dir = c.transform.position - transform.position;
            Gizmos.DrawRay(transform.position, dir);
        }   
    }
}
