using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trackable : MonoBehaviour
{
    [SerializeField] Fact factCondition, unless;

    [SerializeField] bool track;
    [SerializeField] Sprite trackerIcon;

    private void Update()
    {
        if (factCondition != null) track = FactManager.i.IsPresent(factCondition) && (unless == null || !FactManager.i.IsPresent(unless));

        if (track && !MarkerTracker.i.AlreadyTracking(transform)) MarkerTracker.i.AddMarker(transform, trackerIcon);
        if (MarkerTracker.i.AlreadyTracking(transform) && !track) MarkerTracker.i.RemoveMarker(transform);
    }
}
