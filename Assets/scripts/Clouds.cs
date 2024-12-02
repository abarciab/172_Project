using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class Clouds : MonoBehaviour
{
    [SerializeField] Vector2 Bounds;
    [SerializeField] bool debug;
    [SerializeField] List<GameObject> prefabs = new List<GameObject>();
    [SerializeField] Vector2 yRange = new Vector2(0, 20), scaleRange = new Vector2(0.1f, 2), scaleRange2 = new Vector2(-1, 1), lifeTimeRange = new Vector2(10, 100);
    [SerializeField] Vector3 speed1, speed2;
    public int maxClounds = 300;
    List<GameObject> clouds = new List<GameObject>();
    [SerializeField] float clumpingChance, clumpingDist = 2;
    Vector3 lastPos;

    private void Start()
    {
        lastPos = transform.position;
    }

    private void Update()
    {
        if (prefabs.Count == 0) return;

        CheckClouds();
        if (clouds.Count < maxClounds) {
            SpawnCloud();
        }
    }

    void CheckClouds()
    {
        for (int i = 0; i < clouds.Count; i++) {
            if (!clouds[i]) clouds.RemoveAt(i);
        }
    }

    void SpawnCloud()
    {
        var pos = GetValidPos();
        var xScale = Random.Range(scaleRange.x, scaleRange.y);

        var scale = new Vector3(xScale, Random.Range(scaleRange.x, scaleRange.y)/2, xScale + Random.Range(scaleRange2.x, scaleRange2.y));
        var rot = new Vector3(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));
        var newCloud = Instantiate(prefabs[Random.Range(0, prefabs.Count)], pos, Quaternion.Euler(rot));

        newCloud.transform.localScale = scale;
        newCloud.GetComponent<Cloud>().lifeTime = Random.Range(lifeTimeRange.x, lifeTimeRange.y);
        newCloud.GetComponent<Cloud>().speed = new Vector3(Random.Range(speed1.x, speed2.x), Random.Range(speed1.y, speed2.y), Random.Range(speed1.z, speed2.z));
        newCloud.transform.parent = transform;

        clouds.Add(newCloud);
    }

    Vector3 GetValidPos()
    {
        var pos = new Vector3(transform.position.x + Random.Range(-Bounds.x, Bounds.x), transform.position.y + Random.Range(yRange.x, yRange.y), transform.position.z + Random.Range(-Bounds.y, Bounds.y));
        if (Random.Range(0, 1.0f) < clumpingChance) pos = lastPos + new Vector3(Random.Range(-clumpingDist, clumpingDist), Random.Range(-clumpingDist, clumpingDist), Random.Range(-clumpingDist, clumpingDist));
        lastPos = pos;
        return pos;
    }

    private void OnDrawGizmosSelected()
    {
        DrawBoxOutline(transform.position + Vector3.down * yRange.x, Bounds * 2, yRange.y);
    }

    private void DrawBoxOutline(Vector3 center, Vector2 size, float height)
    {
        // Calculate half extents
        Vector3 halfExtents = new Vector3(size.x / 2f, height / 2f, size.y / 2f);

        // Calculate box corners
        Vector3 frontBottomLeft = center + new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
        Vector3 frontBottomRight = center + new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
        Vector3 frontTopLeft = center + new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
        Vector3 frontTopRight = center + new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
        Vector3 backBottomLeft = center + new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
        Vector3 backBottomRight = center + new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
        Vector3 backTopLeft = center + new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);
        Vector3 backTopRight = center + new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);

        // Draw lines between corners
        Gizmos.color = Color.green; // Set the color of the lines
        Gizmos.DrawLine(frontBottomLeft, frontBottomRight);
        Gizmos.DrawLine(frontBottomRight, frontTopRight);
        Gizmos.DrawLine(frontTopRight, frontTopLeft);
        Gizmos.DrawLine(frontTopLeft, frontBottomLeft);

        Gizmos.DrawLine(backBottomLeft, backBottomRight);
        Gizmos.DrawLine(backBottomRight, backTopRight);
        Gizmos.DrawLine(backTopRight, backTopLeft);
        Gizmos.DrawLine(backTopLeft, backBottomLeft);

        Gizmos.DrawLine(frontBottomLeft, backBottomLeft);
        Gizmos.DrawLine(frontBottomRight, backBottomRight);
        Gizmos.DrawLine(frontTopLeft, backTopLeft);
        Gizmos.DrawLine(frontTopRight, backTopRight);
    }

}
