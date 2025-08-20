using System.Collections.Generic;
using UnityEngine;

public class TipsPannel : MonoBehaviour
{
    [SerializeField] GameObject tipInteract;
    [SerializeField] GameObject tipDashMechanic;
    [SerializeField] GameObject tipProtectDistance;
    [SerializeField] GameObject tipAboutExp;

    private List<GameObject> tips;
    private int currentTipIndex = 0;

    void Start()
    {
        tips = new List<GameObject> { tipInteract, tipDashMechanic, tipProtectDistance, tipAboutExp };
        tips[0].SetActive(true);
        for (int i = 1; i < tips.Count; i++)
        {
            tips[i].SetActive(false);
        }
    }
    void OnEnable()
    {
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        GameController.IsInputBlocked = true;
        Time.timeScale = 0f;
    }

    void OnDisable()
    {
        Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;// Fare kapanýr
        GameController.IsInputBlocked = false;
        Time.timeScale = 1f;
    }
    public void Close()
    {
        foreach (GameObject tip in tips)
        {
            if (tip != null)
            {              
                Destroy(tip);
            }
        }
        this.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }


    public void Next()
    {
        if (currentTipIndex < tips.Count - 1)
        {
            tips[currentTipIndex].SetActive(false);
            currentTipIndex++;
            tips[currentTipIndex].SetActive(true);
        }
        else
        {
            Close();
        }
    }
}
