using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private SoundMixerManager soundMixer;

    void Start()
    {
        // SoundMixerManager'ý sahnede bul
        soundMixer = FindObjectOfType<SoundMixerManager>();

        if (soundMixer != null)
        {
            // Slider'larýn deðerlerini güncelle
            masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 1f);
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f);
            sfxSlider.value = PlayerPrefs.GetFloat("soundFXVolume", 1f);

            // Slider'lara fonksiyonlarý baðla
            masterSlider.onValueChanged.AddListener(soundMixer.SetMasterVolume);
            musicSlider.onValueChanged.AddListener(soundMixer.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(soundMixer.SetSFXVolume);

            soundMixer.SetMasterVolume(masterSlider.value);
            soundMixer.SetMusicVolume(musicSlider.value);
            soundMixer.SetSFXVolume(sfxSlider.value);
        }
    }
}
