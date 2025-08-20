using UnityEngine;

public class HeartEffect : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifetime = 1.5f;
    public float scaleShrinkSpeed = 1f;
    public float swayAmount = 0.2f;
    public float swaySpeed = 3f;

    private Vector3 initialScale;
    private float timeElapsed;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        // Yükselme
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Sway (saða sola)
        float sway = Mathf.Sin(timeElapsed * swaySpeed) * swayAmount;
        transform.position += Vector3.right * sway * Time.deltaTime;

        // Küçülme
        float scaleFactor = Mathf.Lerp(1f, 0f, timeElapsed / lifetime);
        transform.localScale = initialScale * scaleFactor;

        // Süre dolunca yok et
        if (timeElapsed >= lifetime)
            Destroy(gameObject);
    }
}

