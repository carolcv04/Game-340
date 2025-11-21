using TMPro;
using UnityEngine;
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerWonText;
    private void Start()
    {
        FountainGameManager.Instance.OnStateChanged += FountainGameManager_OnStateChanged;
        Hide();
    }

    private void FountainGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (FountainGameManager.Instance.IsGameOver())
        {
            Show();
            
            //TODO: playerWonText.text = player that successfully claimed the fountaint
        }
        else
        {
            Hide();
        }
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);    
    }
}
