using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public Image Preview;
    public TextMeshProUGUI PercentText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI GoatText;
    public GameObject ContinueTab;
    public GameObject NewTab;

    public void StartNew()
    {
        FindObjectOfType<TitleScreen>().StartNewGame();
    }
}
