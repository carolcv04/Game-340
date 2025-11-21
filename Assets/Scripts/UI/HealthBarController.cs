/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 */

using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    private GameObject[] heartContainers;
    private Image[] heartFills;

    public Transform heartsParent;
    public GameObject heartContainerPrefab;
    public PlayerStats playerStats;

    public void InitializeHUD()
    {
        if (playerStats == null)
        {
            Debug.LogError("HealthBarController: playerStats not assigned!");
            return;
        }
        
        heartContainers = new GameObject[(int)playerStats.MaxTotalHealth];
        heartFills = new Image[(int)playerStats.MaxTotalHealth];

        playerStats.onHealthChangedCallback += UpdateHeartsHUD;

        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }
    
    public void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }

    void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < playerStats.MaxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    void SetFilledHearts()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            // Changed from playerStats.Health to playerStats.CurrentHealth
            if (i < playerStats.CurrentHealth)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }

        // Handle half hearts
        if (playerStats.CurrentHealth % 1 != 0)
        {
            int lastPos = Mathf.FloorToInt(playerStats.CurrentHealth);
            heartFills[lastPos].fillAmount = playerStats.CurrentHealth % 1;
        }
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < playerStats.MaxTotalHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }
}