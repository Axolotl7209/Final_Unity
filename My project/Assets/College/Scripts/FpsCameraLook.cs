using UnityEngine;

public class FpsCameraLook : MonoBehaviour
{
    [Header("��������� ������")]
    [Tooltip("���������������� ����")]
    [SerializeField] private float sensitivity = 2f;

    [Tooltip("������������ ���� ���������� �����/����")]
    [SerializeField, Range(10f, 90f)] private float verticalClamp = 80f;

    [Tooltip("������������� ��� Y")]
    [SerializeField] private bool invertY = false;

    [Header("������")]
    [Tooltip("������ ������ (�������� ������)")]
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * (invertY ? -1 : 1);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
