using UnityEngine;

public class JellyShotgunEnemyMover : MonoBehaviour
{
    public enum Axis { Horizontal, Vertical }

    public Axis moveAxis = Axis.Horizontal;
    public float range = 2f;
    public float speed = 2f;

    Vector3 _origin;

    void Start()
    {
        _origin = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * range;
        transform.position = moveAxis == Axis.Horizontal
            ? _origin + Vector3.right * offset
            : _origin + Vector3.up * offset;
    }
}