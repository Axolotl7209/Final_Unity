using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolBotAI : MonoBehaviour
{
    [Header("Патрульные точки")]
    [Tooltip("Список точек для патрулирования")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Настройки движения")]
    [Tooltip("Дистанция до точки для выбора следующей")]
    [SerializeField, Min(0.1f)] private float pointReachDistance = 0.5f;

    [Header("Паузы на точках")]
    [Tooltip("Шанс (0-1), что бот остановится в точке")]
    [SerializeField, Range(0f, 1f)] private float stopChance = 0.5f;
    [Tooltip("Минимальная длительность паузы")]
    [SerializeField, Min(0f)] private float minPauseTime = 1f;
    [Tooltip("Максимальная длительность паузы")]
    [SerializeField, Min(0f)] private float maxPauseTime = 3f;

    private NavMeshAgent agent;
    private int lastPointIndex = -1;
    private bool isWaiting = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError($"{nameof(PatrolBotAI)}: Не заданы patrolPoints!", this);
            enabled = false;
        }
    }

    private void Start()
    {
        MoveToRandomPoint();
    }

    private void Update()
    {
        if (isWaiting || patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= pointReachDistance)
        {
            // Проверяем шанс остановиться
            if (Random.value < stopChance)
            {
                StartCoroutine(PauseAtPoint());
            }
            else
            {
                MoveToRandomPoint();
            }
        }
    }

    private IEnumerator PauseAtPoint()
    {
        isWaiting = true;
        float pauseTime = Random.Range(minPauseTime, maxPauseTime);
        agent.isStopped = true;
        yield return new WaitForSeconds(pauseTime);
        agent.isStopped = false;
        isWaiting = false;
        MoveToRandomPoint();
    }

    private void MoveToRandomPoint()
    {
        if (patrolPoints.Length == 0) return;

        int nextIndex;
        do
        {
            nextIndex = Random.Range(0, patrolPoints.Length);
        } while (patrolPoints.Length > 1 && nextIndex == lastPointIndex);

        lastPointIndex = nextIndex;
        agent.SetDestination(patrolPoints[nextIndex].position);
    }
}
