using System;
using TMPro;
using UnityEngine;

public class GameStartCountdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Start()
    {
        FountainGameManager.Instance.OnStateChanged += FountainGameManager_OnStateChanged;
        HideCoundown();
    }

    private void FountainGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (FountainGameManager.Instance.IsCountdownToStartActive())
        {
            ShowCountdown();
        }
        else
        {
            HideCoundown();
        }
    }
    private void Update()
    {
        if (FountainGameManager.Instance.IsCountdownToStartActive())
        {
            float countdown = FountainGameManager.Instance.GetCountdownToStartTimer();
            countdownText.text = Mathf.Ceil(Mathf.Max(0, countdown)).ToString();
        }    
    }
    private void HideCoundown()
    {
        gameObject.SetActive(false);
    }
    private void ShowCountdown()
    {
        gameObject.SetActive(true);    
    }
}
