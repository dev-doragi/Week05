using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    private LineRenderer _lr;

    void Awake()
    {
        _lr = gameObject.AddComponent<LineRenderer>();
        _lr.positionCount = 2;
        _lr.startWidth = 0.06f;
        _lr.endWidth = 0.06f;
        _lr.material = new Material(Shader.Find("Sprites/Default"));
        _lr.sortingOrder = 5;
        _lr.useWorldSpace = true;
    }

    public void SetColor(Color c)
    {
        _lr.startColor = c;
        _lr.endColor = c;
    }

    public void SetStart(Vector3 start)
    {
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, start);
    }

    public void UpdateEnd(Vector3 end) => _lr.SetPosition(1, end);

    public void SnapEnd(Vector3 end) => _lr.SetPosition(1, end);
}