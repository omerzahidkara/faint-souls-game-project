using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip hoverSound; // Inspector’dan atanacak
    private SoundFXManager sfXManager;

    private void Start()
    {
        sfXManager = FindObjectOfType<SoundFXManager>();//sahnedeki sfx objeyi bulur
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            sfXManager.PlaySoundFXClip(hoverSound,transform, 1f);
        }
    }
}
