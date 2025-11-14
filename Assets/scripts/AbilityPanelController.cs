using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanelController : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilityDisplay _leftClickAbility;
    [SerializeField] private AbilityDisplay _rightClickAbility;
    [SerializeField] private AbilityDisplay _specialAbility;
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
        ClearAbilityDisplays();
    }

    private void ClearAbilityDisplays() {
        /*_leftClickAbility.Clear();
        _rightClickAbility.Clear();
        _specialAbility.Clear();*/
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
        _leftClickAbility.FlashAndEnable();
        _rightClickAbility.FlashAndEnable();
    }

    private void OnThrowSpear()
    {
        _leftClickAbility.SetImage(_recallSprite);
        _leftClickAbility.SetEnabled(false);

        _rightClickAbility.SetImage(_recallSprite);
        _rightClickAbility.SetEnabled(false);
    }

    private void OnCatchSpear()
    {
        _leftClickAbility.SetImage(_throwSprite);
        _rightClickAbility.SetImage(_stabSprite);
    }

    private void DisplaySunblastCooldown(float cooldown)
    {
        int num = Mathf.CeilToInt(cooldown);
        _specialAbility.UpdateText(num > 0 ? num.ToString() : "");
    }

    private void OnSunblastReady()
    {
        _specialAbility.Flash();
        _specialAbility.UpdateText("");
    }

    public void DisplayPlayerHP (float hpPercent)
    {
        _hpBar.value = hpPercent;
        GlobalUI.i.Do(UIAction.UPDATE_HP_OVERLAY, hpPercent);
    }
}
