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
    [SerializeField] private string _drawSpearTrigger = "DrawSpear";
    [SerializeField] private string _putAwaySpearTrigger = "PutAwaySpear";
    [SerializeField] private string _spearOutBool = "SpearOut";
    [SerializeField] private string _walkBool = "Walk";
    [SerializeField] private string _runBool = "Run";
    [SerializeField] private string _leftBool = "TurningLeft";
    [SerializeField] private string _rightBool = "TurningRight";

    private PAnimSpeeds _currentSpeed;
    private bool _spearOut;

    public void DrawSpear() {
        if (_spearOut) return;
        _animator.SetTrigger(_drawSpearTrigger);
        SetSpearOut(true);
    }

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

    private void SetSpearOut(bool spearOut) {
        _spearOut = spearOut;
        _animator.SetBool(_spearOutBool, spearOut);
    }
}
