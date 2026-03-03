using UnityEngine;
using UnityEngine.SceneManagement; 

public class WinScreen : MonoBehaviour // Added : MonoBehaviour here
{
    public GameObject winCanvas;

    void Start()
    {
        // Make sure the screen is hidden when the game starts
        if (winCanvas != null)
            winCanvas.SetActive(false);
    }

    public void ShowWinScreen()
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
            Time.timeScale = 0f; // Pauses the game
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // IMPORTANT: Unpause or the game stays frozen
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}