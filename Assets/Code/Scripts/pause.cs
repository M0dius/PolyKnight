using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Options Menu Buttons")]
    public Button optionsBackButton;

    [Header("Options Settings")]
    public Slider musicVolumeSlider;
    public Toggle fullscreenToggle;

    private void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        optionsButton.onClick.AddListener(OpenOptionsMenu);
        quitButton.onClick.AddListener(QuitGame);

        optionsBackButton.onClick.AddListener(CloseOptionsMenu);

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        LoadSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                if (optionsMenuUI.activeSelf)
                {
                    CloseOptionsMenu();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze game time
        GameIsPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f; 
        GameIsPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenOptionsMenu()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        SaveSettings();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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
        
        // Save fullscreen setting
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}