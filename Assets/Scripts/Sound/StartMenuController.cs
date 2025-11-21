using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private Button lobbiesButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.GameScene);
        });
        
        lobbiesButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LobbyTutorial_Done");
        });
        
        optionsButton.onClick.AddListener(() =>
        {
            // SceneManager.LoadScene("");
        });
        
        quitButton.onClick.AddListener(() =>
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif 
                Application.Quit();
        });

    }
    
    private void LobbiesClick()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnLobbiesClick()
    {
        SceneManager.LoadScene("LobbyTutorial_Done");
    }

    public void OnExitClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif 
            Application.Quit();
    }
}
