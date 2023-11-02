using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnimation : MonoBehaviour
{
    [SerializeField] List<string> options;
    [SerializeField] float timeBetween;
    [SerializeField] TextMeshProUGUI text;

    private void OnEnable() {
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate() {
        while (true) {
            yield return new WaitForSecondsRealtime(timeBetween);
            text.text = options[0];
            options.Add(options[0]);
            options.RemoveAt(0);
        }
    }
}
