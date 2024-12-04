using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailPlacer : MonoBehaviour
{
    [SerializeField] private List<GameObject> _prefabs = new List<GameObject>();

    [SerializeField] private Vector2 _limits = new Vector2(10, 10);
    [SerializeField] private Vector2 _sizeLimits = new Vector2(0.8f, 1.1f);
    [SerializeField] private int _count = 100;
    [SerializeField] private LayerMask _groundLayer;

    private List<GameObject> _spawnedDetails = new List<GameObject>();
    private float GetRandom(float limit) => Random.Range(-1f, 1f) * limit;
    private Vector3 GetWorldSpace(Vector2 point) => transform.TransformPoint(new Vector3(point.x, 0, point.y));

    private void Start()
    {
        Scatter();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) Scatter();
    }

    private void Scatter()
    {
        if (_prefabs.Count == 0) return;

        foreach (var go in _spawnedDetails) Destroy(go.gameObject);
        _spawnedDetails.Clear();
        for (int i = 0; i < _count; i++) {
            SpawnDetail();
        }
    }

    private void SpawnDetail()
    {
        var selected = _prefabs[Random.Range(0, _prefabs.Count)];
        var randomPoint = new Vector2(GetRandom(_limits.x), GetRandom(_limits.y));

        var rayOrigin = GetWorldSpace(randomPoint);

        bool hit = Physics.Raycast(rayOrigin, Vector3.down, out var hitData, 1000, _groundLayer);
        if (!hit) return;

        var rot = Quaternion.Euler(GetRandom(2), GetRandom(180), GetRandom(2));
        var newDetail = Instantiate(selected, hitData.point, rot, transform);
        newDetail.transform.localScale *= Random.Range(_sizeLimits.x, _sizeLimits.y);

        _spawnedDetails.Add(newDetail);
        newDetail.name = "spawnDetail " + _spawnedDetails.Count;
    }


    private void OnDrawGizmosSelected()
    {
        List<Vector2> corners = new List<Vector2>()
            {
            new Vector2(_limits.x, _limits.y),
            new Vector2(_limits.x, -_limits.y),
            new Vector2(-_limits.x, -_limits.y),
            new Vector2(-_limits.x, _limits.y)
        };

        for (int i = 0; i < corners.Count; i++) {
            var point = GetWorldSpace(corners[i]);
            var nextPoint = i < corners.Count - 1 ? GetWorldSpace(corners[i + 1]) : GetWorldSpace(corners[0]);
            Gizmos.DrawSphere(point, 0.5f);
            Gizmos.DrawLine(point, nextPoint);
        }
    }

}
