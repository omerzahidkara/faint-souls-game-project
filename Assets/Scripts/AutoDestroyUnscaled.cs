using UnityEngine;

public class AutoDestroyUnscaled : MonoBehaviour
{
    private float lifetime;
    private float timer = 0f;

    public void SetLifetime(float seconds)
    {
        lifetime = seconds > 0f ? seconds : 1f; // 0 veya negatifse varsayýlan 1 saniye
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
