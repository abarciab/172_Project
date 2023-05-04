using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trackable : MonoBehaviour
{
    [SerializeField] List<Fact> factCondition = new List<Fact>();
    [SerializeField] Fact unless;

    [SerializeField] bool track;
    [SerializeField] Sprite trackerIcon;

    private void Update()
    {
        bool present = false;
        foreach (var f in factCondition) if (FactManager.i.IsPresent(f)) present = true;
        if (factCondition.Count > 0) track = present && (unless == null || !FactManager.i.IsPresent(unless));

        if (track && !MarkerTracker.i.AlreadyTracking(transform)) MarkerTracker.i.AddMarker(transform, trackerIcon);
        if (MarkerTracker.i.AlreadyTracking(transform) && !track) MarkerTracker.i.RemoveMarker(transform);
    }
}
