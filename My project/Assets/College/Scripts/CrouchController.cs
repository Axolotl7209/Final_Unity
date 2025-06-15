using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CrouchController : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Камера")]
    [SerializeField] private GameObject _camera;
    [SerializeField] private float _cameraCrouchPosition = -0.5f;

    private CharacterController _controller;
    private float _standingHeight;
    private Vector3 _standingCenter;
    private Vector3 _originalCameraPos;
    private Vector3 _targetCameraPos;
    private bool _isCrouching;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _standingHeight = _controller.height;
        _standingCenter = _controller.center;

        if (_camera != null)
            _originalCameraPos = _camera.transform.localPosition;
    }

    private void Update()
    {
        HandleInput();
        SmoothHeightAdjustment();
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            _isCrouching = true;
            _targetCameraPos = _originalCameraPos + Vector3.up * _cameraCrouchPosition;
        }
        if (Input.GetKeyUp(crouchKey))
        {
            _isCrouching = false;
            _targetCameraPos = _originalCameraPos;
        }
    }

    private void SmoothHeightAdjustment()
    {
        float targetHeight = _isCrouching ? crouchHeight : _standingHeight;
        Vector3 targetCenter = CalculateNewCenter(targetHeight);

        _controller.height = Mathf.Lerp(
            _controller.height,
            targetHeight,
            transitionSpeed * Time.deltaTime
        );

        _controller.center = Vector3.Lerp(
            _controller.center,
            targetCenter,
            transitionSpeed * Time.deltaTime
        );

       
    }

    private void UpdateCameraPosition()
    {
        if (_camera == null) return;

        _camera.transform.localPosition = Vector3.Lerp(
            _camera.transform.localPosition,
            _targetCameraPos,
            transitionSpeed * Time.deltaTime
        );
    }

    private Vector3 CalculateNewCenter(float height)
    {
        float yOffset = (_standingHeight - height) * 0.5f;
        return new Vector3(0, _standingCenter.y - yOffset, 0);
    }

}
