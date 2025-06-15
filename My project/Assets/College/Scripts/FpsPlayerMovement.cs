using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FpsPlayerMovement : MonoBehaviour
{
    [Header("�������� ���������")]
    [Tooltip("�������� ������ ���������")]
    
    public float walkSpeed = 6f; 
    public void ResetSpeed() => walkSpeed = 6f; // ������� ��������
    [Tooltip("�������� ���� ��� ��������� Shift")]
    [SerializeField] private float runSpeed = 12f;

    [Tooltip("���� ������")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("���������� (��������� �������)")]
    [SerializeField, Min(0f)] private float gravity = 20f;

    [Header("��������� ����������")]
    [Tooltip("������� ����")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Tooltip("������� ������")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;


    [Header("����� ���������������� ��������")]
    [Tooltip("����������� �������� ��� ������������� ��������")]
    [SerializeField, Min(0f)] private float moveDeadzone = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;

   
    public bool IsMoving
    {
        get
        {
            Vector2 input = GetMoveInput();
            return input.sqrMagnitude > moveDeadzone * moveDeadzone;
        }
    }

    public bool IsGrounded => controller != null && controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        velocity.x = move.x * speed;
        velocity.z = move.z * speed;

        if (controller.isGrounded)
        {
            velocity.y = -1f;
            if (Input.GetKeyDown(jumpKey))
                velocity.y = jumpForce;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

  
    public Vector2 GetMoveInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

}
