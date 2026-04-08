using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GorillaDragLaunch2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private GroundCheck2D groundCheck;

    [Header("Launch")]
    [SerializeField] private float maxDragDistance = 2.0f;
    [SerializeField] private float maxLaunchImpulse = 12.0f;
    [SerializeField] private float minLaunchDistance = 0.08f;
    [SerializeField] private float relaunchSpeedThreshold = 0.15f;
    [SerializeField] private bool requireGroundedToLaunch = true;


    private readonly List<RaycastResult> sUiHits = new List<RaycastResult>(8);

    private Rigidbody2D rb;
    private Collider2D col;

    private bool isDragging;
    private bool canLaunch = true;
    private Vector2 currentDragPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (worldCamera == null) worldCamera = Camera.main;

        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }
    }

    private void Update()
    {
        if (!canLaunch)
        {
            bool grounded = groundCheck == null || groundCheck.IsGrounded;
            if (grounded && rb.linearVelocity.magnitude <= relaunchSpeedThreshold)
            {
                canLaunch = true;
            }
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (worldCamera == null) return;
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
        Vector2 pressWorld = ScreenToWorld(mouseScreenPos);
        Collider2D hit = Physics2D.OverlapPoint(pressWorld);

        if (!canLaunch) 
        if (requireGroundedToLaunch && groundCheck != null && !groundCheck.IsGrounded)
        {
            return;
        }

        bool clickedMe = hit != null && (hit == col || hit.transform.IsChildOf(transform));

        if (clickedMe)
        {
            isDragging = true;
            currentDragPoint = pressWorld;
            ShowAimLine(true);
        }
    }

        if (!isDragging) return;

        if (Mouse.current.leftButton.isPressed)
        {
            currentDragPoint = GetClampedDragPoint(ScreenToWorld(mouseScreenPos));
            DrawAimLine(transform.position, currentDragPoint);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ReleaseLaunch();
        }
    }

    private Vector2 ScreenToWorld(Vector3 screenPos)
    {
        float zDistance = Mathf.Abs(worldCamera.transform.position.z - transform.position.z);
        screenPos.z = worldCamera.orthographic ? 0f : zDistance;

        Vector3 world = worldCamera.ScreenToWorldPoint(screenPos);
        return new Vector2(world.x, world.y);
    }


    private Vector2 GetClampedDragPoint(Vector2 rawMouseWorld)
    {
        Vector2 center = transform.position;
        Vector2 offset = rawMouseWorld - center;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        return center + offset;
    }

    private void ReleaseLaunch()
    {
        isDragging = false;
        ShowAimLine(false);

        Vector2 center = transform.position;
        Vector2 dragVector = currentDragPoint - center;
        float dragDistance = dragVector.magnitude;

        if (dragDistance < minLaunchDistance) return;

        Vector2 launchDir = -dragVector.normalized;
        float power01 = Mathf.Clamp01(dragDistance / maxDragDistance);
        float impulse = power01 * maxLaunchImpulse;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(launchDir * impulse, ForceMode2D.Impulse);

        canLaunch = false;
    }

    private void DrawAimLine(Vector2 from, Vector2 to)
    {
        if (aimLine == null) return;
        aimLine.SetPosition(0, from);
        aimLine.SetPosition(1, to);
    }

    private void ShowAimLine(bool show)
    {
        aimLine.enabled = show;
    }
}
