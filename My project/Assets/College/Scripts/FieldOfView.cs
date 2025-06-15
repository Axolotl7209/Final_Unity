using UnityEngine;
using UnityEngine.Events;

public class FieldOfView : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField, Range(1, 50)] private float viewRadius = 10f;
    [SerializeField, Range(0, 360)] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask targetMask;
    [SerializeField, Range(10, 100)] private int raycastQuality = 50;

    [Header("События")]
    public UnityEvent<Transform> OnTargetDetected;
    public UnityEvent OnTargetLost;

    [Header("Дебаг")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmosColor = Color.yellow;
    [SerializeField] private Color detectedColor = Color.red;
    [SerializeField] private Color raycastHitColor = Color.green;
    [SerializeField] private Color raycastMissColor = Color.gray;

    public Transform VisibleTarget { get; private set; }
    private bool hadTargetLastFrame = false;

    private void Update()
    {
        ScanForTargets();
        DebugCheck();
    }

    private void ScanForTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        Transform detected = null;
        foreach (var targetCollider in targetsInViewRadius)
        {
            Transform target = targetCollider.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

            if (angleToTarget < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                RaycastHit hit;
                // Луч ограничивается obstacleMask
                if (Physics.Raycast(transform.position, dirToTarget, out hit, dstToTarget, obstacleMask))
                {
                    // Если луч задет препятствием, цель не видна
                    if (hit.transform != target)
                        continue;
                }
                detected = target;
                break; // Берём первого увиденного
            }
        }

        if (detected != VisibleTarget)
        {
            if (detected != null)
            {
                VisibleTarget = detected;
                OnTargetDetected?.Invoke(VisibleTarget);
            }
            else if (VisibleTarget != null)
            {
                VisibleTarget = null;
                OnTargetLost?.Invoke();
            }
        }
    }

    private void DebugCheck()
    {
        if (VisibleTarget != null && !hadTargetLastFrame)
        {
            Debug.Log($"Target detected: {VisibleTarget.name}", this);
            hadTargetLastFrame = true;
        }
        else if (VisibleTarget == null && hadTargetLastFrame)
        {
            Debug.Log("Target lost", this);
            hadTargetLastFrame = false;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = VisibleTarget != null ? detectedColor : gizmosColor;
        DrawFieldOfView();

        if (VisibleTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, VisibleTarget.position);
        }
    }

    private void DrawFieldOfView()
    {
        Vector3 origin = transform.position;
        float angleStep = viewAngle / raycastQuality;
        float startAngle = transform.eulerAngles.y - viewAngle / 2f;

        Vector3 prevPoint = origin + DirFromAngle(startAngle) * viewRadius;
        for (int i = 1; i <= raycastQuality; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = DirFromAngle(angle);
            Vector3 point = origin + dir * viewRadius;

            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, viewRadius, obstacleMask))
            {
                Gizmos.color = raycastHitColor;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.1f);
                point = hit.point;
            }
            else
            {
                Gizmos.color = raycastMissColor;
                Gizmos.DrawLine(origin, point);
            }

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees)
    {
        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }
#endif
}
