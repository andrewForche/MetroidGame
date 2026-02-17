using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move (Arcade/Snappy)")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 12f;

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
        // -1, 0, 1 (snappy)
        moveInput = Input.GetAxisRaw("Horizontal");

        // capture in Update so we don't miss it
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
    }

    private void FixedUpdate()
    {
        // Set X velocity directly for arcade feel
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jump
        if (jumpPressed && IsGrounded())
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

        jumpPressed = false;
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
