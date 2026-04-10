using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }

    [Header("이동 설정")]
    public MoveAxis axis = MoveAxis.Vertical;
    public float distance = 3f;   // 얼마나 멀리 이동할지
    public float speed = 2f;

    private Rigidbody2D rb;
    private Vector2 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        float offset = Mathf.Sin(Time.time * speed) * distance;

        Vector2 newPos = axis == MoveAxis.Vertical
            ? new Vector2(startPos.x, startPos.y + offset)
            : new Vector2(startPos.x + offset, startPos.y);

        rb.MovePosition(newPos);
    }
}