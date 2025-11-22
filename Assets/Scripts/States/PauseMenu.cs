using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    public GameObject pauseMenuUI;
    private bool isGamePaused = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    public static bool IsGamePaused()
    {
        return Instance != null && Instance.isGamePaused;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
            PauseGame();
        else
            ResumeGame();
    }

    private void PauseGame()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        SetGameplayEnabled(false);
    }

    public void Resume()
    {
        isGamePaused = false;
        ResumeGame();
    }

    private void ResumeGame()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        SetGameplayEnabled(true);
    }

    private void SetGameplayEnabled(bool enabled)
    {
        // Disable/enable player controls
        var playerControllers = FindObjectsOfType<PlayerController>();
        foreach (var controller in playerControllers)
        {
            controller.enabled = enabled;
        }

        // Disable/enable enemy AI if needed
        // var enemies = FindObjectsOfType<EnemyAI>();
        // foreach (var enemy in enemies)
        //     enemy.enabled = enabled;
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
