using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }
    public enum MoveDirection { Plus = 1, Minus = -1 }

    [Header("Movement")]
    public MoveAxis axis = MoveAxis.Vertical;
    public MoveDirection direction = MoveDirection.Plus;
    public bool canMove = true;

    [Space(5), Min(0f)]
    public float distance = 3f;   // 얼마나 멀리 이동할지
    public float speed = 2f;

    private Rigidbody2D rb;
    private Vector2 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb != null ? rb.position : (Vector2)transform.position;
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (!canMove) return;

        // 방향 설정
        Vector2 axisDir = axis == MoveAxis.Vertical ? Vector2.up : Vector2.right;
        axisDir *= (int)direction;

        float offset = Mathf.PingPong(Time.time * speed, distance);

        Vector2 newPos = startPos + axisDir * offset;
        rb.MovePosition(newPos);
    }


    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();

        Vector3 basePos = Application.isPlaying && rb != null
            ? (Vector3)startPos
            : transform.position;

        Vector3 axisDir = axis == MoveAxis.Vertical ? Vector3.up : Vector3.right;
        axisDir *= (int)direction;

        Vector3 endPos = basePos + axisDir * distance;

        // 이동 경로
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(basePos, endPos);

        // 시작점 / 도착점
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(basePos, 0.05f);
        Gizmos.DrawSphere(endPos, 0.05f);


        // 콜라이더 크기만큼 박스 표시
        if (col != null)
        {
            Vector3 centerOffset = col.bounds.center - transform.position;
            Vector3 size = col.bounds.size;

            Gizmos.DrawWireCube(basePos + centerOffset, size);
            Gizmos.DrawWireCube(endPos + centerOffset, size);
        }
    }
}