using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZzzEffect : MonoBehaviour
{
    public float floatSpeed = 0.75f;
    public float lifetime = 1f;
    public float scaleShrinkSpeed = 1f;
    public float swayAmount = 1.0f;
    public float swaySpeed = 4.5f;

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
