using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;

public class HUDOverlays : MonoBehaviour
{
    [SerializeField] private Image _dmgIndicator;
    [SerializeField] private Image _dmgFlash;
    [SerializeField] private Image _goopOverlay;
    [SerializeField] private GameObject _blackOut;
    [SerializeField] private float _redFlashTime = 0.1f;

    private float _goopTimeLeft;

    private void Start()
    {
        _goopOverlay.color = new Color(1, 1, 1, 0);
        _goopOverlay.gameObject.SetActive(false);
        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void Update()
    {
        _goopTimeLeft -= Time.deltaTime;
        if (_goopTimeLeft > 0) SetGoopAlpha(1);
        else if (_goopOverlay.gameObject.activeInHierarchy) SetGoopAlpha(0);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (type == UIAction.UPDATE_HP_OVERLAY && parameter is float playerHP) UpdateDmgIndicator(playerHP);
        if (type == UIAction.UPDATE_GOOP_OVERLAY && parameter is float goopTime) UpdateGoop(goopTime);
        if (type == UIAction.BLACK_OUT && parameter is float blackOutTime) BlackOut(blackOutTime);
    }

    public async void BlackOut(float length)
    {
        _blackOut.SetActive(true);
        await Task.Delay(Mathf.RoundToInt(length * 1000));
        _blackOut.SetActive(false);
    }


    public void UpdateDmgIndicator(float hpPercent)
    {
        var col = _dmgIndicator.color;
        col.a = (1 - hpPercent) * 0.8f;
        if (col.a > _dmgIndicator.color.a) StartCoroutine(FlashRed());
        _dmgIndicator.color = col;
    }

    private IEnumerator FlashRed()
    {
        _dmgFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(_redFlashTime);
        _dmgFlash.gameObject.SetActive(false);
    }

    private void UpdateGoop(float goopTime)
    {
        _goopTimeLeft = goopTime;
    }

    public void SetGoopAlpha(float targetAlpha)
    {
        Color targetCol = Color.black;
        targetCol.a = targetAlpha;
        _goopOverlay.color = Color.Lerp(_goopOverlay.color, targetCol, 10 * Time.deltaTime);
        _goopOverlay.gameObject.SetActive(_goopOverlay.color.a > 0.01f);
    }
}
