using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimatorConnectGame : MiniGame
{
    [Header("프리팹")]
    public AnimatorConnectNode leftNodePrefab;
    public AnimatorConnectNode rightNodePrefab;
    public ConnectionLine linePrefab;

    [Header("노드 데이터")]
    public AnimatorConnectData[] nodeDataSet;

    [Header("레이아웃 설정")]
    public float nodeSpacingY = 1.8f;
    public float leftX = -3f;
    public float rightX = 3f;
    public float startY = 0f;

    [Header("히트 판정 반경 (월드 유닛)")]
    public float hitRadius = 0.4f;

    private static readonly Color[] ColorPool =
    {
        Color.red,
        new Color(1f, 0.85f, 0f),
        new Color(0.1f, 0.75f, 0.1f),
        new Color(0.3f, 0.6f, 1f),
        new Color(0.9f, 0.4f, 1f),
        new Color(1f, 0.5f, 0f),
    };

    private List<AnimatorConnectPoint> _leftPoints = new();
    private List<AnimatorConnectPoint> _rightPoints = new();
    private List<ConnectionLine> _lines = new();
    private List<GameObject> _spawnedObjects = new();
    private int _connectedCount;

    private AnimatorConnectPoint _draggingFrom;
    private ConnectionLine _draggingLine;

    // ── MiniGame override ─────────────────────────────────

    protected override void OnStart()
    {
        Cleanup();
        SpawnNodes();
    }

    // ── Update 드래그 처리 ────────────────────────────────

    void Update()
    {
        if (!gameObject.activeSelf) return;

        Vector3 mouseWorld = GetMouseWorld();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            AnimatorConnectPoint clicked = FindLeftPointAt(mouseWorld);
            if (clicked != null && clicked.linkedPoint == null)
            {
                _draggingFrom = clicked;
                _draggingLine = CreateDraggingLine(clicked, clicked.transform.position);
            }
        }

        if (Mouse.current.leftButton.isPressed && _draggingLine != null)
        {
            _draggingLine.UpdateEnd(mouseWorld);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (_draggingFrom != null && _draggingLine != null)
            {
                AnimatorConnectPoint target = FindRightPointAt(mouseWorld);

                if (target != null
                    && target.matchColor == _draggingFrom.matchColor
                    && target.linkedPoint == null)
                {
                    Connect(_draggingFrom, target, _draggingLine);
                }
                else
                {
                    DestroyLine(_draggingLine);
                }
            }

            _draggingFrom = null;
            _draggingLine = null;
        }
    }

    // ── 스폰 ──────────────────────────────────────────────

    void SpawnNodes()
    {
        int count = nodeDataSet.Length;
        int[] leftOrder = ShuffledIndices(count);
        int[] rightOrder = ShuffledIndices(count);

        float totalHeight = (count - 1) * nodeSpacingY;
        float topY = startY + totalHeight / 2f;

        for (int i = 0; i < count; i++)
        {
            Color color = ColorPool[i % ColorPool.Length];

            float leftY = topY - leftOrder[i] * nodeSpacingY;
            float rightY = topY - rightOrder[i] * nodeSpacingY;

            var leftNode = Instantiate(leftNodePrefab, transform);
            leftNode.transform.localPosition = new Vector3(leftX, leftY, 0f);
            leftNode.Init(this, nodeDataSet[i].leftText, color, PointSide.Left);
            _leftPoints.Add(leftNode.point);
            _spawnedObjects.Add(leftNode.gameObject);

            var rightNode = Instantiate(rightNodePrefab, transform);
            rightNode.transform.localPosition = new Vector3(rightX, rightY, 0f);
            rightNode.Init(this, nodeDataSet[i].rightText, color, PointSide.Right);
            _rightPoints.Add(rightNode.point);
            _spawnedObjects.Add(rightNode.gameObject);
        }
    }

    // ── 내부 유틸 ─────────────────────────────────────────

    AnimatorConnectPoint FindLeftPointAt(Vector3 worldPos)
    {
        foreach (var lp in _leftPoints)
        {
            if (Vector3.Distance(lp.transform.position, worldPos) < hitRadius)
                return lp;
        }
        return null;
    }

    AnimatorConnectPoint FindRightPointAt(Vector3 worldPos)
    {
        AnimatorConnectPoint best = null;
        float bestDist = float.MaxValue;

        foreach (var rp in _rightPoints)
        {
            if (rp.linkedPoint != null) continue;
            float dist = Vector3.Distance(rp.transform.position, worldPos);
            if (dist < hitRadius && dist < bestDist)
            {
                bestDist = dist;
                best = rp;
            }
        }
        return best;
    }

    ConnectionLine CreateDraggingLine(AnimatorConnectPoint from, Vector3 worldStart)
    {
        var line = Instantiate(linePrefab, transform);
        line.SetColor(from.matchColor);
        line.SetStart(worldStart);
        _lines.Add(line);
        return line;
    }

    void DestroyLine(ConnectionLine line)
    {
        if (line == null) return;
        _lines.Remove(line);
        Destroy(line.gameObject);
    }

    void Connect(AnimatorConnectPoint left, AnimatorConnectPoint right, ConnectionLine line)
    {
        left.linkedPoint = right;
        right.linkedPoint = left;

        line.SetStart(left.transform.position);
        line.SnapEnd(right.transform.position);

        _connectedCount++;

        if (_connectedCount >= nodeDataSet.Length)
            Clear();
    }

    void Cleanup()
    {
        foreach (var go in _spawnedObjects) if (go) Destroy(go);
        foreach (var line in _lines) if (line) Destroy(line.gameObject);
        _spawnedObjects.Clear();
        _leftPoints.Clear();
        _rightPoints.Clear();
        _lines.Clear();
        _connectedCount = 0;
        _draggingFrom = null;
        _draggingLine = null;
    }

    Vector3 GetMouseWorld()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 m = new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(m);
    }

    static int[] ShuffledIndices(int count)
    {
        int[] arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}

[System.Serializable]
public class AnimatorConnectData
{
    public string leftText;
    public string rightText;
}