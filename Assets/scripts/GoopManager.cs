using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GoopManager : MonoBehaviour
{
    public static GoopManager i;
    void Awake() { i = this; }
    [SerializeField] float maxSpread;
    [SerializeField] Vector3 maxScale;
    [SerializeField] GameObject goopPrefab;
    [SerializeField] int maxGlobs;

    [Header("testing")]
    [SerializeField] Vector3 testPos;
    [SerializeField] float testAmount;
    [SerializeField] bool test;

    [Header("sounds")]
    [SerializeField] Sound goopSound;

    private void Start()
    {
        if (Application.isPlaying) goopSound = Instantiate(goopSound);
    }

    private void Update()
    {
        if (test) {
            for (int i = 0; i < transform.childCount; i++) {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }
            test = false;
            SpawnGoop(testPos, testAmount);
        }
    }

    public void ClearAllFloorGoop()
    {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SpawnGoop(Vector3 pos, float amount, float time = -1)
    {
        int layerMask = 1 << 7;
        Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out var hit, 150, layerMask: layerMask);
        if (hit.collider == null) return;
        pos.y = hit.point.y;

        int numGlobs = Mathf.Max(Mathf.RoundToInt(amount * maxGlobs), 3);
        GameObject mainGlob = gameObject;
        for (int i = 0; i < numGlobs; i++) {
            var newGlob = Instantiate(goopPrefab, transform);
            Vector2 circlePoint = Random.insideUnitCircle;
            var _pos = pos + new Vector3(circlePoint.x * (maxSpread * amount), 0, circlePoint.y * (maxSpread * amount));
            newGlob.transform.position = _pos;
            var scaleMod = (1 - (Mathf.Abs(circlePoint.x) + Mathf.Abs(circlePoint.y))/2) + 0.1f;
            newGlob.transform.localScale = maxScale * amount * scaleMod;
            mainGlob = newGlob;
            if (time != -1) newGlob.GetComponent<Goop>().lifeTime = time;
        }
        if (goopSound.instantialized) goopSound.Play(mainGlob.transform);
    }
}
