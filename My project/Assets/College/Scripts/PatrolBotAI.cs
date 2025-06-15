using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolBotAI : MonoBehaviour
{
    [Header("���������� �����")]
    [Tooltip("������ ����� ��� ��������������")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("��������� ��������")]
    [Tooltip("��������� �� ����� ��� ������ ���������")]
    [SerializeField, Min(0.1f)] private float pointReachDistance = 0.5f;

    [Header("����� �� ������")]
    [Tooltip("���� (0-1), ��� ��� ����������� � �����")]
    [SerializeField, Range(0f, 1f)] private float stopChance = 0.5f;
    [Tooltip("����������� ������������ �����")]
    [SerializeField, Min(0f)] private float minPauseTime = 1f;
    [Tooltip("������������ ������������ �����")]
    [SerializeField, Min(0f)] private float maxPauseTime = 3f;

    private NavMeshAgent agent;
    private int lastPointIndex = -1;
    private bool isWaiting = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError($"{nameof(PatrolBotAI)}: �� ������ patrolPoints!", this);
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
            // ��������� ���� ������������
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
