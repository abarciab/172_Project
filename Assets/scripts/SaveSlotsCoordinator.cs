using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class SaveSlotsCoordinator : MonoBehaviour
{
    [SerializeField] private List<SaveSlot> _slots = new List<SaveSlot>();

    [SerializeField] private SaveData _testData;
    [SerializeField] private int _slotIndex = 0;
    private readonly string _path = SaveManager.path;
    private readonly string _ssPath = SaveManager.ssPath;

    private void Start()
    {
        for (int i = 0; i < _slots.Count; i++) {
            bool exists = (File.Exists(Application.persistentDataPath + _ssPath + i + ".png"));
            if (exists) ConfigureFromSaveData(i);
            else ConfigureNew(i);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) gameObject.SetActive(false);
    }

    private void ConfigureFromSaveData(int i)
    {
        var slot = _slots[i];
        slot.NewTab.SetActive(false);
        slot.ContinueTab.SetActive(true);

        var tex = LoadTextureFromFile(Application.persistentDataPath + _ssPath + i + ".png");
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        slot.Preview.sprite = sprite;
    }

    private void ConfigureNew(int i)
    {
        var slot = _slots[i];
        slot.NewTab.SetActive(true);
        slot.ContinueTab.SetActive(false);
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        return texture;

    }

    [ButtonMethod] 
    private void LoadDataFromSelectedSlot()
    {
        var path = Application.persistentDataPath + _path;

        if (!File.Exists(path)) {
            ResetAllSaves();
        }

        var saves = File.ReadAllText(path).Split("\n");
        _testData.LoadFromString(saves[_slotIndex].Replace("\n", ""));

        PlayerPrefs.SetInt("CURRENTSAVE", _slotIndex);
    }

    [ButtonMethod]
    private void ResetAllSaves()
    {
        var path = Application.persistentDataPath + _path;

        using StreamWriter writer = File.CreateText(path);
        var emptyLine = new SaveData().ToString() + "\n";
        writer.WriteLine(emptyLine + emptyLine + emptyLine);

        var ssPath = Application.persistentDataPath + _ssPath;

        if (File.Exists(ssPath + "0.png")) File.Delete(ssPath + "0.png");
        if (File.Exists(ssPath + "1.png")) File.Delete(ssPath + "1.png");
        if (File.Exists(ssPath + "2.png")) File.Delete(ssPath + "2.png");
    }
}
