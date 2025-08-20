using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private SoundMixerManager soundMixer;

    void Start()
    {
        // SoundMixerManager'� sahnede bul
        soundMixer = FindObjectOfType<SoundMixerManager>();

        if (soundMixer != null)
        {
            // Slider'lar�n de�erlerini g�ncelle
            masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 1f);
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f);
            sfxSlider.value = PlayerPrefs.GetFloat("soundFXVolume", 1f);

            // Slider'lara fonksiyonlar� ba�la
            masterSlider.onValueChanged.AddListener(soundMixer.SetMasterVolume);
            musicSlider.onValueChanged.AddListener(soundMixer.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(soundMixer.SetSFXVolume);

            soundMixer.SetMasterVolume(masterSlider.value);
            soundMixer.SetMusicVolume(musicSlider.value);
            soundMixer.SetSFXVolume(sfxSlider.value);
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
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex != 0) // Ana men� de�ilse fareyi gizle
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        GameController.IsInputBlocked = false;
        Time.timeScale = 1f;
    }

}
