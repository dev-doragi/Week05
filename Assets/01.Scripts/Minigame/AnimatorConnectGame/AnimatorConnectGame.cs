using System.Collections.Generic;
using UnityEngine;

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
    public float startY = 2f;

    [Header("히트 판정 반경 (스크린 픽셀)")]
    public float hitRadius = 40f;

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

    // ── MiniGame override ─────────────────────────────────

    protected override void OnStart()
    {
        Cleanup();
        SpawnNodes();
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

    // ── AnimatorConnectPoint에서 호출 ─────────────────────

    public ConnectionLine CreateDraggingLine(AnimatorConnectPoint from, Vector3 worldStart)
    {
        var line = Instantiate(linePrefab, transform);
        line.SetColor(from.matchColor);
        line.SetStart(worldStart);
        _lines.Add(line);
        return line;
    }

    public void DestroyLine(ConnectionLine line)
    {
        if (line == null) return;
        _lines.Remove(line);
        Destroy(line.gameObject);
    }

    public AnimatorConnectPoint FindPointAt(Vector2 screenPos, AnimatorConnectPoint exclude)
    {
        AnimatorConnectPoint best = null;
        float bestDist = float.MaxValue;

        foreach (var rp in _rightPoints)
        {
            if (rp == exclude || rp.linkedPoint != null) continue;

            Vector2 rpScreen = Camera.main.WorldToScreenPoint(rp.transform.position);
            float dist = Vector2.Distance(rpScreen, screenPos);

            if (dist < hitRadius && dist < bestDist)
            {
                bestDist = dist;
                best = rp;
            }
        }
        return best;
    }

    public void Connect(AnimatorConnectPoint left, AnimatorConnectPoint right, ConnectionLine line)
    {
        left.linkedPoint = right;
        right.linkedPoint = left;

        line.SetStart(left.transform.position);
        line.SnapEnd(right.transform.position);

        _connectedCount++;

        if (_connectedCount >= nodeDataSet.Length)
            Clear();
    }

    // ── 정리 ──────────────────────────────────────────────

    void Cleanup()
    {
        foreach (var go in _spawnedObjects) if (go) Destroy(go);
        foreach (var line in _lines) if (line) Destroy(line.gameObject);
        _spawnedObjects.Clear();
        _leftPoints.Clear();
        _rightPoints.Clear();
        _lines.Clear();
        _connectedCount = 0;
    }

    // ── 유틸 ──────────────────────────────────────────────

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