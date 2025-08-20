
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    public float clipLength;

    private void Awake()
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
    public void KillYourself()
    {
        Destroy(gameObject); // Restart ederken kullanmak için
    }
    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (audioClip == null || soundFXObject == null) return;

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        float clipLengthSafe = audioClip.length;
        if (clipLengthSafe <= 0f)
        {
            clipLengthSafe = 1f; // Ses uzunluðu alýnamazsa 1 saniye varsay
        }

        AutoDestroyUnscaled destroyer = audioSource.gameObject.AddComponent<AutoDestroyUnscaled>();
        destroyer.SetLifetime(clipLengthSafe);
    }

}
