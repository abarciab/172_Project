using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityDisplay : MonoBehaviour
{
    [SerializeField] private Image _mainImage;
    [SerializeField] private GameObject _disableObj;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Fade _whiteFade;

    public void SetEnabled(bool enabled) => _disableObj.SetActive(!enabled);
    public void SetImage(Sprite newSprite) => _mainImage.sprite = newSprite;
    public void Flash() => _whiteFade.Disapear();
    public void FlashAndEnable()
    {
        Flash();
        SetEnabled(true);
    }

    public void UpdateText(string newText)
    {
        _countText.text = newText;
        _countText.gameObject.SetActive(newText.Length > 0);
        _disableObj.SetActive(_countText.gameObject.activeInHierarchy);
    }
}
