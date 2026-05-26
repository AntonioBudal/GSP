using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Atributos Físicos")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Visuais")]
    [SerializeField] private Transform _characterVisual;

    private Rigidbody2D _rb;
    private InputAction _moveAction;

    private float _moveInput;
    private bool _facingRight = true;

    private static readonly Vector3 RightScale = new Vector3(1f, 1f, 1f);
    private static readonly Vector3 LeftScale  = new Vector3(-1f, 1f, 1f);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_characterVisual == null)
        {
            Debug.LogError($"[{nameof(PlayerMovement)}] _characterVisual não atribuído.", this);
            enabled = false;
            return;
        }

        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Negative", "<Gamepad>/dpad/left")
            .With("Negative", "<Gamepad>/leftStick/x")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow")
            .With("Positive", "<Gamepad>/dpad/right")
            .With("Positive", "<Gamepad>/leftStick/x");
    }

    private void OnEnable() => _moveAction.Enable();
    private void OnDisable() => _moveAction.Disable();

    private void Update()
    {
        _moveInput = _moveAction.ReadValue<float>();

        if (_moveInput > 0.01f)
            SetFacing(true);
        else if (_moveInput < -0.01f)
            SetFacing(false);
    }

    private void FixedUpdate()
    {
        // Aplica a física estritamente no eixo X, preservando a gravidade no eixo Y
        _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed, _rb.linearVelocity.y);
    }

    private void SetFacing(bool faceRight)
    {
        if (_facingRight == faceRight) return;

        _facingRight = faceRight;
        _characterVisual.localScale = faceRight ? RightScale : LeftScale;
    }
}