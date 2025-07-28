using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Management")]
    public string gameplaySceneName = "CAGED";

    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject controlsPanel;

    // ===== Main Menu Buttons =====
    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }
}
