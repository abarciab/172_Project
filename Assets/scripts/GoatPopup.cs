using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoatPopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;

    public void Show(int numGoatsFound)
    {
        gameObject.SetActive(true);
        _text.text = numGoatsFound + "/" + GameManager.i.NumTotalGoats;
    }
}
