using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public Vector2 MoveInput { get; private set; }

    public bool IsJumpHeld { get; private set; }

    private bool jumpQueued = false;

    public bool JumpPressed
    {
        get
        {
            if (jumpQueued)
            {
                jumpQueued = false;
                return true;
            }
            return false;
        }
    }

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpQueued = true;
        inputActions.Player.Jump.started += ctx => IsJumpHeld = true;
        inputActions.Player.Jump.canceled += ctx => IsJumpHeld = false;
    }


    public Vector3 GetWorldDirection(Camera cam)
    {
        Vector3 input = new Vector3(MoveInput.x, 0, MoveInput.y);
        input = Quaternion.Euler(0, 45, 0) * input;
        return input.normalized;
    }

    private void OnDestroy()
    {
        inputActions.Player.Move.performed -= ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled -= ctx => MoveInput = Vector2.zero;
        inputActions.Player.Jump.performed -= ctx => jumpQueued = true;
    }
}
