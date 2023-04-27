using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    void Awake() { i = this; }
    public bool paused { get; private set; }

    public List<Transform> checkPoints = new List<Transform>();

    private void Start()
    {
        RestartFromCheckPoint();
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
    }

    public void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
    }

    public void TogglePause()
    {
        if (paused) Unpause();
        else Pause();
    }

    public void RestartFromCheckPoint()
    {
        int checkPoint = PlayerPrefs.GetInt("checkPoint");
        if (checkPoints.Count <= checkPoint) return;
        Player.i.transform.position = checkPoints[checkPoint].position;
    }
}
