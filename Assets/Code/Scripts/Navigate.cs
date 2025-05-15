using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigate : MonoBehaviour

{
    public void goToScene(string sceneName)
    {
        // Store the current scene *before* loading the new scene
        SceneTracker.previousScene = SceneManager.GetActiveScene().name;
        Debug.Log("Storing previous scene: " + SceneTracker.previousScene);

        if (sceneName == "Settings") // Check if we are going to settings
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetReturningFromSettings(); // Inform GameManager
            }
        }
        SceneManager.LoadScene(sceneName);
        Debug.Log("Loading scene: " + sceneName);
    }

    public void exitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }

    public void BackButton()
    {
        Debug.Log("BackButton clicked. Previous scene: " + SceneTracker.previousScene);
        if (!string.IsNullOrEmpty(SceneTracker.previousScene))
        {
            SceneManager.LoadScene(SceneTracker.previousScene);
            Debug.Log("Loading scene: " + SceneTracker.previousScene);
        }
        else
        {
            // Optional: fallback to MainMenu if we somehow don't have one saved
            SceneManager.LoadScene("MainMenu");
            Debug.Log("Loading scene: MainMenu (fallback)");
        }
    }

}