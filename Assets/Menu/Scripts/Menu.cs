
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] public SoundMixerManager soundMixer; 

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    private void Awake()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("soundFXVolume", 1f);
    }
    private void Start()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex != 0) // Ana menü deðilse fareyi gizle
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        soundMixer.SetMasterVolume(masterSlider.value);
        soundMixer.SetMusicVolume(musicSlider.value);
        soundMixer.SetSFXVolume(sfxSlider.value);
    }
    public void PlayGame()
    {
        PlayerPrefs.SetFloat("masterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("soundFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        PlayerPrefs.SetFloat("masterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("soundFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
        Application.Quit();
    }
}
