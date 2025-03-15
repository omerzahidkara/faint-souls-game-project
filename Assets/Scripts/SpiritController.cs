
using UnityEngine;

public class SpiritController : MonoBehaviour, Interactable
{
    private int spiritHealthValue;
    [SerializeField] public AudioClip collectedSound;
    public void Start()
    {
        spiritHealthValue = 2;
    }

    public void Interact()
    {
        spiritHealthValue = UnityEngine.Random.Range(4, 9); // ruhçuklar 4 - 8 arasý can saðlarlar
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance.spiritNum < PlayerController.Instance.maxHealth)
        {
            PlayerController.Instance.spiritNum += spiritHealthValue;
            PlayerController.Instance.spiritNumOnTheScene--;
            PlayerController.Instance.isGatheredAnySouls = true;
            //blip!
            SoundFXManager.instance.PlaySoundFXClip(collectedSound, transform, 1f);
            // Ruh öldüðünde, pozisyonu spiritSpawnedPositions listesine ekleyebiliriz
            PlayerController.Instance.RemoveSpiritPosition(transform.position);
        }
    }
}
