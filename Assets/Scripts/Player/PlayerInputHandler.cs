using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler: MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool AttackPressedThisFrame { get; private set; }


    private void LateUpdate() {
        JumpPressedThisFrame = false;
        AttackPressedThisFrame = false;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started) JumpHeld = true;
        if (ctx.canceled) JumpHeld = false;

        if (ctx.performed)
            JumpPressedThisFrame = true;
    }

    public void OnAttack(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            AttackPressedThisFrame = true;
    }
}
