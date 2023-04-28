using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class CheckPoint
    {
        public Transform point;
        public int ID;

        public CheckPoint(Transform _point, int _ID)
        {
            point = _point;
            ID = _ID;
        }
    }

    public static GameManager i;
    
    public bool paused { get; private set; }

    [SerializeField] List<CheckPoint> checkPoints = new List<CheckPoint>();
    bool started;

    [Header("Manual Setup")]
    [SerializeField] int startingCheckPoint;
    [SerializeField] bool setStarting;

    public void AddCheckPoint(Transform point, int ID)
    {
        checkPoints.Add(new CheckPoint(point, ID));
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
        started = true;
        int checkPoint = PlayerPrefs.GetInt("checkpoint");
        if (GetCheckPoint(checkPoint) == Vector3.zero) return;
        Player.i.transform.position = GetCheckPoint(checkPoint);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(0);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    public void SetCheckPointManually()
    {
        PlayerPrefs.SetInt("checkpoint", startingCheckPoint);
    }

    Vector3 GetCheckPoint(int ID)
    {
        foreach (var c in checkPoints) if (c.ID == ID) return c.point.position;
        return Vector3.zero;
    }

    private void Update()
    {
        if (setStarting) {
            PlayerPrefs.SetInt("checkpoint", startingCheckPoint);
            setStarting = false;
        }

        if (!started) RestartFromCheckPoint();
        
    }

    void Awake() 
    { 
        i = this;
        if (!PlayerPrefs.HasKey("checkpoint")) PlayerPrefs.SetInt("checkpoint", -1);
    }
}
