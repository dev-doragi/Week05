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

    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;
    private bool _isGrounded = false;

    private Rigidbody2D _rigid;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        StageMove.OnMove += HandleMove;
        StageScriptJump.OnJump += HandleJump;
        StageMakeTrampoline.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        StageMove.OnMove -= HandleMove;
        StageScriptJump.OnJump -= HandleJump;
        StageMakeTrampoline.OnJump -= HandleJump;
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

    private void HandleJump(bool active)
    {
        Debug.Log("[Player] Jump " + active);
        canJump = active;
        canMove = active;
    }

    private void FixedUpdate()
    {
        if (canMove) _rigid.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigid.linearVelocity.y);
        else _rigid.linearVelocity = new Vector2(0f, _rigid.linearVelocity.y);
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnJump()
    {
        if (!canJump || !_isGrounded) return;

        _isGrounded = false;
        _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, 0f);
        _rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _isGrounded = true;
    }
}