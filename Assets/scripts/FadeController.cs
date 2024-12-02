using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Fade _fade;
    [SerializeField] private GameObject _gameOverScreen;
    [SerializeField] private Sound _snakeHissSound;
    [SerializeField] private float _creditFadeTime = 3.5f;

    private bool _loadingSave;
    private bool _gameOver;

    private void Start()
    {
        _fade.Disapear();
        _snakeHissSound = Instantiate(_snakeHissSound);
        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (type == UIAction.LOSE_GAME) GameOver();
        else if (type == UIAction.GO_TO_CREDITS) FadeToCredits();
        else if (type == UIAction.RESET_GAME) ResetGame();
    }


    public async void ResetGame()
    {
        if (_loadingSave) return;
        _loadingSave = true;

        Time.timeScale = 1;
        _fade.Appear();

        await Task.Delay(1500);
        GameManager.i.RestartScene();
    }

    public async void FadeToCredits()
    {
        if (_gameOver) return;
        _gameOver = true;

        _fade.Appear();
        await Task.Delay(Mathf.RoundToInt((_creditFadeTime) * 100));
        SceneManager.LoadScene(3);
    }


    public async void GameOver()
    {
        _gameOverScreen.SetActive(true);
        if (_loadingSave) return;
        _loadingSave = true;
        Time.timeScale = 1;
        _snakeHissSound.Play();

        _fade.Appear();
        await Task.Delay(Mathf.RoundToInt(_snakeHissSound.GetClipLength() * 1000));
        GameManager.i.RestartScene();
    }
}
