using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


[System.Serializable]
public class DisplayImageData
{
    [HideInInspector] public string Name;
    public Fact TriggerFact;
    public Sprite Img;
    public float WaitTime = 1.5f;

    [SerializeField] private Sound _playOnDisplay;

    public void PlaySound()
    {
        if (_playOnDisplay == null) return;

        if (!_playOnDisplay.Instantialized) _playOnDisplay = GameObject.Instantiate(_playOnDisplay);
        _playOnDisplay.Play();
    }
}

public class ImageDisplay : MonoBehaviour
{
    [SerializeField] private List<DisplayImageData> _displayImages = new List<DisplayImageData>();
    [SerializeField] private Image _displayObj;
    [SerializeField] private Sound _turnPageSound;

    private void Start()
    {
        _turnPageSound = Instantiate(_turnPageSound);
        GlobalUI.i.OnUpdateUI.AddListener(OnUpdate);
    }

    private void OnUpdate(UIAction type, object parameter)
    {
        if (type == UIAction.DISPLAY_IMAGE && parameter is DisplayImageData data) DisplayImage(data);
    }

    public void Inform(Fact newFact)
    {
        for (int i = 0; i < _displayImages.Count; i++) {
            if (_displayImages[i].TriggerFact != newFact) continue;
            DisplayImage(_displayImages[i]);
            _displayImages.RemoveAt(0);
        }
    }

    public void DisplayImage(DisplayImageData data)
    {
        _displayObj.sprite = data.Img;
        _displayObj.gameObject.SetActive(true);

        Player.i.FreezePlayer();
        data.PlaySound();

        StartCoroutine(WaitForDisplayedItem(data));
    }

    private IEnumerator WaitForDisplayedItem(DisplayImageData data)
    {
        yield return new WaitForSeconds(data.WaitTime);
        while (!Input.GetKeyDown(KeyCode.F)) yield return null;

        EndDisplayImage(data);
    }

    private void EndDisplayImage(DisplayImageData data)
    {
        _turnPageSound.Play();
        _displayObj.gameObject.SetActive(false);
        Player.i.UnfreezePlayer();
    }
}
