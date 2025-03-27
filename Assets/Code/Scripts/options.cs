using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsMenuController : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider musicVolumeSlider;

    [Header("Graphics Settings")]
    public Toggle fullscreenToggle;

    [Header("Menu Buttons")]
    public Button saveButton;
    public Button loadButton;
    public Button backButton;

    private void Start()
    {
        saveButton.onClick.AddListener(SaveSettings);
        loadButton.onClick.AddListener(LoadSettings);
        backButton.onClick.AddListener(BackToMainMenu);

        LoadSettings();

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void LoadSettings()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}