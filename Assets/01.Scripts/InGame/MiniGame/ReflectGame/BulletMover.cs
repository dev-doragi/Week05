using UnityEngine;

public class BulletMover : MonoBehaviour
{
    public float speed = 5f;

    Vector2 _dir = Vector2.left;

    public void Launch(Vector2 dir)
    {
        _dir = dir.normalized;
        gameObject.SetActive(true);
    }

    void Update()
    {
        transform.Translate(_dir * speed * Time.deltaTime);
    }
}