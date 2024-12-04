using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector2 _fovRanges;
    [SerializeField] private float _fovSnappiness = 5;

    [SerializeField] private float _posSnapiness;
    [SerializeField] private float _rotSnapiness;

    [SerializeField] private Camera _camera;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Vector3 _targetOffset;
    [SerializeField] private float _rotSpeed;
    [SerializeField] private float _aimSnapiness = 10;

    private Transform _player;
    private Vector3 _targetPosition;
    private Vector3 _aimTarget;
    [SerializeField, ReadOnly] private bool _hasAimTarget;

    public void ClearTarget() => _hasAimTarget = false;

    private void Start()
    {
        _player = Player.i.transform;
    }

    private void Update()
    {
        SetPos();
        Rotate();
        AimCamera();
        SetFOV();
    }

    public void SetTarget(Vector3 targetPoint)
    {
        print("set target");
        _hasAimTarget = true;
        _aimTarget = targetPoint;
    }

    private void SetFOV()
    {
        float targetFOV = Player.i.IsRunning ? _fovRanges.y : _fovRanges.x;
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, _fovSnappiness * Time.deltaTime);
    }

    private void Rotate()
    {
        if (_hasAimTarget) {
            var current = transform.rotation;
            transform.LookAt(_aimTarget);
            var euler = transform.localEulerAngles;
            euler.x = euler.z = 0;
            transform.localEulerAngles = euler;
            transform.rotation = Quaternion.Lerp(current, transform.rotation, _aimSnapiness * Time.deltaTime);  
        }
        else {
            var inputDelta = Input.GetAxis("Mouse X");
            inputDelta = Mathf.Clamp(inputDelta, -7, 7);
            transform.Rotate(0, inputDelta * _rotSpeed * Time.deltaTime * 100, 0);
        }

    }

    private void AimCamera()
    {
        Vector3 aimPos;
        if (_hasAimTarget) aimPos = _aimTarget; 
        else aimPos = transform.TransformPoint(_targetOffset);

        Debug.DrawLine(_camera.transform.position, aimPos, Color.red);
        var current = _camera.transform.rotation;
        _camera.transform.LookAt(aimPos);
        _camera.transform.rotation = Quaternion.Lerp(current, _camera.transform.rotation, _aimSnapiness * Time.deltaTime);
    }

    private void SetPos()
    {
        _targetPosition = _player.TransformPoint(_offset);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _posSnapiness * Time.fixedDeltaTime);
    }
}
