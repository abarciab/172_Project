using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _menuParent;
    [SerializeField] private GameObject _settingsSection;
    [SerializeField] private GameObject _savesSection;
    [SerializeField] private GameObject _controlsSection;
    [SerializeField] private Sound _openMenuSound;

    [Header("Volume Sliders")]
    [SerializeField] Slider _masterSlider;
    [SerializeField] Slider _sfxSlider; 
    [SerializeField] Slider _musicSlider;

    private void Awake()
    {
        _masterSlider.onValueChanged.AddListener(UpdateVolumeValues);
        _musicSlider.onValueChanged.AddListener(UpdateVolumeValues);
        _sfxSlider.onValueChanged.AddListener(UpdateVolumeValues);
        _openMenuSound = Instantiate(_openMenuSound);
    }

    private void Start()
    {
        _openMenuSound.Play();
        SetSliderPositions(AudioManager.i.Volumes);
        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (type == UIAction.PAUSE) Pause();
        else if (type == UIAction.RESUME) Resume();
    }

    public void SetPaused(bool paused)
    {
        if (paused) Pause();
        else Resume();
    }

    public void Resume()
    {
        _menuParent.SetActive(false);
    }

    public void Pause()
    {
        _menuParent.SetActive(true);
    }

    private void UpdateVolumeValues(float value)
    {
        AudioManager.i.SetMasterVolume(_masterSlider.value);
        AudioManager.i.SetMusicVolume(_musicSlider.value);
        AudioManager.i.SetSfxVolume(_sfxSlider.value);
    }


    public void SetSliderPositions(Vector4 volumes)
    {
        _masterSlider.value = volumes.x;
        _musicSlider.value = volumes.y;
        _sfxSlider.value = volumes.y;
    }

    public void ToggleSaves()
    {
        HideMenuSections();
        _savesSection.SetActive(!_savesSection.activeInHierarchy);
    }

    public void ToggleControls()
    {
        HideMenuSections();
        _controlsSection.SetActive(!_controlsSection.activeInHierarchy);
    }

    public void ToggleSettings()
    {
        HideMenuSections();
        _settingsSection.SetActive(!_settingsSection.activeInHierarchy);
    }

    public void HideMenuSections()
    {
        _settingsSection.SetActive(false);
        _controlsSection.SetActive(false);
        _savesSection.SetActive(false);
    }
}
