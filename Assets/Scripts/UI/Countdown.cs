using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    private bool isPaused = true;       
    private bool isPreGame = false;   
    private int timerDuration = 60; 
    public bool isTimerFinished;
    
    public AudioSource countdownMusic;
    public Text gameTimerText;
    private Coroutine activeCountdown; 

    private IEnumerator Timer(int duration) //Counts from 10 -> 0 per each question displayed
    {
        while (duration >= 0)
        {
            UpdateGameTimerUI();                  
            yield return new WaitForSeconds(1f);
            duration--;
        }

        // Timer reached 0
        duration = 0;
        UpdateGameTimerUI();                        
        isTimerFinished = true;
        isPaused = true;
    }

    public void GameTimer(int duration) 
    {
        isTimerFinished = false;
        isPaused = false;
    
        if (activeCountdown != null)
            StopCoroutine(activeCountdown);
    
        activeCountdown = StartCoroutine(Timer(duration));
    }
    private void UpdateGameTimerUI() //Displays the countdown/time remaning
    {
        gameTimerText.color = timerDuration < 5 ? Color.red : Color.white;
        gameTimerText.text = "Time Remaining: " + timerDuration.ToString();
    }

    public int GetSecondsLeft()
    {
        return timerDuration;
    }
}

