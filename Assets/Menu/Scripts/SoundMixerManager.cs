
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    public static SoundMixerManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deðiþtiðinde kaybolmasýn
        }
        else
        {
            Destroy(gameObject); // Eðer zaten varsa, yeni oluþaný yok et
        }
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("masterVolume", volume);
        PlayerPrefs.Save();
    }
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("soundFXVolume", volume);
        PlayerPrefs.Save();
    }

}
