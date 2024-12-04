using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobUpAndDown : MonoBehaviour
{
    [SerializeField] private float _amplitude = 0.005f;
    [SerializeField] private float _frequency = 5;
    [SerializeField] private bool _randomStart;
    private float _offset;
    [SerializeField] private bool _fixedTime;
    private float _startY;
    private const float _ampMod = 0.1f;

    private void Start()
    {
        _startY = transform.localPosition.y;
        if (_randomStart) _offset = Random.Range(-10, 10);
        _amplitude *= Random.Range(0.9f, 1.1f);
        _frequency *= Random.Range(0.9f, 1.1f);
    }

    void Update()
    {
        float time = _fixedTime ? Time.fixedTime : Time.time;
        float delta = (_amplitude * _ampMod) * Mathf.Sin((time + _offset) * (_frequency * Mathf.PI));
        var pos = transform.localPosition;
        pos.y = _startY + delta;
        transform.localPosition = pos;
    }

    private void OnDrawGizmosSelected() {
        var pos = transform.position;

        if (Application.isPlaying) {
            pos = transform.localPosition;
            pos.y = _startY;
            if (transform.parent) pos = transform.parent.TransformPoint(pos);
        }

        Gizmos.DrawWireSphere(pos + Vector3.up * (_amplitude * _ampMod), 0.05f);
        Gizmos.DrawWireSphere(pos + Vector3.down * (_amplitude * _ampMod), 0.05f);
    }
}
