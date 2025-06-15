using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("References")]
    [Tooltip("������ ���������� �������")]
    [SerializeField] private FpsPlayerMovement playerController;
    [Tooltip("��������� ������")]
    [SerializeField] private Transform cameraTransform;

    [Header("Bob Settings")]
    [Tooltip("�������� �����������")]
    [SerializeField] private float bobSpeed = 8f;
    [Tooltip("��������� �� Y")]
    [SerializeField] private float bobAmountY = 0.05f;
    [Tooltip("��������� �� X")]
    [SerializeField] private float bobAmountX = 0.02f;

    [Header("Strafe Tilt")]
    [Tooltip("������������ ���� ������� ������ ��� �������")]
    [SerializeField] private float tiltAngle = 5f;
    [Tooltip("�������� �������� �������")]
    [SerializeField] private float tiltSpeed = 5f;

    private Vector3 defaultLocalPos;
    private Quaternion defaultLocalRot;
    private float bobTimer;

    private void Awake()
    {
        if (cameraTransform == null)
            Debug.LogError("Camera Transform �� ��������!", this);

        defaultLocalPos = cameraTransform.localPosition;
        defaultLocalRot = cameraTransform.localRotation;
    }

    private void Update()
    {
        if (cameraTransform == null || playerController == null) return;

        HandleHeadBob();
        HandleStrafeTilt();
    }

    private void HandleHeadBob()
    {
        if (playerController.IsMoving && playerController.IsGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            float offsetY = Mathf.Sin(bobTimer) * bobAmountY;
            float offsetX = Mathf.Cos(bobTimer * 0.5f) * bobAmountX;

            cameraTransform.localPosition = defaultLocalPos + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, defaultLocalPos, Time.deltaTime * bobSpeed);
        }
    }

    private void HandleStrafeTilt()
    {
        // �������� ����������� �������� �� X (������)
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        float strafeInput = input.x; // ����� ������ ���������� Vector2: (x, y)

        float targetZRot = -strafeInput * tiltAngle;
        Quaternion targetRot = defaultLocalRot * Quaternion.Euler(0f, 0f, targetZRot);

        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, targetRot, Time.deltaTime * tiltSpeed);
    }
}
