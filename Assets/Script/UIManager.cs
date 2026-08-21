using UnityEngine;
using UnityEngine.SceneManagement; 

public class UIManager : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Enter the exact name of your Game scene (e.g., 'GameScene')")]
    public string playOnlineSceneName = "Menu"; 

    public string mainMenuSceneName = "MainMenu"; // The name of your Main Menu scene

    [Tooltip("Enter the exact name of your How To Play scene")]
    public string howToPlaySceneName = "HowToPlay"; 

    public void OnClickBackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Call this from the "PLAY ONLINE" Button's OnClick() event
    public void OnClickPlayOnline()
    {
        SceneManager.LoadScene(playOnlineSceneName);
    }

    // Call this from the "HOW TO PLAY" Button's OnClick() event
    public void OnClickHowToPlay()
    {
        SceneManager.LoadScene(howToPlaySceneName);
    }

    // Call this from the "QUIT GAME" Button's OnClick() event
    public void OnClickQuitGame()
    {
        Debug.Log("Quit Game Initiated.");
        Application.Quit();

        // This ensures the quit button stops the game while testing inside the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}