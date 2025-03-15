
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour // oyuncu için
{
    public Slider slider;

    public Gradient gradient;
    public Image fill;

    public Text healthText; // Can miktarýný gösteren yazý

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;

        fill.color = gradient.Evaluate(1f);
    }
    public void SetHealth(int health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
        UpdateHealthText(health);
    }

    public void UpdateHealthText(int currentHealth)
    {
        healthText.text = currentHealth + " / " + slider.maxValue; // Sayýyý güncelle
    }
}
