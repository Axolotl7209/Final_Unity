using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [Min(1)] public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("События")]
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;

    [Header("Респаун")]
    public Transform respawnPoint;
    public float respawnDelay = 3f;

    private CharacterController characterController;
    private bool isDead = false;

    public delegate void HealthChangedDelegate(int current, int max);
    public event HealthChangedDelegate OnHealthChanged;

    // Публичное свойство для доступа к текущему здоровью
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        // Убедимся, что UI обновляется при старте
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            Debug.Log("Player is dead, can't take more damage");
            return;
        }

        // Проверка на валидность урона
        if (damage <= 0)
        {
            Debug.LogWarning($"Invalid damage value: {damage}");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        // Убедимся, что событие вызывается
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnDamageTaken?.Invoke();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");
        OnDeath?.Invoke();

        FpsPlayerMovement movement = GetComponent<FpsPlayerMovement>();
        if (movement != null) movement.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log($"Respawning in {respawnDelay} seconds...");
        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null)
        {
            characterController.enabled = false;
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
            characterController.enabled = true;
        }

        currentHealth = maxHealth;
        isDead = false;
        Debug.Log("Player respawned!");

        // Убедимся, что UI обновляется при респавне
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        FpsPlayerMovement movement = GetComponent<FpsPlayerMovement>();
        if (movement != null) movement.enabled = true;

        OnRespawn?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}