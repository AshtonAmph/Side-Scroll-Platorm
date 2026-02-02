using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Run Properties")]
    [SerializeField] private float maxRunSpeed = 5.0f;
    [SerializeField] private float groundAcceleration = 4.0f;
    [SerializeField] private float airAcceleration = 2.0f;
    [SerializeField] private float runForceMultiplier = 1.0f;
    [SerializeField] private float frictionFactor = 0.5f;
    [SerializeField] private float jumpPower = 10.0f;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private LayerMask groundLayers;

    private PlayerInput playerInput;
    private Rigidbody2D rb2d;
    private bool _isOnGround;
    private Vector2 _movementInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _isOnGround = CheckIfOnGround();
        _movementInput = playerInput.actions["Movement"].ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        playerInput.actions["Jump"].started += OnJumpPressed;
        playerInput.actions["Jump"].canceled += OnJumpReleased;
    }

    private void OnDisable()
    {
        playerInput.actions["Jump"].started -= OnJumpPressed;
        playerInput.actions["Jump"].canceled -= OnJumpReleased;
    }

    private bool CheckIfOnGround()
    {
        return Physics2D.OverlapBox(transform.position, groundCheckSize, 0f, groundLayers);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isOnGround ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(groundCheckSize.x, groundCheckSize.y, 0.1f));
    }

    private void FixedUpdate()
    {
        float targetSpeed, speedDiference, acceleration, runForce;

        targetSpeed = _movementInput.x * maxRunSpeed;
        speedDiference = targetSpeed - rb2d.linearVelocity.x;
        acceleration = _isOnGround ? groundAcceleration : airAcceleration;
        runForce = Mathf.Pow(Mathf.Abs(speedDiference) * acceleration, runForceMultiplier) * Mathf.Sign(speedDiference);

        rb2d.AddForce(runForce * Vector2.right);

        if (_isOnGround && Mathf.Approximately(_movementInput.x, 0))
        {
            float amount = Mathf.Min(Mathf.Abs(rb2d.linearVelocity.x), Mathf.Abs(frictionFactor));
            amount *= Mathf.Sign(rb2d.linearVelocity.x);
            rb2d.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (_isOnGround)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpPower);
        }
    }

    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        if (rb2d.linearVelocity.y > 0)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, rb2d.linearVelocity.y / 2);
        }
    }
}
