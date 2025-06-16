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

    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Проверка наличия коллайдера
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("Player is missing a Collider component!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            Debug.Log("Player is dead, can't take more damage");
            return;
        }

        if (damage <= 0)
        {
            Debug.LogWarning($"Invalid damage value: {damage}");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

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

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

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

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        FpsPlayerMovement movement = GetComponent<FpsPlayerMovement>();
        if (movement != null) movement.enabled = true;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = true;

        OnRespawn?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}