using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Ссылки")]
    public Slider slider;
    public Text healthText;
    public Image fillImage;

    [Header("Цвета")]
    public Color highHealthColor = Color.green;
    public Color mediumHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Пороги")]
    [Range(0f, 1f)] public float mediumThreshold = 0.5f;
    [Range(0f, 1f)] public float lowThreshold = 0.2f;

    private PlayerHealth playerHealth;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                SetMaxHealth(playerHealth.maxHealth);
            }
            else
            {
                Debug.LogError("PlayerHealth component not found on player!");
            }
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"Health changed: {currentHealth}/{maxHealth}");
        SetHealth(currentHealth);
    }

    public void SetMaxHealth(int health)
    {
        if (slider == null)
        {
            Debug.LogError("Slider reference is missing!");
            return;
        }

        slider.maxValue = health;
        slider.value = health;
        UpdateHealthText();
        UpdateColor();
    }

    public void SetHealth(int health)
    {
        if (slider == null) return;

        slider.value = health;
        UpdateColor();
        UpdateHealthText();
    }

    private void UpdateColor()
    {
        if (fillImage == null) return;

        float percentage = slider.value / slider.maxValue;

        if (percentage <= lowThreshold)
            fillImage.color = lowHealthColor;
        else if (percentage <= mediumThreshold)
            fillImage.color = mediumHealthColor;
        else
            fillImage.color = highHealthColor;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{slider.value}/{slider.maxValue}";
        }
    }
}