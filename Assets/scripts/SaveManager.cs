using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SaveData
{
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;
    public float Time;
    public float GameProgress;
    public List<int> KilledEnemies = new List<int>();
    public List<int> KilledBosses = new List<int>();
    public List<int> CollectedGoats = new List<int>();

    public override string ToString()
    {
        var output = "";
        output += GetV3String(PlayerPosition);
        output += GetQuatString(PlayerRotation);
        output += GetNumString(Time);
        output += GetNumString(GameProgress);
        output += GetListString(KilledEnemies);
        output += GetListString(KilledBosses);
        output += GetListString(CollectedGoats);
        return output;
    }

    private string GetNumString(float num) => num + "|";
    private string GetV3String(Vector3 input) => input.x + "," + input.y + "," + input.z + "|";
    private string GetQuatString(Quaternion input) => input.x + "," + input.y + "," + input.z + "," + input.w + "|";
    private string GetListString(List<int> collection) => (string.Join(",", collection)) + "|";
    
    public void LoadFromString(string input)
    {
        try {
            input = input.Trim().Replace("\n", "");

            KilledEnemies.Clear();
            KilledBosses.Clear();
            CollectedGoats.Clear();

            var parts = input.Split("|");

            PlayerPosition = GetV3(parts[0]);
            PlayerRotation = GetQuat(parts[1]);

            Time = float.Parse(parts[2]);
            GameProgress = float.Parse(parts[3]);

            KilledEnemies.AddRange(GetIntList(parts[4]));
            KilledBosses.AddRange(GetIntList(parts[5]));
            CollectedGoats.AddRange(GetIntList(parts[6]));
        }

        catch (System.Exception e) {
            Debug.Log("data load error. input: " + input);
            throw(e);
        }
    }

    private Quaternion GetQuat(string input)
    {
        var parts = input.Split(",");
        var v3 = new Quaternion();
        PlayerRotation.x = float.Parse(parts[0]);
        PlayerRotation.y = float.Parse(parts[1]);
        PlayerRotation.z = float.Parse(parts[2]);
        PlayerRotation.w = float.Parse(parts[3]);
        return v3;
    }

    private Vector3 GetV3(string input)
    {
        var parts = input.Split(",");
        var v3 = new Vector3();
        PlayerPosition.x = float.Parse(parts[0]);
        PlayerPosition.y = float.Parse(parts[1]);
        PlayerPosition.z = float.Parse(parts[2]);
        return v3;
    }

    private List<int> GetIntList(string input)
    {
        var list = new List<int>();
        var stringList = input.Split(",");
        if (stringList.Length > 0 && !string.IsNullOrEmpty(stringList[0])) {
            foreach (var s in stringList) list.Add(int.Parse(s));
        }
        return list;
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager i;
    private void Awake() { i = this; }

    [SerializeField] private SaveData _currentData;

    [HideInInspector]public readonly static string path = "/saves.txt";
    [HideInInspector]public readonly static string ssPath = "/screenshot";
    [HideInInspector] public UnityEvent<SaveData> OnLoad;

    private int _slot;

    private async void Start()
    {
        _slot = PlayerPrefs.GetInt("CURRENTSAVE", 0);

        await Task.Yield();
        LoadCurrent();
    }

    [ButtonMethod]
    private void SaveCurrent()
    {
        var path = Application.persistentDataPath + SaveManager.path;

        var saves = File.ReadAllText(path).Split("\n");
        saves[_slot] = _currentData.ToString();


        using StreamWriter writer = File.CreateText(path);
        writer.WriteLine(string.Join("\n", saves) + "\n");
        CaptureScreenshot(Application.persistentDataPath + ssPath + _slot + ".png");
        print("Save successful");
    }

    private void CaptureScreenshot(string path)
    {
        var cam = Camera.main;
        var width = 1920;
        var height = 1080;

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        cam.Render();

        Texture2D ss = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        ss.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null;

        byte[] bytes = ss.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Destroy(ss);
        Destroy(rt);
    }


    private void LoadCurrent()
    {
        var path = Application.persistentDataPath + SaveManager.path;

        if (!File.Exists(path)) {
            ResetAllSaves();
        }

        int index = PlayerPrefs.GetInt("CURRENTSAVE", 0);
        var saves = File.ReadAllText(path).Split("\n");
        _currentData.LoadFromString(saves[index]);

        OnLoad.Invoke(_currentData);
    }

    [ButtonMethod]
    private void ResetAllSaves()
    {
        var path = Application.persistentDataPath + SaveManager.path;

        using StreamWriter writer = File.CreateText(path);
        var emptyLine = new SaveData().ToString() + "\n";
        writer.WriteLine(emptyLine + emptyLine + emptyLine);

        var ssPath = Application.persistentDataPath + SaveManager.ssPath;

        if (File.Exists(ssPath + "0.png")) File.Delete(ssPath + "0.png");
        if (File.Exists(ssPath + "1.png")) File.Delete(ssPath + "1.png");
        if (File.Exists(ssPath + "2.png")) File.Delete(ssPath + "2.png");

        print("path: " + path);
    }
}
