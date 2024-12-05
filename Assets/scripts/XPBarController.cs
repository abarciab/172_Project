using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPBarController : MonoBehaviour
{
    [SerializeField] private Material _completedMat;
    [SerializeField] private Material _uncompletedMat;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _imgParent;

    [SerializeField] private int _tempXPMax = 5;

    private List<Image> _spawnedImgs = new List<Image>();
    private int _numCompleted;

    private void Start()
    {
        SpawnImgs(_tempXPMax);
    }

    private void SpawnImgs(int amount)
    {
        foreach (var img in _spawnedImgs) Destroy(img.gameObject);
        _spawnedImgs.Clear();

        for (int i = 0; i < amount; i++) {
            var newImg = Instantiate(_prefab, _imgParent);
            _spawnedImgs.Add(newImg.GetComponent<Image>());
        }

        _numCompleted = 0;
        UpdateDisplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            _numCompleted++;
            UpdateDisplay();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            _numCompleted--;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        _numCompleted = Mathf.Clamp(_numCompleted, -1, _spawnedImgs.Count);

        for (int i = 0; i < _spawnedImgs.Count; i++) {
            _spawnedImgs[i].material = i < _numCompleted ? _completedMat : _uncompletedMat;
        }
    }
}
