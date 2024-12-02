using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementController : MonoBehaviour
{
    public static AchievementController i;

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        return;
        try {
            Steamworks.SteamClient.Init(2596710);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }

    public void Unlock(string id, bool unlock = true)
    {
        var ach = new Steamworks.Data.Achievement(id);

        if (unlock) ach.Trigger();
        else ach.Clear();
    }

}
