using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerTracker : MonoBehaviour
{
    public static MarkerTracker i;
    void Awake() { i = this; }

    public List<Transform> trackedMarkers;

    [SerializeField] RectTransform markerParent, marker1;

    public Transform test1;

    private void Update()
    {
        var camForward = Camera.main.transform.forward;
        var dir = test1.position - Camera.main.transform.position;
        dir.y = camForward.y = 0;
        float angle = Vector3.SignedAngle(camForward, dir, Camera.main.transform.up);
        float parentMod = 2;
        float x = (markerParent.rect.width * parentMod) * ((angle + 180) / 360);

        Vector3 targetPos = Vector3.right * (x - markerParent.rect.width*parentMod / 2);
        marker1.transform.localPosition = Vector3.Lerp(marker1.transform.localPosition, targetPos, 0.2f);

        marker1.gameObject.SetActive(targetPos.x < markerParent.rect.width / 2 && targetPos.x > -markerParent.rect.width / 2);
    }
}
