using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigate : MonoBehaviour

{

    public void goToScene(string sceneName)
    {
        SceneTracker.previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void exitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }

    public void BackButton()
    {
        if (!string.IsNullOrEmpty(SceneTracker.previousScene))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneTracker.previousScene);
        }
        else
        {
            // Optional: fallback to MainMenu if we somehow don't have one saved
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

}