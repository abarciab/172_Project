using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonEventCoord: MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Sound _hoverSound;
    [SerializeField] private Sound _clickSound;
    [SerializeField] private string _hoverBool = "Hover";

    private void Start()
    {
        if (_hoverSound != null) _hoverSound = Instantiate(_hoverSound);
        if (_clickSound != null) _clickSound = Instantiate(_clickSound);
    }

    public void SetTrigger(string trigger)
    {
        if (_animator) _animator.SetTrigger(trigger);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Click()
    {
        if (_clickSound != null) _clickSound.Play();
    }

    public void Hover()
    {
        if (_hoverSound != null) _hoverSound.Play();
        if (_animator) _animator.SetBool(_hoverBool, true);
    }

    public void EndHover()
    {
        if (_animator) _animator.SetBool(_hoverBool, false);
    }
}
