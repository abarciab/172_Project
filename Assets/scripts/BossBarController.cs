using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossBarController : MonoBehaviour
{

    public Slider _bossSlider;

    [SerializeField] private GameObject _bossBar;
    [SerializeField] TextMeshProUGUI _bossName;

    private GameObject _activeBoss;

    public void SetSliderValue(float value) => _bossSlider.value = value;

    public void StartBossFight(GameObject bossObj)
    {
        gameObject.SetActive(true);
        _bossName.text = bossObj.name;
        _activeBoss = bossObj;
    }


    public void EndBossFight()
    {
        gameObject.SetActive(false);
    }
}
