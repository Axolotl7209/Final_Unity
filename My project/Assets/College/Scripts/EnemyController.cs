using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Патрулирование")]
    public Transform[] patrolPoints;
    [Min(0.1f)] public float pointReachDistance = 0.5f;
    [Range(0f, 1f)] public float stopChance = 0.5f;
    [Min(0f)] public float minPauseTime = 1f;
    [Min(0f)] public float maxPauseTime = 3f;

    [Header("Обнаружение игрока")]
    [Min(1f)] public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask targetMask;
    [Range(0.1f, 1f)] public float scanFrequency = 0.3f;

    [Header("Преследование")]
    [Min(1f)] public float patrolSpeed = 3f;
    [Min(1f)] public float chaseSpeed = 5f;
    [Min(1f)] public float rotationSpeed = 120f;

    [Header("Атака")]
    [Min(0.5f)] public float attackRange = 2f;
    [Min(0.1f)] public float attackCooldown = 1.5f;
    [Min(1)] public int attackDamage = 10;

    [Header("Анимации")]
    public string moveAnimParam = "IsMoving";
    public string attackAnimParam = "Attack";
    public string idleAnimParam = "IsIdle";

    [Header("Здоровье")]
    [Min(1)] public int maxHealth = 100;
    [SerializeField] private int currentHealth;
    public HealthBar healthBar;

    [Header("Атака лучом")]
    [Min(0.1f)] public float attackRayDistance = 15f;
    [Min(0.1f)] public float attackRayWidth = 0.5f;  // Увеличенная ширина для надежности
    public LayerMask attackRayMask;
    public GameObject attackHitEffect;
    public Transform attackRayOrigin;

    private NavMeshAgent agent;
    private Animator animator;
    private LineRenderer attackRay;
    private float attackRayDuration = 0.1f;
    private enum State { Patrol, Chase, Attack }
    private State currentState;
    private Transform playerTarget;
    private int currentPatrolIndex = 0;
    private bool isPausing;
    private float lastScanTime;
    private float lastAttackTime;
    private List<Vector3> patrolPositions = new List<Vector3>();
    private bool hasPatrolPath;
    private bool hasLineOfSight;
    private float lastTargetSeenTime;
    private float targetMemoryDuration = 4f;
    private PlayerHealth playerHealthComponent;
    private bool isAttacking = false;  // Флаг для отслеживания состояния атаки
    private float attackStartTime;     // Время начала атаки
    private float attackAnimationTime = 0.5f; // Длительность анимации атаки

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = patrolSpeed;
        agent.stoppingDistance = attackRange * 0.9f;

        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);

        attackRay = GetComponentInChildren<LineRenderer>();
        if (attackRay != null) attackRay.enabled = false;
    }

    private void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null) patrolPositions.Add(point.position);
            }
        }

        if (patrolPositions.Count > 0)
        {
            SetState(State.Patrol);
            MoveToNextPatrolPoint();
        }
        else
        {
            Debug.LogWarning("No valid patrol points assigned! Enemy will remain idle.", this);
        }

        // Находим компонент здоровья игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealthComponent = player.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        HandleTargetDetection();

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrolState();
                break;

            case State.Chase:
                UpdateChaseState();
                break;

            case State.Attack:
                UpdateAttackState();
                break;
        }

        UpdateAnimation();
    }

    private void UpdatePatrolState()
    {
        if (patrolPositions.Count == 0 || !hasPatrolPath || isPausing) return;

        if (HasReachedDestination(patrolPositions[currentPatrolIndex]))
        {
            if (Random.value < stopChance)
            {
                StartCoroutine(PauseRoutine());
            }
            else
            {
                MoveToNextPatrolPoint();
            }
        }
    }

    private void UpdateChaseState()
    {
        if (Time.time - lastTargetSeenTime > targetMemoryDuration)
        {
            ReturnToPatrol();
            return;
        }

        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= attackRange && hasLineOfSight)
            {
                SetState(State.Attack);
            }
        }
    }

    private void UpdateAttackState()
    {
        if (playerTarget == null || !hasLineOfSight)
        {
            SetState(State.Chase);
            return;
        }

        // Остановка врага для атаки
        agent.isStopped = true;

        // Поворот к цели
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // Проверка дистанции
        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance > attackRange * 1.2f)
        {
            agent.isStopped = false;
            SetState(State.Chase);
            return;
        }

        // Запуск атаки
        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            StartAttack();
        }

        // Обработка текущей атаки
        if (isAttacking)
        {
            if (Time.time - attackStartTime > attackAnimationTime)
            {
                CompleteAttack();
            }
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackStartTime = Time.time;
        lastAttackTime = Time.time;

        // Запуск анимации атаки
        animator.SetTrigger(attackAnimParam);

        // Начало визуализации луча
        StartCoroutine(ShowAttackRay());

        // Непосредственно нанесение урона
        PerformAttack();
    }

    private void CompleteAttack()
    {
        isAttacking = false;
        agent.isStopped = false;

        // Проверка, нужно ли продолжать атаку
        if (playerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= attackRange * 1.2f && hasLineOfSight)
            {
                SetState(State.Attack);
            }
            else
            {
                SetState(State.Chase);
            }
        }
        else
        {
            SetState(State.Patrol);
        }
    }

    private void ReturnToPatrol()
    {
        SetState(State.Patrol);

        if (patrolPositions.Count > 0)
        {
            if (HasReachedDestination(patrolPositions[currentPatrolIndex]))
            {
                MoveToNextPatrolPoint();
            }
            else
            {
                agent.SetDestination(patrolPositions[currentPatrolIndex]);
                hasPatrolPath = true;
            }
        }
    }

    private void HandleTargetDetection()
    {
        if (Time.time - lastScanTime < scanFrequency) return;
        lastScanTime = Time.time;

        Transform newTarget = ScanForTarget();
        hasLineOfSight = (newTarget != null);

        if (newTarget != null)
        {
            playerTarget = newTarget;
            lastTargetSeenTime = Time.time;

            if (currentState == State.Patrol)
            {
                SetState(State.Chase);
            }
        }
        else if (playerTarget != null && (currentState == State.Chase || currentState == State.Attack))
        {
            if (Time.time - lastTargetSeenTime > targetMemoryDuration)
            {
                playerTarget = null;
            }
        }
    }

    private Transform ScanForTarget()
    {
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        foreach (Collider target in targetsInView)
        {
            Transform targetTransform = target.transform;
            Vector3 dirToTarget = (targetTransform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, targetTransform.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    return targetTransform;
                }
            }
        }
        return null;
    }

    private void UpdateAnimation()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f && !isPausing;

        animator.SetBool(moveAnimParam, currentState == State.Chase && isMoving);
        animator.SetBool(idleAnimParam, currentState == State.Patrol && !isMoving);
    }

    private void SetState(State newState)
    {
        // Выход из предыдущего состояния
        switch (currentState)
        {
            case State.Patrol:
                agent.isStopped = false;
                StopAllCoroutines();
                isPausing = false;
                break;

            case State.Chase:
                agent.speed = patrolSpeed;
                break;

            case State.Attack:
                agent.isStopped = false;
                isAttacking = false;
                break;
        }

        // Вход в новое состояние
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                break;

            case State.Attack:
                // agent.isStopped будет установлен в UpdateAttackState
                break;
        }
    }

    private IEnumerator PauseRoutine()
    {
        isPausing = true;
        agent.isStopped = true;

        float pauseTime = Random.Range(minPauseTime, maxPauseTime);
        yield return new WaitForSeconds(pauseTime);

        agent.isStopped = false;
        isPausing = false;
        MoveToNextPatrolPoint();
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPositions.Count == 0)
        {
            hasPatrolPath = false;
            return;
        }

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPositions.Count;
        agent.SetDestination(patrolPositions[currentPatrolIndex]);
        hasPatrolPath = true;
    }

    private bool HasReachedDestination(Vector3 targetPosition)
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPos = new Vector2(targetPosition.x, targetPosition.z);
        return Vector2.Distance(currentPos, targetPos) <= pointReachDistance;
    }

    private void PerformAttack()
    {
        if (playerTarget == null || playerHealthComponent == null)
        {
            Debug.Log("Attack failed: player target missing");
            return;
        }

        Vector3 attackDirection = (playerTarget.position - attackRayOrigin.position).normalized;
        float distanceToPlayer = Vector3.Distance(attackRayOrigin.position, playerTarget.position);

        // Проверка попадания с помощью Raycast
        RaycastHit hit;
        if (Physics.Raycast(
            attackRayOrigin.position,
            attackDirection,
            out hit,
            distanceToPlayer,
            attackRayMask))
        {
            Debug.DrawRay(attackRayOrigin.position, attackDirection * hit.distance, Color.red, 2f);

            if (hit.collider.CompareTag("Player"))
            {
                playerHealthComponent.TakeDamage(attackDamage);
                Debug.Log($"Player hit! Damage: {attackDamage}");
            }
            else
            {
                Debug.Log($"Hit: {hit.collider.name}");
            }

            if (attackHitEffect != null)
            {
                Instantiate(attackHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            Debug.DrawRay(attackRayOrigin.position, attackDirection * distanceToPlayer, Color.green, 2f);
            Debug.Log("Attack missed!");
        }
    }

    private IEnumerator ShowAttackRay()
    {
        if (attackRay == null) yield break;

        attackRay.enabled = true;
        attackRay.SetPosition(0, attackRayOrigin.position);

        Vector3 endPoint = attackRayOrigin.position + attackRayOrigin.forward * attackRayDistance;
        if (playerTarget != null)
        {
            endPoint = playerTarget.position;
        }

        attackRay.SetPosition(1, endPoint);

        yield return new WaitForSeconds(attackRayDuration);

        attackRay.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentState == State.Patrol && playerTarget != null)
        {
            SetState(State.Chase);
        }
    }

    private void Die()
    {
        enabled = false;
        agent.enabled = false;
        animator.SetTrigger("Die");
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackRayOrigin != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 direction = attackRayOrigin.forward * attackRayDistance;
            Gizmos.DrawRay(attackRayOrigin.position, direction);
        }

        Gizmos.color = playerTarget != null ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in patrolPoints)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.3f);
            }
        }

        if (hasPatrolPath && patrolPositions.Count > 0 && currentPatrolIndex < patrolPositions.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolPositions[currentPatrolIndex], 0.5f);
            Gizmos.DrawLine(transform.position, patrolPositions[currentPatrolIndex]);
        }
    }
}