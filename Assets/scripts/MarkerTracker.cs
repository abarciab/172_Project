using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Marker
{
    public RectTransform UImarker;
    public Transform trackedObj;
}

public class MarkerTracker : MonoBehaviour
{
    public static MarkerTracker i;
    void Awake() { i = this; }

    public List<Marker> activeMarkers = new List<Marker>();

    [SerializeField] private RectTransform markerParent;
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private GameObject leftMarker;
    [SerializeField] private GameObject rightMarker;
    [SerializeField] private Sprite defaultMarker;
    [SerializeField] private bool onlyDefault;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void Update()
    {
        for (int i = 0; i < activeMarkers.Count; i++) {
            if (activeMarkers[i].trackedObj == null || !activeMarkers[i].trackedObj.gameObject.activeInHierarchy) {
                Destroy(activeMarkers[i].UImarker.gameObject);
                activeMarkers.RemoveAt(i);
            }
        }
        foreach (var m in activeMarkers) UpdateMarker(m);
    }


    public bool AlreadyTracking(Transform obj)
    {
        foreach (var m in activeMarkers) if (m.trackedObj == obj) return true;
        return false;
    }

    public void RemoveMarker(Transform obj) {
        if (!AlreadyTracking(obj)) return;

        for (int i = 0; i < activeMarkers.Count; i++) {
            if (activeMarkers[i].trackedObj == obj) {
                Destroy(activeMarkers[i].UImarker.gameObject);
                activeMarkers.RemoveAt(i);
            }
        }
    }

    public void AddMarker(Transform obj, Sprite img)
    {
        if (AlreadyTracking(obj)) return;
        if (onlyDefault) img = defaultMarker;

        GameObject newUImarker = Instantiate(markerPrefab, markerParent);
        newUImarker.transform.GetChild(0).GetComponentInChildren<Image>().sprite = img;
        Marker newMarker = new Marker();
        newMarker.trackedObj = obj;
        newMarker.UImarker = newUImarker.GetComponent<RectTransform>();
        UpdateMarker(newMarker);
        activeMarkers.Add(newMarker);
    }


    void UpdateMarker(Marker m)
    {
        var camForward = Camera.main.transform.forward;
        var dir = m.trackedObj.position - Camera.main.transform.position;
        dir.y = camForward.y = 0;
        float angle = Vector3.SignedAngle(camForward, dir, Camera.main.transform.up);
        float parentMod = 2;
        float x = (markerParent.rect.width * parentMod) * ((angle + 180) / 360);

        Vector3 targetPos = Vector3.right * (x - markerParent.rect.width * parentMod / 2);
        m.UImarker.transform.localPosition = Vector3.Lerp(m.UImarker.transform.localPosition, targetPos, 0.2f);

        m.UImarker.gameObject.SetActive(targetPos.x < markerParent.rect.width / 2 && targetPos.x > -markerParent.rect.width / 2);

        rightMarker.SetActive(false);
        leftMarker.SetActive(false);
        if (targetPos.x > markerParent.rect.width / 2) rightMarker.SetActive(true);
        else if (targetPos.x < -markerParent.rect.width / 2) leftMarker.SetActive(true);
    }
}
