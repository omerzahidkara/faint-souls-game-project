
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
            DontDestroyOnLoad(gameObject); // Sahne de�i�ti�inde kaybolmas�n
        }
        else
        {
            Destroy(gameObject); // E�er zaten varsa, yeni olu�an� yok et
        }
    }
    public void KillYourself()
    {
        Destroy(gameObject); // Restart ederken kullanmak i�in
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
            clipLengthSafe = 1f; // Ses uzunlu�u al�namazsa 1 saniye varsay
        }

        AutoDestroyUnscaled destroyer = audioSource.gameObject.AddComponent<AutoDestroyUnscaled>();
        destroyer.SetLifetime(clipLengthSafe);
    }

}
