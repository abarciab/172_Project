using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanelController : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilityDisplay _throwAbility;
    [SerializeField] private AbilityDisplay _stabAbility;
    [SerializeField] private AbilityDisplay _sunblastAbility;
    [SerializeField] private float _abilityFlashTime = 0.1f;

    [Header("sprites")]
    [SerializeField] private Sprite _throwSprite;
    [SerializeField] private Sprite _stabSprite;
    [SerializeField] private Sprite _recallSprite;

    [Header("Player Health")]
    [SerializeField] private Slider _hpBar;

    private void Start()
    {
        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (type == UIAction.SUNBLAST_READY) OnSunblastReady();
        else if (type == UIAction.DISPLAY_SUNBLAST_COOLDOWN && parameter is float cooldown) DisplaySunblastCooldown(cooldown);
        else if (type == UIAction.CATCH_SPEAR) OnCatchSpear();
        else if (type == UIAction.THROW_SPEAR) OnThrowSpear();
        else if (type == UIAction.RECALL_READY) OnRecallReady();
    }

    private void OnRecallReady()
    {
        _throwAbility.FlashAndEnable();
        _stabAbility.FlashAndEnable();
    }

    private void OnThrowSpear()
    {
        _throwAbility.SetImage(_recallSprite);
        _throwAbility.SetEnabled(false);

        _stabAbility.SetImage(_recallSprite);
        _stabAbility.SetEnabled(false);
    }

    private void OnCatchSpear()
    {
        _throwAbility.SetImage(_throwSprite);
        _stabAbility.SetImage(_stabSprite);
    }

    private void DisplaySunblastCooldown(float cooldown)
    {
        int num = Mathf.CeilToInt(cooldown);
        _sunblastAbility.UpdateText(num > 0 ? num.ToString() : "");
    }

    private void OnSunblastReady()
    {
        _sunblastAbility.Flash();
        _sunblastAbility.UpdateText("");
    }

    public void DisplayPlayerHP (float hpPercent)
    {
        _hpBar.value = hpPercent;
        GlobalUI.i.Do(UIAction.UPDATE_HP_OVERLAY, hpPercent);
    }
}
