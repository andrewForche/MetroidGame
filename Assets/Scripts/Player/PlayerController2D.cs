using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move (Arcade/Snappy)")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    private bool jumpHeld;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.1f;      // seconds
    [SerializeField] private float jumpBufferTime = 0.1f;  // seconds

    private float coyoteTimer;
    private float jumpBufferTimer;


    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Buffer jump input
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        jumpHeld = Input.GetButton("Jump");

        // timers tick down in real time
        jumpBufferTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;

        // refresh coyote timer while grounded
        if (IsGrounded())
            coyoteTimer = coyoteTime;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // If jump was pressed recently AND we are grounded (or within coyote time), jump
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            jumpBufferTimer = 0f; // consume buffer
            coyoteTimer = 0f;     // optional: prevents double-trigger
        }

        // Variable jump height (jump cut)
        if (!jumpHeld && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
