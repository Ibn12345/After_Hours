using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI; // Required for Button component

public class MainMenuManager : MonoBehaviour
{
    public Button playButton; // Reference to the Play Button

    void Start()
    {
        // Ensure the cursor is visible and unlocked when in the main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Add a listener to the button's onClick event
        if (playButton != null)
        {
         playButton.onClick.AddListener(StartGame);
         
        }
        else
        {
        Debug.LogError("Play Button not assigned in MainMenuManager!");
        }
    }

    // This method will be called when the Play Button is clicked
    public void StartGame()

    {
        // Load your main game scene.
        // Make sure the scene name matches exactly (e.g., "GameScene")
        SceneManager.LoadScene("GameScene");
    }
}
