using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private float _lerpFactor = 5;

    private void Start()
    {
        SwitchToScene(PlayerPrefs.GetInt("NEXTSCENE", 0));
    }

    private async void SwitchToScene(int nextScene)
    {
        _progressSlider.value = 0;

        await Task.Delay(1000);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        while (!asyncLoad.isDone) {
            _progressSlider.value = Mathf.Lerp(_progressSlider.value, asyncLoad.progress, _lerpFactor * Time.deltaTime);
            await Task.Yield();
        }

        while (_progressSlider.value < 0.99f) {
            _progressSlider.value = Mathf.Lerp(_progressSlider.value, 1, _lerpFactor * Time.deltaTime);
            await Task.Yield();
        }
    }

}
