using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class TitleScreen : MonoBehaviour {

    [SerializeField] private Fade _fade;
    [SerializeField] private GameObject _video;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private Sound _click;
    [SerializeField] private float _startDelayTime = 0.5f;

    private bool _started;

    private void Start()
    {
        _click = Instantiate(_click);
        Time.timeScale = 1;
    }

    public void Click() => _click.Play();

    public void Quit()
    {
        Application.Quit();
    }

    public void StartNewGame()
    {
        if (_started) return;
        _started = true;

        Click();
        StopAllCoroutines();
        _StartGame(_startDelayTime);
    }

    private async void _StartGame(float delay)
    {
        FindObjectOfType<MusicPlayer>().FadeOutCurrent(delay);
        await Task.Delay((int) (delay * 1000));

        _video.SetActive(true);

        float timePassed = -0.1f;
        while (!Input.GetKeyDown(KeyCode.Escape) && timePassed < _videoPlayer.length) {
            timePassed += Time.deltaTime;
            await Task.Yield();
        }

        LoadGame();
    }

    public async void LoadGame()
    {        
        _fade.Appear();
        await Task.Delay(Mathf.RoundToInt(_fade.FadeTime * 1000));

        Destroy(AudioManager.i.gameObject);
        AudioManager.i = null;

        PlayerPrefs.SetInt("NEXTSCENE", 2);
        SceneManager.LoadScene(4);
    }

    public void Credits()
    {
        SceneManager.LoadScene(3);
    }

}
