using UnityEngine;
using UnityEngine.UI;
public class TabController : MonoBehaviour
{
    public Image[] tabImages;

    public GameObject[] pages;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateTab(0); //Always opens the first page
    }

    public void ActivateTab(int tabNumber)
    {
        for (int i = 0; i < pages.Length; i++) //Iterates through all the tab pages & sets them to false
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        //Activates the tab selected
        pages[tabNumber].SetActive(true);
        tabImages[tabNumber].color = Color.white;
    }
}
