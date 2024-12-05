using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PAnimSpeeds {STILL, WALKING, RUNNING}

public class PAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _rollTrigger = "Roll";
    [SerializeField] private string _drawStaffTrigger = "DrawStaff";
    [SerializeField] private string _walkBool = "Walk";
    [SerializeField] private string _runBool = "Run";
    [SerializeField] private string _leftBool = "TurningLeft";
    [SerializeField] private string _rightBool = "TurningRight";

    [SerializeField, ReadOnly] private PAnimSpeeds _currentSpeed;

    public void DrawStaff() => _animator.SetTrigger(_drawStaffTrigger);

    public void Roll()
    {
        if (_currentSpeed == PAnimSpeeds.STILL) return;
        _animator.SetTrigger(_rollTrigger);
    }

    public void SetSpeed(PAnimSpeeds speed)
    {
        _currentSpeed = speed;

        _animator.SetBool(_walkBool, speed == PAnimSpeeds.WALKING);
        _animator.SetBool(_runBool, speed == PAnimSpeeds.RUNNING);

        if (speed != PAnimSpeeds.STILL) SetTurning(false, false);
    }

    public void SetTurning(bool left, bool right)
    {
        _animator.SetBool(_leftBool, left);
        _animator.SetBool(_rightBool, right);
    }

}
