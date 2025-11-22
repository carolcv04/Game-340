using UnityEngine;
using Unity.Netcode;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("[ExitGame] Quitting game...");
        
        // Shutdown network if active
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log("[ExitGame] Shutting down network...");
            NetworkManager.Singleton.Shutdown();
        }
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("[ExitGame] Stopped play mode in editor");
#else
            Application.Quit();
            Debug.Log("[ExitGame] Application quit");
#endif
    }
}