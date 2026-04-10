using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool canMove = false;
    [SerializeField] private bool canJump = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector2 moveInput;


    private Rigidbody2D _rigid;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        StageMove.OnMove += HandleMove;
    }

    private void OnDisable()
    {
        StageMove.OnMove -= HandleMove;
    }

    private void HandleMove(bool active)
    {
        Debug.Log("[Player] Move " + active);

        canMove = active;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            _rigid.linearVelocity = new Vector2(0f, _rigid.linearVelocityY);
        }
    }


    private void FixedUpdate()
    {
        Debug.Log("[Player] Move Input " + moveInput);
        if (canMove) _rigid.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigid.linearVelocity.y);
        else _rigid.linearVelocity = Vector2.zero;
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
