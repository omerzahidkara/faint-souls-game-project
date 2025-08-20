using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathPannel : MonoBehaviour
{
    [SerializeField] GameObject deathPannel;
    public void Restart()
    {
        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        yield return new WaitForEndOfFrame();
        if (SoundFXManager.instance != null)
        {
            SoundFXManager.instance.KillYourself();
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        //Oyun kapanmadan önce ses ögeleri silinirs
        GameObject[] soundObjects = GameObject.FindGameObjectsWithTag("SoundFX");

        foreach (GameObject obj in soundObjects)
        {
            Destroy(obj);
        }
        SceneManager.LoadScene(0);
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
}
