
using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject info;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);
        info.SetActive(false);
        Time.timeScale = 0f;
    }
    public void Continue() 
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    public void Restart()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Options()
    {
        Time.timeScale = 0f;
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        optionsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }
    public void Info()
    {
        Time.timeScale = 0f;
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        info.SetActive(true);
        pauseMenu.SetActive(false);
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
}
