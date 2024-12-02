using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DoorState
{
    public Vector3 Pos;
    public Quaternion Rot;

    public DoorState(Vector3 pos, Quaternion rot)
    {
        Pos = pos;
        Rot = rot;
    }
}

public class DoorController : MonoBehaviour
{
    [SerializeField, ReadOnly] private DoorState _openState;
    [SerializeField, ReadOnly] private DoorState _closeState;
    [SerializeField] private bool _open;
    [HideInInspector] public bool IsOpen => _open;

    [Header("Animation")]
    [SerializeField] private float _animateTime = 1.2f;
    [SerializeField] private AnimationCurve _curve;

    [Header("Sounds")]
    [SerializeField] private Sound _openSound;

    [Header("Automatic Behavior")]
    [SerializeField] private bool _openAutomatically;
    [SerializeField, ConditionalField(nameof(_openAutomatically))] private float _automaticDetectionRange = 3;

    [ButtonMethod]
    private void SetClosed()
    {
        _closeState.Pos = transform.localPosition;
        _closeState.Rot = transform.localRotation;
    }

    [ButtonMethod]
    private void SetOpen()
    {
        _openState.Pos = transform.localPosition;
        _openState.Rot = transform.localRotation;
    }

    [ButtonMethod]
    public void Open()
    {
        StopAllCoroutines();
        if (!Application.isPlaying) SnapToState(_openState, true);
        else StartCoroutine(AnimateToState(_openState, true));
    }

    [ButtonMethod]
    public void Close()
    {
        StopAllCoroutines();
        if (!Application.isPlaying) SnapToState(_closeState, false);
        else StartCoroutine(AnimateToState(_closeState, false));
    }

    private void Start()
    {
        _openSound = Instantiate(_openSound);
    }

    private void Update()
    {
        if (!_openAutomatically || _open) return;

        float dist = Mathf.Infinity;
        if (dist > _automaticDetectionRange) return;

        _open = true;
        Open();
    }

    private void SnapToState(DoorState state, bool openState)
    {
        transform.localPosition = state.Pos; 
        transform.localRotation = state.Rot;
        _open = openState;
    }

    private IEnumerator AnimateToState(DoorState target, bool targetStateOpen)
    {
        DoorState startState = new DoorState(transform.localPosition, transform.localRotation);

        float timePassed = 0;
        while (timePassed <  _animateTime) {
            float progress = timePassed / _animateTime;
            progress = _curve.Evaluate(progress);

            transform.localPosition = Vector3.Lerp(startState.Pos, target.Pos, progress);
            transform.localRotation = Quaternion.Lerp(startState.Rot, target.Rot, progress);

            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        SnapToState(target, targetStateOpen);
        _openSound.Play(transform);
    }

    private void OnDrawGizmosSelected()
    {
        if (_openAutomatically) Gizmos.DrawWireSphere(transform.position, _automaticDetectionRange);
    }
}
