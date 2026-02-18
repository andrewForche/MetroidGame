// PlayerController2D.cs
//
// This version is designed to work with the **New Input System** via a separate
// PlayerInputHandler component (clean architecture).
//
// MAJOR CHANGES vs legacy-input versions:
// 1) ✅ Removed all Input.GetAxisRaw / Input.GetButton* polling.
//    - We now read input from `PlayerInputHandler` (fed by PlayerInput events).
// 2) ✅ Input forgiveness is handled with timers:
//    - jump buffer (press slightly early still jumps)
//    - coyote time (press slightly late after leaving a ledge still jumps)
// 3) ✅ “Missed input during editor hitches” protection:
//    - We refresh jump buffer on JumpPressedThisFrame OR while JumpHeld.
//      (So holding jump during a hitch still triggers the jump when frames resume.)
// 4) ✅ Snappy arcade movement:
//    - Horizontal velocity is set directly each physics tick.
//
// NOTE: This script assumes you have:
// - Rigidbody2D + Collider2D on the Player
// - A child Transform assigned as groundCheck
// - groundLayer set to your Ground layer
// - PlayerInputHandler component on the same GameObject

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input; // New Decoupled input source

    [Header("Move (Arcade/Snappy)")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 12f;

    [Header("Jump Feel")]
    [SerializeField] private float jumpCutMultiplier = 0.5f;  // tap = short hop, hold = full jump
    [SerializeField] private float coyoteTime = 0.10f;        // seconds after leaving ledge
    [SerializeField] private float jumpBufferTime = 0.12f;    // seconds before landing

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.15f);
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    // Cached input/state
    private float moveInput;    // -1, 0, +1 for snappy movement
    private bool jumpHeld;

    // Timers (core of buffer/coyote)
    private float coyoteTimer;
    private float jumpBufferTimer;
    
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        // New: auto-find input handler if not assigned in Inspector
        if (input == null)
            input = GetComponent<PlayerInputHandler>();

        // Helpful warnings
        if (input == null)
            Debug.LogWarning("PlayerInputHandler not found. Add PlayerInputHandler + PlayerInput (Invoke Unity Events).");
        
        if (groundCheck == null)
            Debug.LogWarning("GroundCheck is not assigned. Create an empty child named GroundCheck and assign it.");

        // Recommended for smooth camera follow with physics-driven movement
        // (You can also set this in the Inspector.)
        // rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        // --- INPUT (NEW SYSTEM) ---
        // We read already-processed values from PlayerInputHandler.
        // That handler is filled by PlayerInput events from the Input Actions asset.

        if (input != null)
        {
            // Snappy: convert analog stick / keys to -1, 0, +1
            // Add a tiny deadzone so controller drift doesn't cause movement.
            float x = input.Move.x;
            moveInput = Mathf.Abs(x) > 0.10f ? Mathf.Sign(x) : 0f;

            jumpHeld = input.JumpHeld;

            // NEW: Jump buffering that survives editor hitching
            // - If jump was pressed this frame OR jump is currently held, refresh buffer timer.
            // This prevents missing "tap" jumps during frame spikes and makes input feel consistent.
            if (input.JumpPressedThisFrame || jumpHeld)
                jumpBufferTimer = jumpBufferTime;

            if (input.Move != Vector2.zero || jumpHeld || input.JumpPressedThisFrame)
            Debug.Log($"Move={input.Move} JumpHeld={jumpHeld} Pressed={input.JumpPressedThisFrame}");
        }

        // --- COYOTE TIME / BUFFER TIMERS ---
        // We update these timers in Update() using deltaTime (feel timers).
        // This keeps behavior consistent even if FixedUpdate timing shifts a bit.

        if (IsGrounded())
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        jumpBufferTimer -= Time.deltaTime;        
    }

    private void FixedUpdate()
    {
        // --- HORIZONTAL MOVEMENT (SNAPPY) ---
        // Directly set X velocity each physics step.
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // --- JUMP (BUFFER + COYOTE) ---
        // If jump was pressed recently AND we're grounded (or within coyote time), jump.
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

            // Consume timers so you don't get repeated jumps from one buffered press
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // --- VARIABLE JUMP HEIGHT (JUMP CUT) ---
        // If player releases jump while still moving upward, cut upward velocity.
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