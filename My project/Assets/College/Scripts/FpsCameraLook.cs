using UnityEngine;

public class FpsCameraLook : MonoBehaviour
{
    [Header("Настройки обзора")]
    [Tooltip("Чувствительность мыши")]
    [SerializeField] private float sensitivity = 2f;

    [Tooltip("Максимальный угол отклонения вверх/вниз")]
    [SerializeField, Range(10f, 90f)] private float verticalClamp = 80f;

    [Tooltip("Инвертировать ось Y")]
    [SerializeField] private bool invertY = false;

    [Header("Ссылки")]
    [Tooltip("Объект игрока (родитель камеры)")]
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
