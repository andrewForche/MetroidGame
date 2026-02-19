using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    public enum AttackDir { Left, Right, Up, Down }

    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerController2D controller;

    [Header("Hitbox")]
    [SerializeField] private GameObject hitboxPrefab;

    [Tooltip("Local offsets relative to playe rposition for each direction.")]
    [SerializeField] private Vector2 offsetRight = new Vector2(0.9f, 0.0f);
    [SerializeField] private Vector2 offsetUp = new Vector2(0.0f, 0.9f);
    [SerializeField] private Vector2 offsetDown = new Vector2(0.0f, -0.9f);

    [Header("Direction Selection")]
    [SerializeField] private float verticalPriorityThreshold = 0.35f; // if |Y| above this, prefer Up/Down
    [SerializeField]  private float horizontalDeadzone = 0.10f; // ignore tiny stick drift

    [Header("Timing")]
    [SerializeField] private float attackCooldown = 0.20f;

    private float cooldownTimer;

    private void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
        if (controller == null) controller = GetComponent<PlayerController2D>();

    }

    // Update is called once per frame
    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (input != null && input.AttackPressedThisFrame && cooldownTimer <= 0f)
        {
            var dir = DetermineAttackDir();
            SpawnHitbox(dir);
            cooldownTimer = attackCooldown;
        }
    }

    private AttackDir DetermineAttackDir()
    {
        // Use current move input as "attack intent"
        Vector2 m = input != null ? input.Move : Vector2.zero;

        // Prefer Up/Down if player is clearly pressing vertical
        if (Mathf.Abs(m.y) >= verticalPriorityThreshold)
            return (m.y > 0f) ? AttackDir.Up : AttackDir.Down;
        
        // Otherwise, Left/Right:
        // If player is pressing horizontal, use that. If not, use facing.
        if (Mathf.Abs(m.x)> horizontalDeadzone)
            return (m.x > 0f) ? AttackDir.Right : AttackDir.Left;

        // Default
        int facing = controller != null ? controller.Facing : 1;
        return (facing >= 0) ? AttackDir.Right : AttackDir.Left;
    }

    private void SpawnHitbox(AttackDir dir)
    {
        if (hitboxPrefab == null) return;

        Vector2 offset = dir switch
        {
            AttackDir.Up => offsetUp,
            AttackDir.Down => offsetDown,
            AttackDir.Left => new Vector2(-offsetRight.x, offsetRight.y),
            _ => offsetRight,
        };

        Vector3 spawnPos = transform.position + (Vector3)offset;

        // Rotate the hitbox so its "forward" matches direction (optional but nice)
        Quaternion rot = dir switch
        {
            AttackDir.Up => Quaternion.Euler(0, 0, 90),
            AttackDir.Down => Quaternion.Euler(0, 0, -90),
            AttackDir.Left => Quaternion.Euler(0, 0, 180),
            _ => Quaternion.identity
        };

        Instantiate(hitboxPrefab, spawnPos, rot);
    }   
}
