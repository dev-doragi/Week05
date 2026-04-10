using System;
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

        StageMove.OnMove += HandleMove;
    }

    private void OnDisable()
    {
        StageMove.OnMove -= HandleMove;
    }

    private void HandleMove(bool active)
    {
        Debug.Log("[Player] Move " + active);

        canMove = !active;
    }


    private void FixedUpdate()
    {
        if (canMove) _rigid.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigid.linearVelocity.y);
        else _rigid.linearVelocity = Vector2.zero;
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
