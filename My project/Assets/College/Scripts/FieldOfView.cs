using UnityEngine;

public class FieldOfViewVisualizer : MonoBehaviour
{
    [Header("Настройки")]
    [Range(1, 50)] public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    [Range(10, 100)] public int raycastQuality = 50;

    [Header("Цвета")]
    public Color gizmosColor = Color.yellow;
    public Color detectedColor = Color.red;
    public Color raycastHitColor = Color.green;
    public Color raycastMissColor = Color.gray;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
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
            if (Physics.Raycast(origin, dir, out hit, viewRadius))
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